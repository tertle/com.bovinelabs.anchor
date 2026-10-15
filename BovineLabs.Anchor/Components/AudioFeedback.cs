namespace BovineLabs.Anchor.Components
{
    using System;
    using BovineLabs.Anchor.Audio;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    [VisualElementComponent]
    public partial struct AudioFeedback
    {
        [CreateProperty]
        [UxmlAttribute("audio-profile")]
        public string AudioProfile;

        [CreateProperty]
        [UxmlAttribute("hover-audio-mode")]
        public AnchorAudioOverrideMode HoverAudioMode;

        [CreateProperty]
        [UxmlAttribute("hover-audio-clip")]
        public AudioClip HoverAudioClip;

        [CreateProperty]
        [UxmlAttribute("activate-audio-mode")]
        public AnchorAudioOverrideMode ActivateAudioMode;

        [CreateProperty]
        [UxmlAttribute("activate-audio-clip")]
        public AudioClip ActivateAudioClip;

        private ButtonSubscription _subscription;

        [UxmlCreateInstanceMethod]
        public static AudioFeedback Create()
        {
            return new AudioFeedback
            {
                AudioProfile = AnchorAudioSettings.DefaultProfileKey,
            };
        }

        [OnComponentAdded]
        private static void OnAdded(ref AudioFeedback self, VisualElement owner)
        {
            if (owner is Button button)
            {
                self._subscription = new ButtonSubscription(button);
            }
        }

        [OnComponentRemoved]
        private static void OnRemoved(ref AudioFeedback self, VisualElement owner)
        {
            self._subscription?.Dispose();
        }

        [ReleaseComponentResources]
        private static void Release(ref AudioFeedback self)
        {
            self._subscription?.Dispose();
        }

        [RegisterCallback(typeof(PointerEnterEvent))]
        private static void OnPointerEnter(ref AudioFeedback self, VisualElement owner, PointerEnterEvent evt)
        {
            if (!owner.enabledInHierarchy)
            {
                return;
            }

            if (evt.pointerId != PointerId.mousePointerId && evt.pointerId < PointerId.trackedPointerIdBase)
            {
                return;
            }

            AnchorAudio.Play(self.AudioProfile, AnchorAudioCue.Hover, new AnchorAudioCueOverride(self.HoverAudioMode, self.HoverAudioClip));
        }

        [RegisterCallback(typeof(ActionTriggeredEvent))]
        private static void OnActionTriggered(ref AudioFeedback self, VisualElement owner, ActionTriggeredEvent evt)
        {
            if (evt.target == owner)
            {
                PlayActivate(ref self);
            }
        }

        private static void PlayActivate(ref AudioFeedback self)
        {
            AnchorAudio.Play(self.AudioProfile, AnchorAudioCue.Activate, new AnchorAudioCueOverride(self.ActivateAudioMode, self.ActivateAudioClip));
        }

        private sealed class ButtonSubscription : IDisposable
        {
            private Button _button;

            public ButtonSubscription(Button button)
            {
                _button = button;
                _button.clicked += OnActivated;
            }

            public void Dispose()
            {
                if (_button == null)
                {
                    return;
                }

                _button.clicked -= OnActivated;
                _button = null;
            }

            private void OnActivated()
            {
                // An earlier click callback may remove this component during the same invocation.
                var button = _button;
                if (button == null || button.resourcesReleased)
                {
                    return;
                }

                ref var feedback = ref button.GetComponent<AudioFeedback>();
                PlayActivate(ref feedback);
            }
        }
    }
}
