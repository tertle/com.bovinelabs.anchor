// <copyright file="AnchorSettingsEditor.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Editor
{
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.Editor.Inspectors;
    using BovineLabs.Core.Editor.ObjectManagement;
    using UnityEditor;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(AnchorSettings))]
    public class AnchorSettingsEditor : ElementEditor
    {
        /// <inheritdoc />
        protected override VisualElement CreateElement(SerializedProperty property)
        {
            return property.name switch
            {
                "actions" => new AssetCreator<AnchorNamedAction>(this.serializedObject, property).Element,
                _ => CreatePropertyField(property),
            };
        }
    }
}