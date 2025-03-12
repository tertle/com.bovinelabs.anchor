// <copyright file="VisualElementExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.AppUI.Navigation;
    using Unity.Properties;
    using UnityEngine.UIElements;

    public static class VisualElementExtensions
    {
        public static void Navigate(this VisualElement element, string screen)
        {
            element?.FindNavController()?.Navigate(screen);
        }

        public static void PopBackStack(this VisualElement element)
        {
            element?.FindNavController()?.PopBackStack();
        }

        public static void SetBindingTwoWay(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding { dataSourcePath = new PropertyPath(property) });
        }

        public static void SetBindingToUI(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(property),
            });
        }

        public static void SetBindingFromUI(this VisualElement element, string field, string property)
        {
            element.SetBinding(field, new DataBinding
            {
                bindingMode = BindingMode.ToSource,
                dataSourcePath = new PropertyPath(property),
            });
        }
    }
}
