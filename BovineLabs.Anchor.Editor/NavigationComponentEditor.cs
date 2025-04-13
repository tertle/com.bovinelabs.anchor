// <copyright file="NavigationComponentEditor.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE
namespace BovineLabs.Anchor.Editor
{
    using BovineLabs.Core.Editor.Inspectors;
    using UnityEditor;

    [CustomPropertyDrawer(typeof(UISystemTypes.NavigationComponent))]
    public class NavigationComponentEditor : ElementProperty
    {
        /// <inheritdoc/>
        protected override string GetDisplayName(SerializedProperty property)
        {
            var names = property.FindPropertyRelative(nameof(UISystemTypes.NavigationComponent.States));

            return names.arraySize == 0 ? "[Invalid]" : names.GetArrayElementAtIndex(0).stringValue;
        }
    }
}
#endif