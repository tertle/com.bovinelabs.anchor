namespace BovineLabs.Anchor.Editor
{
    using BovineLabs.Core.Editor.Inspectors;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(UISystemTypes.NavigationComponent))]
    public class NavigationComponentEditor : ElementProperty
    {
        protected override string GetDisplayName(SerializedProperty property)
        {
            var names = property.FindPropertyRelative(nameof(UISystemTypes.NavigationComponent.States));

            return names.arraySize == 0 ? "[Invalid]" : names.GetArrayElementAtIndex(0).stringValue;
        }
    }
}
