namespace BovineLabs.Anchor
{
    using UnityEngine.UIElements;

    public interface IAnchorPanel
    {
        VisualElement RootVisualElement { get; }

        string Theme { get; set; }

        string Scale { get; set; }
    }
}
