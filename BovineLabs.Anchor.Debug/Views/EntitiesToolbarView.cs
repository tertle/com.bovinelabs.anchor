// <copyright file="EntitiesToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class EntitiesToolbarView : VisualElement
    {
        public const string UssClassName = "bl-entities-tab";

        [Preserve]
        public EntitiesToolbarView(EntitiesToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            this.Add(KeyValueGroup.Create(viewModel,
                new[]
                {
                    ("Entities", nameof(EntitiesToolbarViewModel.Entities)),
                    ("Archetypes", nameof(EntitiesToolbarViewModel.Archetypes)),
                    ("Chunks", nameof(EntitiesToolbarViewModel.Chunks)),
                }));
        }
    }
}
