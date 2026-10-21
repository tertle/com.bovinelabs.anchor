namespace BovineLabs.Anchor.Components
{
    using System;
    using System.Windows.Input;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    [VisualElementComponent]
    public partial struct EventCommand
    {
        [CreateProperty]
        public ICommand Command;

        private ButtonSubscription _subscription;

        [OnComponentAdded]
        private static void OnAdded(ref EventCommand self, VisualElement owner)
        {
            if (owner is Button button)
            {
                self._subscription = new ButtonSubscription(button);
            }
        }

        [OnComponentRemoved]
        private static void OnRemoved(ref EventCommand self, VisualElement owner)
        {
            self._subscription?.Dispose();
        }

        [ReleaseComponentResources]
        private static void Release(ref EventCommand self)
        {
            self._subscription?.Dispose();
        }

        [RegisterCallback(typeof(ActionTriggeredEvent))]
        private static void OnActionTriggered(ref EventCommand self, VisualElement owner, ActionTriggeredEvent evt)
        {
            if (evt.target == owner)
            {
                self.Command?.Execute(evt);
            }
        }

        private sealed class ButtonSubscription : IDisposable
        {
            private readonly Pressable _pressable;
            private Button _button;

            public ButtonSubscription(Button button)
            {
                _button = button;
                _pressable = button.clickable;
                _pressable.clickedWithEventInfo += OnActivated;
            }

            public void Dispose()
            {
                if (_button == null)
                {
                    return;
                }

                _pressable.clickedWithEventInfo -= OnActivated;
                _button = null;
            }

            private void OnActivated(EventBase evt)
            {
                // An earlier callback may remove this component during the same invocation.
                var button = _button;
                if (button == null || button.resourcesReleased)
                {
                    return;
                }

                ref var command = ref button.GetComponent<EventCommand>();
                command.Command?.Execute(evt);
            }
        }
    }
}
