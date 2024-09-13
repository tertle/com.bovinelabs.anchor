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
    public class EntitiesToolbarView : VisualElement, IView<EntitiesToolbarViewModel>
    {
        public const string UssClassName = "bl-entities-tab";

        public EntitiesToolbarView()
        {
            this.AddToClassList(UssClassName);

            this.Add(KeyValueGroup.Create(this.ViewModel, new[]
            {
                ("Entities", nameof(EntitiesToolbarViewModel.Entities)),
                ("Archetypes", nameof(EntitiesToolbarViewModel.Archetypes)),
                ("Chunks", nameof(EntitiesToolbarViewModel.Chunks)),
            }));
        }

        /// <inheritdoc/>
        public EntitiesToolbarViewModel ViewModel { get; } = new();
    }
}
#endif
