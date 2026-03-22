// <copyright file="AnchorScreenMetrics.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Snapshot of the current screen-space metrics Anchor uses to evaluate safe-area layout.
    /// </summary>
    public readonly struct AnchorScreenMetrics : IEquatable<AnchorScreenMetrics>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorScreenMetrics"/> struct.
        /// </summary>
        /// <param name="screenWidth">The current screen width in pixels.</param>
        /// <param name="screenHeight">The current screen height in pixels.</param>
        /// <param name="safeArea">The current safe area in screen-space pixels.</param>
        public AnchorScreenMetrics(int screenWidth, int screenHeight, Rect safeArea)
        {
            this.ScreenWidth = screenWidth;
            this.ScreenHeight = screenHeight;
            this.SafeArea = safeArea;
        }

        /// <summary>
        /// Gets the current screen width in pixels.
        /// </summary>
        public int ScreenWidth { get; }

        /// <summary>
        /// Gets the current screen height in pixels.
        /// </summary>
        public int ScreenHeight { get; }

        /// <summary>
        /// Gets the current safe area in screen-space pixels.
        /// </summary>
        public Rect SafeArea { get; }

        internal static AnchorScreenMetrics Current()
        {
            return new AnchorScreenMetrics(Screen.width, Screen.height, AnchorApp.SafeArea);
        }

        public bool Equals(AnchorScreenMetrics other)
        {
            return this.ScreenWidth == other.ScreenWidth &&
                   this.ScreenHeight == other.ScreenHeight &&
                   this.SafeArea.Equals(other.SafeArea);
        }

        public override bool Equals(object obj)
        {
            return obj is AnchorScreenMetrics other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.ScreenWidth, this.ScreenHeight, this.SafeArea);
        }
    }
}
