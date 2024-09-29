// <copyright file="EntitiesToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Binding;
    using Unity.Burst;
    using Unity.Properties;

    public class EntitiesToolbarViewModel : SystemObservableObject<EntitiesToolbarViewModel.Data>
    {
        private Data data;

        public override ref Data Value => ref this.data;

        [CreateProperty]
        public int Entities => this.data.Entities;

        [CreateProperty]
        public int Archetypes => this.data.Archetypes;

        [CreateProperty]
        public int Chunks => this.data.Chunks;

        public struct Data : IBindingObjectNotifyData
        {
            private int entities;
            private int archetypes;
            private int chunks;

            public int Entities
            {
                readonly get => this.entities;
                set => this.SetProperty(ref this.entities, value);
            }

            public int Archetypes
            {
                readonly get => this.archetypes;
                set => this.SetProperty(ref this.archetypes, value);
            }

            public int Chunks
            {
                readonly get => this.chunks;
                set => this.SetProperty(ref this.chunks, value);
            }

            public FunctionPointer<OnPropertyChangedDelegate> Notify { get; set; }
        }
    }
}
#endif
