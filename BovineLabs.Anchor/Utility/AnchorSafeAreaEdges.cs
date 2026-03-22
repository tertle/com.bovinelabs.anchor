// <copyright file="AnchorSafeAreaEdges.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using BovineLabs.Anchor.Elements;

    /// <summary>
    /// Edge selection used by <see cref="Elements.AnchorSafeArea"/> and related safe-area helpers.
    /// </summary>
    [Flags]
    public enum AnchorSafeAreaEdges
    {
        /// <summary>No safe-area insets are applied.</summary>
        None = 0,

        /// <summary>Apply the top safe-area inset when overlapping the unsafe top band.</summary>
        Top = 1 << 0,

        /// <summary>Apply the bottom safe-area inset when overlapping the unsafe bottom band.</summary>
        Bottom = 1 << 1,

        /// <summary>Apply the left safe-area inset when overlapping the unsafe left band.</summary>
        Left = 1 << 2,

        /// <summary>Apply the right safe-area inset when overlapping the unsafe right band.</summary>
        Right = 1 << 3,

        /// <summary>Apply all safe-area insets.</summary>
        All = Top | Bottom | Left | Right,
    }
}
