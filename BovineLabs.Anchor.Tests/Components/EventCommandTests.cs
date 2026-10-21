namespace BovineLabs.Anchor.Tests.Components
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.Components;
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
            object received = new object();
            ICommand expected = new RelayCommand<EventBase>(evt =>
            {
                received = evt;
                calls++;
            });
            var component = new EventCommand();

            Assert.IsTrue(PropertyContainer.TrySetValue(ref component, nameof(EventCommand.Command), expected));
            Assert.IsTrue(PropertyContainer.TryGetValue(ref component, nameof(EventCommand.Command), out ICommand actual));
            Assert.AreSame(expected, actual);
            var button = new Button();
            button.AddComponent(component);
            button.clickable.InvokePressed(null);

            Assert.AreEqual(1, calls);
            Assert.IsNull(received);
        }

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
