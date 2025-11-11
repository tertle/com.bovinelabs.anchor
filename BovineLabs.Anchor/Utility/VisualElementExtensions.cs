// <copyright file="VisualElementExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.Properties;
    using UnityEngine.UIElements;

    /// <summary>
    /// Extension helpers that simplify common data binding and traversal patterns for UITK elements.
    /// </summary>
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Binds a field in two directions by using the specified property name.
        /// </summary>
        /// <param name="element">Element whose field should be bound.</param>
        /// <param name="field">Field name on the element (for example <c>text</c>).</param>
        /// <param name="property">Property path exposed on the data source.</param>
        public static void SetBindingTwoWay(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding { dataSourcePath = new PropertyPath(property) });
        }

        /// <summary>
        /// Binds a field in two directions while supplying conversion functions between types.
        /// </summary>
        /// <typeparam name="TSource">Data source type being projected into the UI.</typeparam>
        /// <typeparam name="TDestination">UI field type.</typeparam>
        /// <param name="element">Element whose field should be bound.</param>
        /// <param name="field">Field name on the element (for example <c>text</c>).</param>
        /// <param name="property">Property path exposed on the data source.</param>
        /// <param name="sourceToUIConverter">Converter invoked when data flows to the UI.</param>
        /// <param name="uiToSourceConverter">Converter invoked when data flows back to the source.</param>
        public static void SetBindingTwoWay<TSource, TDestination>(
            this VisualElement element, string field, string property, TypeConverter<TSource, TDestination> sourceToUIConverter,
            TypeConverter<TDestination, TSource> uiToSourceConverter)
        {
            var db = new DataBinding { dataSourcePath = new PropertyPath(property) };
            db.sourceToUiConverters.AddConverter(sourceToUIConverter);
            db.uiToSourceConverters.AddConverter(uiToSourceConverter);
            element.SetBinding(field, db);
        }

        /// <summary>
        /// Binds a field from the data source to the UI target.
        /// </summary>
        /// <param name="element">Element whose field should be bound.</param>
        /// <param name="field">Field name on the element (for example <c>text</c>).</param>
        /// <param name="property">Property path exposed on the data source.</param>
        public static void SetBindingToUI(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(property),
            });
        }

        /// <summary>
        /// Binds a field from the data source to the UI target using a converter.
        /// </summary>
        /// <typeparam name="TSource">Data source type being projected into the UI.</typeparam>
        /// <typeparam name="TDestination">UI field type.</typeparam>
        /// <param name="element">Element whose field should be bound.</param>
        /// <param name="field">Field name on the element (for example <c>text</c>).</param>
        /// <param name="property">Property path exposed on the data source.</param>
        /// <param name="converter">Converter invoked when data flows to the UI.</param>
        public static void SetBindingToUI<TSource, TDestination>(
            this VisualElement element, string field, string property, TypeConverter<TSource, TDestination> converter)
        {
            var db = new DataBinding { bindingMode = BindingMode.ToTarget, dataSourcePath = new PropertyPath(property) };
            db.sourceToUiConverters.AddConverter(converter);
            element.SetBinding(field, db);
        }

        /// <summary>
        /// Binds a field so changes propagate from the UI back to the data source.
        /// </summary>
        /// <param name="element">Element whose field should be bound.</param>
        /// <param name="field">Field name on the element (for example <c>text</c>).</param>
        /// <param name="property">Property path exposed on the data source.</param>
        public static void SetBindingFromUI(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding { bindingMode = BindingMode.ToSource, dataSourcePath = new PropertyPath(property) });
        }

        /// <summary>
        /// Binds a field so changes propagate from the UI back to the data source using a converter.
        /// </summary>
        /// <typeparam name="TSource">Data source type.</typeparam>
        /// <typeparam name="TDestination">UI field type.</typeparam>
        /// <param name="element">Element whose field should be bound.</param>
        /// <param name="field">Field name on the element (for example <c>text</c>).</param>
        /// <param name="property">Property path exposed on the data source.</param>
        /// <param name="converter">Converter invoked when data flows back to the source.</param>
        public static void SetBindingFromUI<TSource, TDestination>(
            this VisualElement element, string field, string property, TypeConverter<TSource, TDestination> converter)
        {
            var db = new DataBinding { bindingMode = BindingMode.ToSource, dataSourcePath = new PropertyPath(property) };
            db.uiToSourceConverters.AddConverter(converter);
            element.SetBinding(field, db);
        }

        /// <summary>Recursively sets the picking mode on the element hierarchy.</summary>
        /// <param name="e">Root element whose hierarchy should be updated.</param>
        /// <param name="mode">Picking mode to apply.</param>
        public static void SetPickingModeRecursive(this VisualElement e, PickingMode mode)
        {
            e.pickingMode = mode;
            foreach (var child in e.Children())
            {
                SetPickingModeRecursive(child, mode);
            }
        }

        /// <summary>
        /// Attempts to resolve the data source bound to the element or its ancestors.
        /// </summary>
        /// <typeparam name="T">Type of data source to locate.</typeparam>
        /// <param name="element">Element whose ancestry should be inspected.</param>
        /// <param name="slot">When this method returns, contains the resolved data source.</param>
        /// <returns><c>true</c> when a data source of type <typeparamref name="T"/> is found.</returns>
        public static bool TryResolveDataSource<T>(this VisualElement element, out T slot)
            where T : class
        {
            slot = null;

            if (element == null)
            {
                return false;
            }

            var context = element.GetHierarchicalDataSourceContext();
            var dataSource = context.dataSource;

            if (PropertyContainer.TryGetValue(ref dataSource, context.dataSourcePath, out object obj))
            {
                dataSource = obj;
            }

            slot = dataSource as T;
            return slot != null;
        }
    }
}
