// <copyright file="PhysicsToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DRAW || UNITY_PHYSICS
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.Binding;
    using Unity.Properties;

    public class PhysicsToolbarViewModel : BindableObservableObject, IBindingObject<PhysicsToolbarViewModel.Data>
    {
        private Data data;

        public ref Data Value => ref this.data;

        [CreateProperty]
        public bool DrawColliderEdges
        {
            get => this.data.DrawColliderEdges;
            set => this.SetProperty(ref this.data.DrawColliderEdges, value);
        }

        [CreateProperty]
        public bool DrawColliderAabbs
        {
            get => this.data.DrawColliderAabbs;
            set => this.SetProperty(ref this.data.DrawColliderAabbs, value);
        }

        [CreateProperty]
        public bool DrawCollisionEvents
        {
            get => this.data.DrawCollisionEvents;
            set => this.SetProperty(ref this.data.DrawCollisionEvents, value);
        }

        [CreateProperty]
        public bool DrawTriggerEvents
        {
            get => this.data.DrawTriggerEvents;
            set => this.SetProperty(ref this.data.DrawTriggerEvents, value);
        }

#if BL_DRAW
        [CreateProperty]
        public bool DrawMeshColliderEdges
        {
            get => this.data.DrawMeshColliderEdges;
            set => this.SetProperty(ref this.data.DrawMeshColliderEdges, value);
        }

        [CreateProperty]
        public bool DrawTerrainColliderEdges
        {
            get => this.data.DrawTerrainColliderEdges;
            set => this.SetProperty(ref this.data.DrawTerrainColliderEdges, value);
        }
#endif

        public struct Data : IBindingObject
        {
            public bool DrawColliderEdges;
            public bool DrawColliderAabbs;
            public bool DrawCollisionEvents;
            public bool DrawTriggerEvents;
#if BL_DRAW
            public bool DrawMeshColliderEdges;
            public bool DrawTerrainColliderEdges;
#endif
        }
    }
}
#endif
