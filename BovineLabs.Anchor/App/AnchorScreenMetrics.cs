namespace BovineLabs.Anchor
{
    using System;
    using UnityEngine;

    public readonly struct AnchorScreenMetrics : IEquatable<AnchorScreenMetrics>
    {
        public AnchorScreenMetrics(int screenWidth, int screenHeight, Rect safeArea)
        {
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            SafeArea = safeArea;
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
            return ScreenWidth == other.ScreenWidth &&
                   ScreenHeight == other.ScreenHeight &&
                   SafeArea.Equals(other.SafeArea);
        }

        public override bool Equals(object obj)
        {
            return obj is AnchorScreenMetrics other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ScreenWidth;
                hashCode = (hashCode * 397) ^ ScreenHeight;
                return (hashCode * 397) ^ SafeArea.GetHashCode();
            }
        }
    }
}
