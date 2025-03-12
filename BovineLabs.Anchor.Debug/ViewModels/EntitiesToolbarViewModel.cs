// <copyright file="EntitiesToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Contracts;
    using Unity.Properties;

    public partial class EntitiesToolbarViewModel : SystemObservableObject<EntitiesToolbarViewModel.Data>
    {
        [CreateProperty(ReadOnly = true)]
        public int Entities => this.Value.Entities;

        [CreateProperty(ReadOnly = true)]
        public int Archetypes => this.Value.Archetypes;

        [CreateProperty(ReadOnly = true)]
        public int Chunks => this.Value.Chunks;

        public partial struct Data
        {
            [SystemProperty]
            private int entities;

            [SystemProperty]
            private int archetypes;

            [SystemProperty]
            private int chunks;
        }
    }
}
#endif
