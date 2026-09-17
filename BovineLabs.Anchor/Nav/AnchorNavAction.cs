namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class AnchorNavAction
    {
        [SerializeField]
        private string destination = string.Empty;

        [SerializeField]
        private List<AnchorNavArgument> defaultArguments = new();

        [SerializeField]
        private AnchorNavOptions options = new();

        public AnchorNavAction()
        {
        }

        public AnchorNavAction(string destination, AnchorNavOptions options, IEnumerable<AnchorNavArgument> defaultArguments = null)
        {
            this.destination = destination;
            this.options = options != null ? options.Clone() : new AnchorNavOptions();
            this.defaultArguments = defaultArguments != null ? new List<AnchorNavArgument>(defaultArguments) : new List<AnchorNavArgument>();
        }

        public string Destination
        {
            get => this.destination;
            set => this.destination = value;
        }

        public AnchorNavOptions Options
        {
            get => this.options ??= new AnchorNavOptions();
            set => this.options = value ?? new AnchorNavOptions();
        }

        public IList<AnchorNavArgument> DefaultArguments
        {
            get => this.defaultArguments ??= new List<AnchorNavArgument>();
            set => this.defaultArguments = value != null ? new List<AnchorNavArgument>(value) : new List<AnchorNavArgument>();
        }

        public AnchorNavArgument[] MergeArguments(AnchorNavArgument[] arguments = null)
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
