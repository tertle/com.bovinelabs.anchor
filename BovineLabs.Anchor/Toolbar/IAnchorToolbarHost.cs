namespace BovineLabs.Anchor.Toolbar
{
    using UnityEngine.UIElements;

    public interface IAnchorToolbarHost
    {
        VisualElement CreateRootVisualElement();

        /// <summary>
        /// Releases visuals while retaining durable toolbar registrations and models.
        /// </summary>
        void ReleaseRootVisualElement();
    }
}
