// <copyright file="AnchorNavOptionsEditor.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Editor
{
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.Editor.Inspectors;
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    [CustomPropertyDrawer(typeof(AnchorNavOptions))]
    public class AnchorNavOptionsEditor : ElementProperty
    {
        /// <inheritdoc/>
        protected override VisualElement CreateElement(SerializedProperty property)
        {
            var cache = this.Cache<Cache>();

            switch (property.name)
            {
                case "stackStrategy":
                    cache.StackStrategyProperty = property;
                    return cache.StackStrategyField = CreatePropertyField(property);

                case "popupToDestination":
                    return cache.PopupToDestinationField = CreatePropertyField(property);
            }

            return base.CreateElement(property);
        }

        /// <inheritdoc/>
        protected override void PostElementCreation(VisualElement root, bool createdElements)
        {
            var cache = this.Cache<Cache>();
            cache.StackStrategyField.RegisterValueChangeCallback(_ => UpdateStackStrategy(cache));

            UpdateStackStrategy(cache);
        }

        private static void UpdateStackStrategy(Cache cache)
        {
            ElementUtility.SetVisible(cache.PopupToDestinationField, cache.StackStrategyProperty.enumValueIndex == 1);
        }

        private class Cache
        {
            public SerializedProperty StackStrategyProperty;

            public PropertyField StackStrategyField;
            public PropertyField PopupToDestinationField;
        }
    }
}
