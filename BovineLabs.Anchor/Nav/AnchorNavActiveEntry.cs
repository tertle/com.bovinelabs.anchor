namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine.UIElements;

    internal sealed class AnchorNavActiveEntry
    {
        public AnchorNavActiveEntry(string destination, AnchorNavArgument[] arguments, bool isPopup, AnchorNavOptions options, VisualElement element)
        {
            Destination = destination;
            Arguments = arguments ?? Array.Empty<AnchorNavArgument>();
            IsPopup = isPopup;
            Options = options ?? new AnchorNavOptions();
            Element = element ?? throw new ArgumentNullException(nameof(element));
        }

        public string Destination { get; }

        public bool IsPopup { get; }

        public AnchorNavOptions Options { get; private set; }

        public AnchorNavArgument[] Arguments { get; private set; }

        public VisualElement Element { get; }

        public void Update(AnchorNavStackItem item)
        {
            Options = item.Options;
            Arguments = item.Arguments;
        }
    }
}
