// <copyright file="AnchorNavAction.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using Unity.AppUI.Navigation;
    using UnityEngine;

    /// <summary>
    /// Description of a navigation action.
    /// </summary>
    [Serializable]
    public class AnchorNavAction
    {
        [SerializeField]
        private string destination = string.Empty;

        [SerializeField]
        private AnchorNavOptions options = new();

        [SerializeField]
        private List<Argument> defaultArguments = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavAction"/> class.
        /// </summary>
        public AnchorNavAction()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorNavAction"/> class.
        /// </summary>
        /// <param name="destination">Destination identifier.</param>
        /// <param name="options">Navigation options.</param>
        /// <param name="defaultArguments">Default arguments to merge when navigating.</param>
        public AnchorNavAction(string destination, AnchorNavOptions options, IEnumerable<Argument> defaultArguments = null)
        {
            this.destination = destination;
            this.options = options ?? new AnchorNavOptions();
            this.defaultArguments = defaultArguments != null ? new List<Argument>(defaultArguments) : new List<Argument>();
        }

        /// <summary> Gets the destination that should be navigated to when this action is used. </summary>
        public string Destination => this.destination;

        /// <summary> Gets the navigation options associated with this action. </summary>
        public AnchorNavOptions Options => this.options;

        /// <summary> Gets the default arguments associated with this action. </summary>
        public IList<Argument> DefaultArguments => this.defaultArguments;

        /// <summary> Merge the default arguments with the provided arguments. </summary>
        /// <param name="arguments"> Arguments to merge with defaults.</param>
        /// <returns> The merged arguments.</returns>
        public Argument[] MergeArguments(params Argument[] arguments)
        {
            var mergedArguments = this.defaultArguments != null
                ? new List<Argument>(this.defaultArguments)
                : new List<Argument>();

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
