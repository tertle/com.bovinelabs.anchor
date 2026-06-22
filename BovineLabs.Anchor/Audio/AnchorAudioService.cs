// <copyright file="AnchorAudioService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using UnityEngine;
    using Object = UnityEngine.Object;

    internal sealed class AnchorAudioService : IAnchorAudioService, IDisposable
    {
        private GameObject host;
        private AudioSource source;

        internal AudioSource Source => this.source;

        /// <inheritdoc />
        public void PlayOneShot(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            this.EnsureSource().PlayOneShot(clip);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.host != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(this.host);
                }
                else
                {
                    Object.DestroyImmediate(this.host);
                }
            }

            this.host = null;
            this.source = null;
        }

        private AudioSource EnsureSource()
        {
            if (this.source != null)
            {
                return this.source;
            }

            this.host = new GameObject("Anchor UI Audio");
            this.source = this.host.AddComponent<AudioSource>();
            this.source.playOnAwake = false;
            this.source.spatialBlend = 0f;
            this.source.loop = false;
            return this.source;
        }
    }
}
