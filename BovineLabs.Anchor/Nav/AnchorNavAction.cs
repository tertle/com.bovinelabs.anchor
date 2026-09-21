namespace BovineLabs.Anchor.Nav
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class AnchorNavAction
    {
        [SerializeField]
        private string _destination = string.Empty;

        [SerializeField]
        private List<AnchorNavArgument> _defaultArguments = new();

        [SerializeField]
        private AnchorNavOptions _options = new();

        public AnchorNavAction()
        {
        }

        public AnchorNavAction(string destination, AnchorNavOptions options, IEnumerable<AnchorNavArgument> defaultArguments = null)
        {
            _destination = destination;
            _options = options != null ? options.Clone() : new AnchorNavOptions();
            _defaultArguments = defaultArguments != null ? new List<AnchorNavArgument>(defaultArguments) : new List<AnchorNavArgument>();
        }

        public string Destination
        {
            get => _destination;
            set => _destination = value;
        }

        public AnchorNavOptions Options
        {
            get => _options ??= new AnchorNavOptions();
            set => _options = value ?? new AnchorNavOptions();
        }

        public IList<AnchorNavArgument> DefaultArguments
        {
            get => _defaultArguments ??= new List<AnchorNavArgument>();
            set => _defaultArguments = value != null ? new List<AnchorNavArgument>(value) : new List<AnchorNavArgument>();
        }

        public AnchorNavArgument[] MergeArguments(AnchorNavArgument[] arguments = null)
        {
            arguments ??= Array.Empty<AnchorNavArgument>();
            var mergedArguments = new List<AnchorNavArgument>(DefaultArguments);

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
