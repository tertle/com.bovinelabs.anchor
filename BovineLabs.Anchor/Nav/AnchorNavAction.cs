// <copyright file="AnchorNavAction.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
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
        private List<AnchorNavArgument> defaultArguments = new();

        [SerializeField]
        private AnchorNavOptions options = new();

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
        public AnchorNavAction(string destination, AnchorNavOptions options, IEnumerable<AnchorNavArgument> defaultArguments = null)
        {
            this.destination = destination;
            this.options = options != null ? options.Clone() : new AnchorNavOptions();
            this.defaultArguments = defaultArguments != null ? new List<AnchorNavArgument>(defaultArguments) : new List<AnchorNavArgument>();
        }

        /// <summary> Gets or sets the destination that should be navigated to when this action is used. </summary>
        public string Destination
        {
            get => this.destination;
            set => this.destination = value;
        }

        /// <summary> Gets the navigation options associated with this action. </summary>
        public AnchorNavOptions Options
        {
            get => this.options ??= new AnchorNavOptions();
            set => this.options = value ?? new AnchorNavOptions();
        }

        /// <summary> Gets the default arguments associated with this action. </summary>
        public IList<AnchorNavArgument> DefaultArguments
        {
            get => this.defaultArguments ??= new List<AnchorNavArgument>();
            set => this.defaultArguments = value != null ? new List<AnchorNavArgument>(value) : new List<AnchorNavArgument>();
        }

        /// <summary> Merge the default arguments with the provided arguments. </summary>
        /// <param name="arguments"> Arguments to merge with defaults.</param>
        /// <returns> The merged arguments.</returns>
        public AnchorNavArgument[] MergeArguments(params AnchorNavArgument[] arguments)
        {
            arguments ??= Array.Empty<AnchorNavArgument>();
            var mergedArguments = new List<AnchorNavArgument>(this.DefaultArguments);

            foreach (var arg in arguments)
            {
                if (arg == null)
                {
                    continue;
                }

                var existingArgIdx = mergedArguments.FindIndex(a => a.Name == arg.Name);
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
