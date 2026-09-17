namespace BovineLabs.Anchor.Editor
{
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.Editor.Inspectors;
    using BovineLabs.Core.Editor.Asset;
    using UnityEditor;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(AnchorSettings))]
    public class AnchorSettingsEditor : ElementEditor
    {
        protected override VisualElement CreateElement(SerializedProperty property)
        {
            return property.name switch
            {
                "actions" => new AssetCreator<AnchorAction>(this.serializedObject, property).Element,
                "animations" => new AssetCreator<AnchorNavAnimation>(this.serializedObject, property).Element,
                _ => CreatePropertyField(property),
            };
        }
    }
}
