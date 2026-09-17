namespace BovineLabs.Anchor
{
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class AnchorPanel : Panel, IAnchorPanel
    {
        public VisualElement RootVisualElement => this;

        public string Theme
        {
            get => this.theme;
            set => this.theme = value;
        }

        public string Scale
        {
            get => this.scale;
            set => this.scale = value;
        }
    }
}
