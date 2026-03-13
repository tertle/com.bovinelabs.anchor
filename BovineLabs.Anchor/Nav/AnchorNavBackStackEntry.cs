// <copyright file="AnchorNavBackStackEntry.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;

    /// <summary> Representation of an entry in the back stack of a <see cref="NavController"/>. </summary>
    internal sealed class AnchorNavBackStackEntry
    {
        /// <summary> Initializes a new instance of the <see cref="AnchorNavBackStackEntry"/> class. </summary>
        /// <param name="destination"> The destination associated with this entry. </param>
        /// <param name="options"> The options associated with this entry. </param>
        /// <param name="arguments"> The arguments associated with this entry. </param>
        /// <param name="snapshot"> The snapshot of the visual stack for this entry. </param>
        internal AnchorNavBackStackEntry(
            string destination,
            AnchorNavOptions options,
            AnchorNavArgument[] arguments,
            AnchorNavStackSnapshot snapshot = null)
        {
            this.Destination = destination;
            this.Options = options ?? new AnchorNavOptions();
            this.Arguments = arguments ?? Array.Empty<AnchorNavArgument>();
            this.Snapshot = snapshot ?? AnchorNavStackSnapshot.Empty;
        }

        /// <summary> Gets the destination associated with this entry. </summary>
        public string Destination { get; }

        /// <summary> Gets the options associated with this entry. </summary>
        public AnchorNavOptions Options { get; }

        /// <summary> Gets the arguments associated with this entry. </summary>
        public AnchorNavArgument[] Arguments { get; }

        /// <summary> Gets the snapshot of the visual stack for this entry. </summary>
        public AnchorNavStackSnapshot Snapshot { get; }
    }
}
