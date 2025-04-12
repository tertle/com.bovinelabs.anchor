// <copyright file="View.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using UnityEngine.UIElements;

    [IsService]
    public abstract class View<T> : VisualElement
    {
        protected View(T viewModel)
        {
            this.ViewModel = viewModel;
        }

        public T ViewModel { get; }
    }
}
