// <copyright file="VisualElementExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.AppUI.Navigation;
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
    }
}
