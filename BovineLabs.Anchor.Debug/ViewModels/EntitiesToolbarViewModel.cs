// <copyright file="EntitiesToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Attributes;
    using BovineLabs.Anchor.Binding;
    using Unity.Properties;

    public partial class EntitiesToolbarViewModel : SystemObservableObject<EntitiesToolbarViewModel.Data>
    {
        private Data data;

        public override ref Data Value => ref this.data;

        [CreateProperty(ReadOnly = true)]
        public int Entities => this.data.Entities;

        [CreateProperty(ReadOnly = true)]
        public int Archetypes => this.data.Archetypes;

        [CreateProperty(ReadOnly = true)]
        public int Chunks => this.data.Chunks;

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
