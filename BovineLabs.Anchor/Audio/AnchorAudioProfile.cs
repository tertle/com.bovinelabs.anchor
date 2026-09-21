namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class AnchorAudioProfile
    {
        [HideInInspector]
        [SerializeField]
        private string _key = string.Empty;

        [SerializeField]
        private AudioClip _hoverClip;

        [SerializeField]
        private AudioClip _activateClip;

        public string Key
        {
            get => _key;
            set => _key = value ?? string.Empty;
        }

        public AudioClip HoverClip
        {
            get => _hoverClip;
            set => _hoverClip = value;
        }

        public AudioClip ActivateClip
        {
            get => _activateClip;
            set => _activateClip = value;
        }

        internal AudioClip GetClip(AnchorAudioCue cue)
        {
            return cue switch
            {
                AnchorAudioCue.Hover => _hoverClip,
                AnchorAudioCue.Activate => _activateClip,
                _ => null,
            };
        }
    }
}
