// <copyright file="EntitiesToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using BovineLabs.Anchor.Toolbar;
    using UnityEngine.UIElements;
    using KeyValueElement = BovineLabs.Anchor.Elements.KeyValueElement;

    public class EntitiesToolbarView : VisualElement, IView
    {
        private readonly EntitiesToolbarViewModel viewModel = new();

        public EntitiesToolbarView()
        {
            this.Add(KeyValueGroup.Create(this.viewModel, new[]
            {
                ("Entities", nameof(EntitiesToolbarViewModel.Entities)),
                ("Archetypes", nameof(EntitiesToolbarViewModel.Archetypes)),
                ("Chunks", nameof(EntitiesToolbarViewModel.Chunks)),
            }));
        }

        object IView.ViewModel => this.viewModel;
    }
}
#endif
