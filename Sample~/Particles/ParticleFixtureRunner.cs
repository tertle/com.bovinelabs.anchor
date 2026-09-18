namespace BovineLabs.Anchor.Particles.Sample
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Profiling;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class ParticleFixtureRunner : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset fixture;
        [SerializeField] private ThemeStyleSheet theme;
        private readonly List<ParticleFixtureElement> controls = new();
        private readonly List<ProfilerRecorder> recorders = new();
        private readonly Texture2D[] textures = new Texture2D[4];
        private UIDocument document;
        private UIDocument secondDocument;
        private PanelSettings settings;
        private PanelSettings secondSettings;
        private int resizeStep;

        private static readonly string[] MarkerNames =
        {
            "Anchor.Particles.MeshFill", "Anchor.Particles.MeshAllocate", "Anchor.Particles.MeshSubmit", "Anchor.Particles.PanelTick",
            "RenderTreeManager.Process", "Main Thread", "Render Thread", "GC Allocated In Frame", "Batches Count", "Total Used Memory",
            "Anchor.Particles.LifecycleBatch100",
        };

        private static readonly ProfilerMarker LifecycleMarker = new("Anchor.Particles.LifecycleBatch100");

        private void Awake()
        {
            this.settings = ScriptableObject.CreateInstance<PanelSettings>();
            this.settings.themeStyleSheet = this.theme;
            this.settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            this.settings.clearColor = true;
            this.settings.colorClearValue = new Color(0.04f, 0.05f, 0.08f, 1);
            this.document = this.gameObject.AddComponent<UIDocument>();
            this.document.panelSettings = this.settings;
            this.document.visualTreeAsset = this.fixture;
            for (var t = 0; t < this.textures.Length; t++)
            {
                var size = t == 0 ? 2 : 128;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Point;
                var colors = new Color[size * size];
                for (var y = 0; y < size; y++)
                {
                    for (var x = 0; x < size; x++)
                    {
                        colors[(y * size) + x] = y < size / 2 ? (x < size / 2 ? Color.blue : Color.white) :
                            (x < size / 2 ? Color.red : Color.green);
                    }
                }

                texture.SetPixels(colors);
                texture.Apply(false, true);
                this.textures[t] = texture;
            }

            this.BindFixture();
        }

        private void BindFixture()
        {
            var root = this.document.rootVisualElement;
            root.Q("preview-step").style.display = DisplayStyle.None;
            root.Q<ParticleFixtureElement>("uv").Texture = this.textures[0];
            root.Q("transformed").style.rotate = new Rotate(Angle.Degrees(15));
            root.Q("transformed").style.scale = new Scale(new Vector3(0.8f, 1.1f, 1));
            root.Q<Button>("clear").clicked += () =>
            {
                var element = root.Q<ParticleFixtureElement>("siblings");
                element.Count = element.Count > 1 ? 1 : element.Count == 1 ? 0 : 12;
            };
            root.Q<Button>("rebuild").clicked += () =>
            {
                root.Clear();
                this.fixture.CloneTree(root);
                this.BindFixture();
            };
            root.Q<Button>("detach").clicked += () =>
            {
                var element = root.Q<ParticleFixtureElement>("siblings");
                var parent = element.parent;
                var index = parent.IndexOf(element);
                element.RemoveFromHierarchy();
                parent.Insert(index, element);
            };
            root.Q<Button>("scale").clicked += () => this.settings.scale = this.settings.scale == 1 ? 1.25f : 1;
            root.Q<Button>("resize").clicked += () =>
            {
                this.resizeStep = (this.resizeStep + 1) % 3;
                root.Q("cases").style.width = this.resizeStep == 0 ? StyleKeyword.Auto : this.resizeStep == 1 ? 650 : 0;
            };
            root.Q<Button>("opacity").clicked += () =>
                root.Q("faded").style.opacity = root.Q("faded").resolvedStyle.opacity < 1 ? 1 : 0.5f;
        }

        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            var argument = Array.IndexOf(args, "--particle-results");
            if (argument < 0)
            {
                yield break;
            }

            var directory = Path.GetFullPath(args[argument + 1]);
            Directory.CreateDirectory(directory);
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            Application.runInBackground = true;
            for (var f = 0; f < 120; f++)
            {
                yield return null;
            }

            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "visual-baseline.png"));
            yield return this.CheckLifecycle(directory);
            var shader = Shader.Find("Hidden/Internal-UIRDefault");
            if (shader != null)
            {
                var material = new Material(shader);
                var element = this.document.rootVisualElement.Q<ParticleFixtureElement>("siblings");
                element.style.unityMaterial = material;
                yield return null;
                yield return null;
                ScreenCapture.CaptureScreenshot(Path.Combine(directory, "visual-material-probe.png"));
                File.WriteAllText(Path.Combine(directory, "material-probe.txt"),
                    $"Element material property accepted. Shader: {shader.name}. " +
                    $"Exposes _SrcBlend: {material.HasProperty("_SrcBlend")}; _DstBlend: {material.HasProperty("_DstBlend")}. " +
                    "No custom shader was authored; additive rendering is not established by this probe.");
                element.style.unityMaterial = StyleKeyword.Null;
                Destroy(material);
            }
            else
            {
                File.WriteAllText(Path.Combine(directory, "material-probe.txt"), "Default UI shader unavailable by public name; optional probe unsupported.");
            }

            this.document.rootVisualElement.Q("fixture").RemoveFromHierarchy();
            File.WriteAllText(Path.Combine(directory, "environment.json"), JsonUtility.ToJson(new EnvironmentRecord(), true));
            using var csv = new StreamWriter(Path.Combine(directory, "frames.csv"));
            csv.Write("workload,run,frame,controls,quads,vertexBytes,indexBytes,inputBytes,frameMs,cpuMs,gpuMs");
            foreach (var marker in MarkerNames)
            {
                csv.Write("," + marker);
            }

            csv.WriteLine();
            foreach (var workload in Workloads)
            {
                for (var run = 0; run < 5; run++)
                {
                    this.Configure(workload);
                    for (var frame = 0; frame < 120; frame++)
                    {
                        yield return null;
                    }

                    foreach (var marker in MarkerNames)
                    {
                        this.recorders.Add(new ProfilerRecorder(marker, 1,
                            ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.SumAllSamplesInFrame));
                    }

                    var samples = new double[600, 6 + MarkerNames.Length];
                    var timing = new FrameTiming[1];
                    var previous = Time.realtimeSinceStartupAsDouble;
                    for (var frame = 0; frame < 600; frame++)
                    {
                        if (workload.Lifecycle)
                        {
                            using (LifecycleMarker.Auto())
                            {
                                var surface = this.document.rootVisualElement.Q("benchmark");
                                foreach (var control in this.controls)
                                {
                                    control.Count = 0;
                                    control.RemoveFromHierarchy();
                                    surface.Add(control);
                                    control.Count = 10;
                                }
                            }
                        }

                        FrameTimingManager.CaptureFrameTimings();
                        yield return null;
                        var now = Time.realtimeSinceStartupAsDouble;
                        samples[frame, 0] = (now - previous) * 1000;
                        previous = now;
                        var available = FrameTimingManager.GetLatestTimings(1, timing) > 0;
                        samples[frame, 1] = available && timing[0].cpuFrameTime > 0 ? timing[0].cpuFrameTime : -1;
                        samples[frame, 2] = available && timing[0].gpuFrameTime > 0 ? timing[0].gpuFrameTime : -1;
                        long bytes = 0;
                        var liveQuads = 0;
                        var attached = 0;
                        foreach (var control in this.controls)
                        {
                            bytes += control.NativeBytes;
                            liveQuads += control.Count;
                            attached += control.panel != null ? 1 : 0;
                        }

                        samples[frame, 3] = bytes;
                        samples[frame, 4] = liveQuads;
                        samples[frame, 5] = attached;
                        for (var m = 0; m < this.recorders.Count; m++)
                        {
                            var recorder = this.recorders[m];
                            samples[frame, 6 + m] = recorder.Valid ? recorder.LastValue : -1;
                        }
                    }

                    for (var frame = 0; frame < 600; frame++)
                    {
                        csv.Write($"{workload.Name},{run},{frame},{samples[frame, 5]},{samples[frame, 4]}," +
                            $"{samples[frame, 4] * 4L * UnsafeUtility.SizeOf<Vertex>()},{samples[frame, 4] * 12L},{samples[frame, 3]},");
                        csv.Write(samples[frame, 0].ToString("R", CultureInfo.InvariantCulture));
                        csv.Write("," + samples[frame, 1].ToString("R", CultureInfo.InvariantCulture));
                        csv.Write("," + samples[frame, 2].ToString("R", CultureInfo.InvariantCulture));
                        for (var m = 0; m < MarkerNames.Length; m++)
                        {
                            csv.Write("," + samples[frame, 6 + m].ToString("R", CultureInfo.InvariantCulture));
                        }

                        csv.WriteLine();
                    }

                    csv.Flush();
                    foreach (var recorder in this.recorders)
                    {
                        recorder.Dispose();
                    }

                    this.recorders.Clear();
                }
            }

            File.WriteAllText(Path.Combine(directory, "complete.txt"), "All workloads: five runs, 120 warmup + 600 measured frames each.");
            Application.Quit();
        }

        private IEnumerator CheckLifecycle(string directory)
        {
            var element = this.document.rootVisualElement.Q<ParticleFixtureElement>("siblings");
            var parent = element.parent;
            var index = parent.IndexOf(element);
            element.Count = 1;
            yield return null;
            element.Count = 0;
            yield return null;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "visual-cleared.png"));
            var ticks = element.TickCount;
            for (var f = 0; f < 5; f++)
            {
                yield return null;
            }

            var idle = ticks == element.TickCount;
            element.RemoveFromHierarchy();
            var released = element.NativeBytes == 0;
            parent.Insert(index, element);
            element.Count = 12;
            yield return null;
            this.settings.scale = 1.25f;
            yield return null;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "visual-scaled.png"));
            this.settings.scale = 1;
            element.style.width = 0;
            element.style.height = 0;
            yield return null;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "visual-zero-size.png"));
            this.document.rootVisualElement.Clear();
            var regeneratedReleased = element.NativeBytes == 0;
            this.fixture.CloneTree(this.document.rootVisualElement);
            this.BindFixture();
            yield return null;
            yield return null;
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, "visual-regenerated.png"));
            File.WriteAllText(Path.Combine(directory, "lifecycle.txt"),
                $"Idle stops ticking: {idle}\nDetach releases native input: {released}\nRegeneration releases old input: {regeneratedReleased}\n");
            if (!idle || !released || !regeneratedReleased)
            {
                throw new InvalidOperationException("Particle fixture lifecycle failed; inspect lifecycle.txt.");
            }
        }

        private void Configure(Workload workload)
        {
            var root = this.document.rootVisualElement;
            var surface = root.Q("benchmark");
            surface.style.display = DisplayStyle.Flex;
            surface.EnableInClassList("masked", workload.Masked);
            surface.Clear();
            this.controls.Clear();
            if (this.secondDocument != null)
            {
                Destroy(this.secondDocument.gameObject);
                Destroy(this.secondSettings);
                this.secondDocument = null;
            }

            if (workload.TwoPanels)
            {
                this.secondSettings = Instantiate(this.settings);
                this.secondSettings.clearColor = false;
                this.secondSettings.sortingOrder = 1;
                this.secondDocument = new GameObject("Second particle panel").AddComponent<UIDocument>();
                this.secondDocument.panelSettings = this.secondSettings;
                this.secondDocument.visualTreeAsset = this.fixture;
                this.secondDocument.rootVisualElement.Q("fixture").RemoveFromHierarchy();
                this.secondDocument.rootVisualElement.Q("benchmark").style.display = DisplayStyle.Flex;
            }

            for (var c = 0; c < workload.Controls; c++)
            {
                var element = new ParticleFixtureElement
                {
                    Count = (workload.Quads / workload.Controls) + (c < workload.Quads % workload.Controls ? 1 : 0),
                    QuadSize = workload.Large ? 600 : 8,
                    Overlap = workload.Large,
                    Texture = this.textures[workload.ManyTextures ? c % this.textures.Length : 0],
                    Paused = workload.Paused,
                    Opaque = !workload.Large,
                };
                element.AddToClassList("benchmark-control");
                if (workload.Hidden)
                {
                    element.style.display = DisplayStyle.None;
                }

                if (workload.TwoPanels && c % 2 == 1)
                {
                    this.secondDocument.rootVisualElement.Q("benchmark").Add(element);
                }
                else
                {
                    surface.Add(element);
                }

                this.controls.Add(element);
            }
        }

        private void OnDestroy()
        {
            foreach (var recorder in this.recorders)
            {
                recorder.Dispose();
            }

            // UIDocument can already have detached its tree when component destruction reaches the runner.
            if (this.document != null)
            {
                this.document.rootVisualElement?.Clear();
            }
            if (this.secondDocument != null)
            {
                Destroy(this.secondDocument.gameObject);
                Destroy(this.secondSettings);
            }

            Destroy(this.settings);
            foreach (var texture in this.textures)
            {
                Destroy(texture);
            }
        }

        private readonly struct Workload
        {
            public readonly string Name;
            public readonly int Controls;
            public readonly int Quads;
            public readonly bool Large;
            public readonly bool ManyTextures;
            public readonly bool Masked;
            public readonly bool TwoPanels;
            public readonly bool Paused;
            public readonly bool Hidden;
            public readonly bool Lifecycle;

            public Workload(string name, int controls, int quads, bool large = false, bool manyTextures = false,
                bool masked = false, bool twoPanels = false, bool paused = false, bool hidden = false, bool lifecycle = false)
            {
                this.Name = name;
                this.Controls = controls;
                this.Quads = quads;
                this.Large = large;
                this.ManyTextures = manyTextures;
                this.Masked = masked;
                this.TwoPanels = twoPanels;
                this.Paused = paused;
                this.Hidden = hidden;
                this.Lifecycle = lifecycle;
            }
        }

        [NoAutoStaticsCleanup]
        private static readonly Workload[] Workloads =
        {
            new("empty", 0, 0), new("idle-100", 100, 0), new("paused-100", 100, 1000, paused: true),
            new("hidden-100", 100, 1000, hidden: true), new("quads-64", 1, 64), new("quads-256", 1, 256),
            new("quads-1000", 1, 1000), new("quads-5000", 1, 5000), new("quads-20000", 1, 20000),
            new("1000-on-32", 32, 1000), new("1000-on-100", 100, 1000), new("5000-on-32", 32, 5000), new("5000-on-100", 100, 5000),
            new("large-overlap", 1, 1000, large: true), new("atlas-32", 32, 1000), new("textures-32", 32, 1000, manyTextures: true),
            new("masked-32", 32, 1000, masked: true), new("two-panels", 32, 1000, twoPanels: true),
            new("lifecycle-100", 100, 1000, lifecycle: true),
        };

        [Serializable]
        private sealed class EnvironmentRecord
        {
            public string unity = Application.unityVersion;
            public string os = SystemInfo.operatingSystem;
            public string cpu = SystemInfo.processorType;
            public int cores = SystemInfo.processorCount;
            public int systemMemoryMB = SystemInfo.systemMemorySize;
            public string gpu = SystemInfo.graphicsDeviceName;
            public string graphicsAPI = SystemInfo.graphicsDeviceType.ToString();
            public string driver = SystemInfo.graphicsDeviceVersion;
            public int width = Screen.width;
            public int height = Screen.height;
            public float dpi = Screen.dpi;
            public int vsync = QualitySettings.vSyncCount;
            public int frameCap = Application.targetFrameRate;
            public bool development = Debug.isDebugBuild;
            public string unsupported = "-1 means unavailable. Marker times are nanoseconds; memory counters are bytes; batches are counts.";
        }
    }
}
