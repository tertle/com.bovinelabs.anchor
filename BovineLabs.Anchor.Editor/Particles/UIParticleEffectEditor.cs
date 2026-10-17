namespace BovineLabs.Anchor.Editor.Particles
{
    using System;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Particles;
    using BovineLabs.Core.Editor.Inspectors;
    using UnityEditor;
    using UnityEditor.Compilation;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(UIParticleEffect)), CanEditMultipleObjects]
    public sealed class UIParticleEffectEditor : ElementEditor
    {
        protected override void PostElementCreation(VisualElement root, bool createdElements)
        {
            var feedback = new HelpBox(string.Empty, HelpBoxMessageType.Info);
            root.Add(feedback);
            AnchorParticles preview = null;
            VisualElement source = null;
            VisualElement destination = null;
            var hasFlights = false;
            IVisualElementScheduledItem clock = null;
            var lastTime = 0d;
            var running = false;
            void Stop()
            {
                running = false;
                clock?.Pause();
                preview?.Release();
            }

            void Validate()
            {
                long capacity = 0;
                hasFlights = false;
                try
                {
                    foreach (var item in targets)
                    {
                        var compiled = new UIParticleCompiledEffect((UIParticleEffect)item);
                        compiled.Retain();
                        capacity += compiled.Capacity;
                        hasFlights |= compiled.FlightCapacity != 0;
                        compiled.Release();
                    }

                    feedback.text = $"Ordered alpha emitters • {capacity:N0} total capacity across selection. " +
                        (hasFlights ? "Panel-space target flights; drag S (source) and T (target) in the preview." :
                            "UI units; sampled size/color curves; normalized texture UV rectangles.");
                    feedback.messageType = HelpBoxMessageType.Info;
                }
                catch (ArgumentException exception)
                {
                    Stop();
                    feedback.text = exception.Message;
                    feedback.messageType = HelpBoxMessageType.Error;
                }
                catch (OverflowException)
                {
                    Stop();
                    feedback.text = "Combined emitter capacity exceeds the supported integer range.";
                    feedback.messageType = HelpBoxMessageType.Error;
                }
            }

            Validate();
            void BindPreview()
            {
                preview.SimulationSpace = hasFlights ? UIParticleSpace.Panel : UIParticleSpace.Local;
                preview.Target = destination;
                preview.TargetPoint = new Vector2(10, 10);
                preview.SetSourcePoint(source, new Vector2(10, 10));
                destination.style.display = hasFlights ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!MultiEditing)
            {
                var asset = (UIParticleEffect)target;
                var tools = new VisualElement();
                tools.style.flexDirection = FlexDirection.Row;
                root.Add(tools);
                var seed = new UnsignedIntegerField("Seed")
                {
                    value = 1,
                };
                root.Add(seed);
                var background = new ColorField("Preview background")
                {
                    value = new Color(0.08f, 0.08f, 0.1f),
                };
                root.Add(background);
                preview = new AnchorParticles
                {
                    Effect = asset,
                    PlayOnAttach = false,
                    ManualClock = true,
                };
                preview.style.height = 220;
                preview.style.backgroundColor = background.value;
                preview.style.overflow = Overflow.Hidden;
                root.Add(preview);
                source = new Label("S");
                destination = new Label("T");
                foreach (var handle in new[] { source, destination })
                {
                    handle.style.position = Position.Absolute;
                    handle.style.width = 20;
                    handle.style.height = 20;
                    handle.style.top = 100;
                    handle.style.unityTextAlign = TextAnchor.MiddleCenter;
                    handle.style.backgroundColor = new Color(0.2f, 0.35f, 0.5f);
                    handle.style.color = Color.white;
                    handle.AddManipulator(new UIParticlePreviewDrag());
                    preview.Add(handle);
                }

                source.style.left = Length.Percent(25);
                destination.style.left = Length.Percent(75);
                destination.style.display = hasFlights ? DisplayStyle.Flex : DisplayStyle.None;
                background.RegisterValueChangedCallback(evt => preview.style.backgroundColor = evt.newValue);
                clock = preview.schedule.Execute(() =>
                {
                    var now = EditorApplication.timeSinceStartup;
                    preview.Advance(now - lastTime);
                    lastTime = now;
                    if (!preview.IsPlaying)
                    {
                        running = false;
                        clock.Pause();
                    }
                }).Every(16);
                clock.Pause();
                void Start(bool restart)
                {
                    Validate();
                    if (feedback.messageType == HelpBoxMessageType.Error)
                    {
                        return;
                    }

                    BindPreview();
                    if (restart || !preview.IsPlaying)
                    {
                        preview.Play(seed.value);
                    }
                    else
                    {
                        preview.Resume();
                    }

                    lastTime = EditorApplication.timeSinceStartup;
                    running = true;
                    clock.Resume();
                }

                tools.Add(new Button(() => Start(false))
                {
                    text = "Play",
                });
                tools.Add(new Button(() =>
                {
                    preview.Pause();
                    running = false;
                    clock.Pause();
                })
                {
                    text = "Pause",
                });
                tools.Add(new Button(() => Start(true))
                {
                    text = "Restart",
                });
                tools.Add(new Button(() =>
                {
                    Validate();
                    if (feedback.messageType == HelpBoxMessageType.Error || asset.Emitters.Count == 0)
                    {
                        return;
                    }

                    BindPreview();
                    var emitter = hasFlights ? asset.Emitters.FindIndex(static settings => settings.Flight.Enabled) : 0;
                    preview.Emit(emitter, 16, source, new Vector2(10, 10));
                    lastTime = EditorApplication.timeSinceStartup;
                    running = true;
                    clock.Resume();
                })
                {
                    text = "Emit 16",
                    tooltip = "Join the first flight emitter (or emitter zero) without restarting existing particles.",
                });
                tools.Add(new Button(Stop)
                {
                    text = "Clear",
                });
                void CompilationStarted(object context) => Stop();
                void PlayModeChanged(PlayModeStateChange state) => Stop();
                root.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    CompilationPipeline.compilationStarted += CompilationStarted;
                    EditorApplication.playModeStateChanged += PlayModeChanged;
                });
                root.RegisterCallback<DetachFromPanelEvent>(_ =>
                {
                    Stop();
                    CompilationPipeline.compilationStarted -= CompilationStarted;
                    EditorApplication.playModeStateChanged -= PlayModeChanged;
                });
            }

            root.TrackSerializedObjectValue(serializedObject, _ =>
            {
                var restart = running;
                Stop();
                Validate();
                if (restart && feedback.messageType != HelpBoxMessageType.Error)
                {
                    BindPreview();
                    preview.Play();
                    lastTime = EditorApplication.timeSinceStartup;
                    running = true;
                    clock.Resume();
                }
            });
        }
    }
}
