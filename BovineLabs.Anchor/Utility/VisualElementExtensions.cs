// <copyright file="VisualElementExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.Properties;
    using UnityEngine.UIElements;

    public static class VisualElementExtensions
    {
        public static void SetBindingTwoWay(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding { dataSourcePath = new PropertyPath(property) });
        }

        public static void SetBindingTwoWay<TSource, TDestination>(
            this VisualElement element, string field, string property, TypeConverter<TSource, TDestination> sourceToUIConverter,
            TypeConverter<TDestination, TSource> uiToSourceConverter)
        {
            var db = new DataBinding { dataSourcePath = new PropertyPath(property) };
            db.sourceToUiConverters.AddConverter(sourceToUIConverter);
            db.uiToSourceConverters.AddConverter(uiToSourceConverter);
            element.SetBinding(field, db);
        }

        public static void SetBindingToUI(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(property),
            });
        }

        public static void SetBindingToUI<TSource, TDestination>(
            this VisualElement element, string field, string property, TypeConverter<TSource, TDestination> converter)
        {
            var db = new DataBinding { bindingMode = BindingMode.ToTarget, dataSourcePath = new PropertyPath(property) };
            db.sourceToUiConverters.AddConverter(converter);
            element.SetBinding(field, db);
        }

        public static void SetBindingFromUI(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding { bindingMode = BindingMode.ToSource, dataSourcePath = new PropertyPath(property) });
        }

        public static void SetBindingFromUI<TSource, TDestination>(
            this VisualElement element, string field, string property, TypeConverter<TSource, TDestination> converter)
        {
            var db = new DataBinding { bindingMode = BindingMode.ToSource, dataSourcePath = new PropertyPath(property) };
            db.uiToSourceConverters.AddConverter(converter);
            element.SetBinding(field, db);
        }

        public static void SetPickingModeRecursive(this VisualElement e, PickingMode mode)
        {
            e.pickingMode = mode;
            foreach (var child in e.Children())
            {
                SetPickingModeRecursive(child, mode);
            }
        }
    }
}
