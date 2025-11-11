// <copyright file="View.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using UnityEngine.UIElements;

    /// <summary>
    /// Base class for Anchor visual elements that exposes a strongly typed view model.
    /// </summary>
    [IsService]
    public abstract class View<T> : VisualElement
    {
        protected View(T viewModel)
        {
            this.ViewModel = viewModel;
        }

        /// <summary>Gets the view model instance the view is bound to.</summary>
        public T ViewModel { get; }
    }
}
