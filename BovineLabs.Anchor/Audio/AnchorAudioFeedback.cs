// <copyright file="AnchorAudioFeedback.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine.UIElements;

    /// <summary>
    /// Explicit API for playing Anchor UI audio feedback for a specific element.
    /// </summary>
    public sealed class AnchorAudioFeedback
    {
        private readonly AnchorAudioProfileResolver resolver;
        private readonly IAnchorAudioService audioService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorAudioFeedback"/> class.
        /// </summary>
        /// <param name="serviceProvider">The app service provider.</param>
        /// <param name="audioService">The playback service.</param>
        public AnchorAudioFeedback(IServiceProvider serviceProvider, IAnchorAudioService audioService)
            : this(
                serviceProvider?.GetService(typeof(AnchorAudioProfileResolver)) as AnchorAudioProfileResolver ?? new AnchorAudioProfileResolver(),
                audioService)
        {
        }

        internal AnchorAudioFeedback(AnchorAudioProfileResolver resolver, IAnchorAudioService audioService)
        {
            this.resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            this.audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        }

        /// <summary>
        /// Plays the resolved cue for an element.
        /// </summary>
        /// <param name="element">The element whose metadata should resolve the cue.</param>
        /// <param name="cue">The cue to play.</param>
        public void Play(VisualElement element, AnchorAudioCue cue)
        {
            var clip = this.resolver.ResolveClip(element, cue);
            if (clip != null)
            {
                this.audioService.PlayOneShot(clip);
            }
        }
    }
}
