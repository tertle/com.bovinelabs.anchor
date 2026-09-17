namespace BovineLabs.Anchor.Debug.Toolbar
{
    using UnityEngine.UIElements;

    public interface IToolbarElement
    {
        /// <summary>
        /// Return a fresh visual element that has never been attached to a toolbar.
        /// </summary>
        VisualElement CreateElement();
    }
}
