// <copyright file="AnchorAudio.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using System.Runtime.CompilerServices;
    using UnityEngine.UIElements;

    /// <summary>
    /// Attached audio metadata helpers for UI Toolkit elements that do not implement Anchor audio interfaces.
    /// </summary>
    public static class AnchorAudio
    {
        /// <summary>The standard Anchor UI audio profile key.</summary>
        public const string DefaultProfileKey = "default";

        private static readonly ConditionalWeakTable<VisualElement, AnchorAudioOptions> Options = new();

        /// <summary>
        /// Sets runtime audio options on a visual element.
        /// </summary>
        /// <param name="element">The element to configure.</param>
        /// <param name="options">The options to attach. Passing null clears existing options.</param>
        public static void SetOptions(VisualElement element, AnchorAudioOptions options)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            Options.Remove(element);
            if (options != null)
            {
                Options.Add(element, options);
            }
        }

        /// <summary>
        /// Clears runtime audio options from a visual element.
        /// </summary>
        /// <param name="element">The element to clear.</param>
        public static void ClearOptions(VisualElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            Options.Remove(element);
        }

        internal static bool TryGetOptions(VisualElement element, out AnchorAudioOptions options)
        {
            options = null;
            return element != null && Options.TryGetValue(element, out options);
        }
    }
}
