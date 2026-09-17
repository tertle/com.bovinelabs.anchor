namespace BovineLabs.Anchor
{
    using UnityEngine.UIElements;

    /// <summary>
    /// Panel contract used by Anchor so core runtime does not depend on AppUI types.
    /// </summary>
    public interface IAnchorPanel
    {
        /// <summary>Gets the root visual element hosted by the panel.</summary>
        VisualElement RootVisualElement { get; }

        /// <summary>Gets or sets the panel theme identifier.</summary>
        string Theme { get; set; }

        /// <summary>Gets or sets the panel scale identifier.</summary>
        string Scale { get; set; }
    }
}
