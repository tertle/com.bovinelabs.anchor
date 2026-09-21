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
                "_actions" => new AssetCreator<AnchorAction>(serializedObject, property).Element,
                "_animations" => new AssetCreator<AnchorNavAnimation>(serializedObject, property).Element,
                _ => CreatePropertyField(property),
            };
        }
    }
}
