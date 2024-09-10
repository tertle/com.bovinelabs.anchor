// <copyright file="EntitiesToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using UnityEngine.UIElements;

    [Transient]
    public class EntitiesToolbarView : VisualElement, IView
    {
        public const string UssClassName = "bl-entities-tab";

        private readonly EntitiesToolbarViewModel viewModel = new();

        public EntitiesToolbarView()
        {
            this.AddToClassList(UssClassName);

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
