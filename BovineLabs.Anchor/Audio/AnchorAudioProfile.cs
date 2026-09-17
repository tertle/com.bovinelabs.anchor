namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class AnchorAudioProfile
    {
        [HideInInspector]
        [SerializeField]
        private string key = string.Empty;

        [SerializeField]
        private AudioClip hoverClip;

        [SerializeField]
        private AudioClip activateClip;

        public string Key
        {
            get => this.key;
            set => this.key = value ?? string.Empty;
        }

        public AudioClip HoverClip
        {
            get => this.hoverClip;
            set => this.hoverClip = value;
        }

        public AudioClip ActivateClip
        {
            get => this.activateClip;
            set => this.activateClip = value;
        }

        internal AudioClip GetClip(AnchorAudioCue cue)
        {
            return cue switch
            {
                AnchorAudioCue.Hover => this.hoverClip,
                AnchorAudioCue.Activate => this.activateClip,
                _ => null,
            };
        }
    }
}
