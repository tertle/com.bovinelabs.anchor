// <copyright file="EntitiesToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Elements;

    [Transient]
    public class EntitiesToolbarView : View<EntitiesToolbarViewModel>
    {
        public const string UssClassName = "bl-entities-tab";

        public EntitiesToolbarView()
            : base(new EntitiesToolbarViewModel())
        {
            this.AddToClassList(UssClassName);

            this.Add(KeyValueGroup.Create(this.ViewModel,
                new[]
                {
                    ("Entities", nameof(EntitiesToolbarViewModel.Entities)),
                    ("Archetypes", nameof(EntitiesToolbarViewModel.Archetypes)),
                    ("Chunks", nameof(EntitiesToolbarViewModel.Chunks)),
                }));
        }
    }
}
#endif
