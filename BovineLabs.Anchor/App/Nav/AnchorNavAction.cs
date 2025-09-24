// <copyright file="AnchorNavAction.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System.Collections.Generic;
    using Unity.AppUI.Navigation;

    public record AnchorNavAction(string Destination, AnchorNavOptions Options, List<Argument> DefaultArguments = null)
    {
        /// <summary> Gets the ID of the destination that should be navigated to when this action is used. </summary>
        public string Destination { get; } = Destination;

        /// <summary> Gets the NavOptions to be used by default when navigating to this action. </summary>
        public AnchorNavOptions Options { get; } = Options;

        /// <summary> Gets the default arguments to be used when navigating to this action. </summary>
        public List<Argument> DefaultArguments { get; } = DefaultArguments;

        /// <summary> Merge the default arguments with the provided arguments. </summary>
        /// <param name="arguments"> The arguments to merge with the default arguments.</param>
        /// <returns> The merged arguments.</returns>
        public Argument[] MergeArguments(params Argument[] arguments)
        {
            var mergedArguments = this.DefaultArguments != null ? new List<Argument>(this.DefaultArguments) : new List<Argument>();
            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    continue;
                }

                var existingArgIdx = mergedArguments.FindIndex(a => a.name == arg.name);
                if (existingArgIdx >= 0)
                {
                    mergedArguments[existingArgIdx] = arg;
                }
                else
                {
                    mergedArguments.Add(arg);
                }
            }

            return mergedArguments.ToArray();
        }
    }
}
