namespace BovineLabs.Anchor
{
    using System;
    using UnityEngine;

    public readonly struct AnchorScreenMetrics : IEquatable<AnchorScreenMetrics>
    {
        public AnchorScreenMetrics(int screenWidth, int screenHeight, Rect safeArea)
        {
            this.ScreenWidth = screenWidth;
            this.ScreenHeight = screenHeight;
            this.SafeArea = safeArea;
        }

        public int ScreenWidth { get; }

        public int ScreenHeight { get; }

        /// <summary>
        /// Screen-space pixels.
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
            unchecked
            {
                var hashCode = this.ScreenWidth;
                hashCode = (hashCode * 397) ^ this.ScreenHeight;
                return (hashCode * 397) ^ this.SafeArea.GetHashCode();
            }
        }
    }
}
