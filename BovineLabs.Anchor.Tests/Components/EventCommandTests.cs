namespace BovineLabs.Anchor.Tests.Components
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Components;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using NUnit.Framework;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    public class EventCommandTests
    {
        [Test]
        public void Command_PropertyBagRoundTrip_DrivesActivation()
        {
            var calls = 0;
            ICommand expected = new RelayCommand<EventBase>(_ => calls++);
            var component = new EventCommand();

            Assert.IsTrue(PropertyContainer.TrySetValue(ref component, nameof(EventCommand.Command), expected));
            Assert.IsTrue(PropertyContainer.TryGetValue(ref component, nameof(EventCommand.Command), out ICommand actual));
            Assert.AreSame(expected, actual);
            var button = new Button();
            button.AddComponent(component);
            button.clickable.InvokePressed(null);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void PulseEnabled_PropertyBagRoundTrip_PreservesConfiguredValue()
        {
            var component = Pulse.Create();

            Assert.IsTrue(PropertyContainer.TrySetValue(ref component, nameof(Pulse.Enabled), true));
            Assert.IsTrue(PropertyContainer.TryGetValue(ref component, nameof(Pulse.Enabled), out bool enabled));

            Assert.IsTrue(enabled);
            Assert.IsTrue(component.Enabled);
        }

        [Test]
        public void SafeAreaEdges_PropertyBagRoundTrip_PreservesConfiguredValue()
        {
            var component = SafeArea.Create();
            const AnchorSafeAreaEdges expected = AnchorSafeAreaEdges.Top | AnchorSafeAreaEdges.Left;

            Assert.IsTrue(PropertyContainer.TrySetValue(ref component, nameof(SafeArea.Edges), expected));
            Assert.IsTrue(PropertyContainer.TryGetValue(ref component, nameof(SafeArea.Edges), out AnchorSafeAreaEdges actual));

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, component.Edges);
        }

        [Test]
        public void TimerIntervalAndCommand_PropertyBagRoundTrip_PreservesConfiguredValues()
        {
            var component = new Timer();
            ICommand expected = new RelayCommand(() => { });

            Assert.IsTrue(PropertyContainer.TrySetValue(ref component, nameof(Timer.Interval), 250L));
            Assert.IsTrue(PropertyContainer.TrySetValue(ref component, nameof(Timer.Command), expected));
            Assert.IsTrue(PropertyContainer.TryGetValue(ref component, nameof(Timer.Interval), out long interval));
            Assert.IsTrue(PropertyContainer.TryGetValue(ref component, nameof(Timer.Command), out ICommand command));

            Assert.AreEqual(250L, interval);
            Assert.AreSame(expected, command);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Button_PointerOrKeyboardActivation_PassesOriginalEvent(bool keyboard)
        {
            EventBase received = null;
            var button = new Button();
            button.AddComponent(new EventCommand
            {
                Command = new RelayCommand<EventBase>(evt => received = evt),
            });
            using EventBase evt = keyboard ? KeyUpEvent.GetPooled() : PointerUpEvent.GetPooled();
            evt.target = button;

            button.clickable.InvokePressed(evt);

            Assert.AreSame(evt, received);
            Assert.AreSame(button, received.target);
        }

        [Test]
        public void Button_ProgrammaticActivation_PassesNullEventOnce()
        {
            var calls = 0;
            object received = new object();
            var button = new Button();
            button.AddComponent(new EventCommand
            {
                Command = new RelayCommand<EventBase>(evt =>
                {
                    calls++;
                    received = evt;
                }),
            });

            button.clickable.InvokePressed(null);

            Assert.AreEqual(1, calls);
            Assert.IsNull(received);
        }

        [Test]
        public void Button_Activation_UsesCurrentCommand()
        {
            var calls = new List<string>();
            var button = new Button();
            button.AddComponent(new EventCommand
            {
                Command = new RelayCommand<EventBase>(_ => calls.Add("original")),
            });
            button.clickable.InvokePressed(null);

            ref var command = ref button.GetComponent<EventCommand>();
            command.Command = new RelayCommand<EventBase>(_ => calls.Add("updated"));
            button.NotifyComponentChanged(new BindingId("${component:BovineLabs.Anchor.Components.EventCommand}.Command"));
            button.clickable.InvokePressed(null);

            CollectionAssert.AreEqual(new[] { "original", "updated" }, calls);
        }

        [Test]
        public void Button_RemoveThenReadd_StopsOldSubscriptionAndExecutesOnce()
        {
            var calls = 0;
            var button = new Button();
            var command = new RelayCommand<EventBase>(_ => calls++);
            button.AddComponent(new EventCommand { Command = command });
            button.RemoveComponent<EventCommand>();

            button.clickable.InvokePressed(null);
            Assert.AreEqual(0, calls);

            button.AddComponent(new EventCommand { Command = command });
            button.clickable.InvokePressed(null);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void Button_EarlierCallbackRemovesComponent_DoesNotExecuteRemovedCommand()
        {
            var calls = 0;
            var button = new Button();
            button.clickable.clickedWithEventInfo += _ => button.RemoveComponent<EventCommand>();
            button.AddComponent(new EventCommand
            {
                Command = new RelayCommand<EventBase>(_ => calls++),
            });

            button.clickable.InvokePressed(null);

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void Button_MissingCommand_AllowsActivation()
        {
            var button = new Button();
            button.GetOrAddComponent<EventCommand>();

            Assert.DoesNotThrow(() => button.clickable.InvokePressed(null));
        }

        [Test]
        public void Button_WithAudioAndBothCommands_PreservesActivationOrder()
        {
            var calls = new List<string>();
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(typeof(IAudioService), new RecordingAudioService(calls));
            using var provider = services.BuildServiceProvider();
            using var app = new AnchorApp();
            app.Initialize(provider);
            var button = new Button();
            button.clickable.command = new RelayCommand(() => calls.Add("normal-command"));
            button.AddComponent(new EventCommand
            {
                Command = new RelayCommand<EventBase>(_ => calls.Add("event-command")),
            });
            button.GetOrAddComponent<AudioFeedback>();

            button.clickable.InvokePressed(null);

            CollectionAssert.AreEqual(new[] { "normal-command", "audio", "event-command" }, calls);
        }

        private sealed class RecordingAudioService : IAudioService
        {
            private readonly List<string> _calls;

            public RecordingAudioService(List<string> calls)
            {
                _calls = calls;
            }

            public void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
            {
                _calls.Add("audio");
            }
        }
    }
}
