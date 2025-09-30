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
        protected override ParentTypes ParentType => ParentTypes.None;

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

                case "popupStrategy":
                    cache.PopupStrategyProperty = property;
                    return cache.PopupStrategyField = CreatePropertyField(property);

                case "popupBaseDestination":
                    return cache.PopupBaseDestinationField = CreatePropertyField(property);

                case "popupBaseArguments":
                    return cache.PopupBaseArgumentsField = CreatePropertyField(property);

                case "popupExistingStrategy":
                    return cache.PopupExistingStrategyField = CreatePropertyField(property);
            }

            return base.CreateElement(property);
        }

        /// <inheritdoc/>
        protected override void PostElementCreation(VisualElement root, bool createdElements)
        {
            var cache = this.Cache<Cache>();
            cache.StackStrategyField.RegisterValueChangeCallback(_ => UpdateStackStrategy(cache));
            cache.PopupStrategyField.RegisterValueChangeCallback(_ => UpdatePopupStrategy(cache));

            UpdateStackStrategy(cache);
            UpdatePopupStrategy(cache);
        }

        private static void UpdateStackStrategy(Cache cache)
        {
            ElementUtility.SetVisible(cache.PopupToDestinationField, cache.StackStrategyProperty.enumValueIndex == (int)AnchorStackStrategy.PopToSpecificDestination);
        }

        private static void UpdatePopupStrategy(Cache cache)
        {
            var showBaseFields = cache.PopupStrategyProperty.enumValueIndex == (int)AnchorPopupStrategy.EnsureBaseAndPopup;
            ElementUtility.SetVisible(cache.PopupBaseDestinationField, showBaseFields);
            ElementUtility.SetVisible(cache.PopupBaseArgumentsField, showBaseFields);

            var showExistingStrategy = cache.PopupStrategyProperty.enumValueIndex != (int)AnchorPopupStrategy.None;
            ElementUtility.SetVisible(cache.PopupExistingStrategyField, showExistingStrategy);
        }

        private class Cache
        {
            public SerializedProperty StackStrategyProperty;
            public SerializedProperty PopupStrategyProperty;

            public PropertyField StackStrategyField;
            public PropertyField PopupStrategyField;
            public PropertyField PopupToDestinationField;
            public PropertyField PopupExistingStrategyField;
            public PropertyField PopupBaseDestinationField;
            public PropertyField PopupBaseArgumentsField;
        }
    }
}
