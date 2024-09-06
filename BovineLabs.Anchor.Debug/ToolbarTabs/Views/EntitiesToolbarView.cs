// <copyright file="EntitiesToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if !BL_DISABLE_TOOLBAR
namespace BovineLabs.Anchor.Debug.ToolbarTabs.Views
{
    using BovineLabs.Anchor.Debug.ToolbarTabs.ViewModels;
    using UnityEngine.UIElements;
    using KeyValueElement = BovineLabs.Anchor.Toolbar.KeyValueElement;

    public class EntitiesToolbarView : VisualElement, IView
    {
        private readonly EntitiesToolbarViewModel viewModel = new();

        public EntitiesToolbarView()
        {
            this.Add(KeyValueElement.Create(this.viewModel, "Entities", nameof(EntitiesToolbarViewModel.Entities)));
            this.Add(KeyValueElement.Create(this.viewModel, "Archetypes", nameof(EntitiesToolbarViewModel.Archetypes)));
            this.Add(KeyValueElement.Create(this.viewModel, "Chunks", nameof(EntitiesToolbarViewModel.Chunks)));
        }

        object IView.ViewModel => this.viewModel;
    }
}
#endif
