// <copyright file="AnchorNavBackStackEntry.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using Unity.AppUI.Navigation;

    /// <summary> Representation of an entry in the back stack of a <see cref="NavController"/>. </summary>
    /// <param name="Destination"> The destination associated with this entry. </param>
    /// <param name="Options"> The options associated with this entry. </param>
    /// <param name="Arguments"> The arguments associated with this entry. </param>
    public record AnchorNavBackStackEntry(string Destination, AnchorNavOptions Options, Argument[] Arguments)
    {
        /// <summary> The destination associated with this entry. </summary>
        public string Destination { get; } = Destination;

        /// <summary> The options associated with this entry. </summary>
        public AnchorNavOptions Options { get; } = Options;

        /// <summary> The arguments associated with this entry. </summary>
        public Argument[] Arguments { get; } = Arguments;
    }
}
