namespace BovineLabs.Anchor
{
    using Unity.AppUI.UI;
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

        /// <summary>
        /// Releases an element and its descendants. App UI pressables keep their command subscription after release,
        /// and a released element throws when a later CanExecuteChanged updates it, so their commands are cleared first.
        /// </summary>
        public static void ReleaseResourcesRecursive(this VisualElement element)
        {
            element.Query<VisualElement>().ForEach(static child =>
            {
                if (child is IPressable pressable)
                {
                    pressable.clickable.command = null;
                }
            });

            element.hierarchy.Clear(VisualElementClearOptions.RecursiveReleaseResources);
            element.ReleaseResources();
        }

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
