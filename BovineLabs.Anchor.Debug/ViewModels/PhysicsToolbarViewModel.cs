// <copyright file="PhysicsToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_PHYSICS
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System;
    using Unity.Properties;
    using UnityEngine;

    [Serializable]
    public partial class PhysicsToolbarViewModel : SystemObservableObject<PhysicsToolbarViewModel.Data>
    {
        [CreateProperty]
        public bool DrawColliderEdges
        {
            get => this.Value.DrawColliderEdges;
            set => this.Value.DrawColliderEdges = value;
        }

        [CreateProperty]
        public bool DrawColliderAabbs
        {
            get => this.Value.DrawColliderAabbs;
            set => this.Value.DrawColliderAabbs = value;
        }

        [CreateProperty]
        public bool DrawCollisionEvents
        {
            get => this.Value.DrawCollisionEvents;
            set => this.Value.DrawCollisionEvents = value;
        }

        [CreateProperty]
        public bool DrawTriggerEvents
        {
            get => this.Value.DrawTriggerEvents;
            set => this.Value.DrawTriggerEvents = value;
        }

        [CreateProperty]
        public bool DrawMeshColliderEdges
        {
            get => this.Value.DrawMeshColliderEdges;
            set => this.Value.DrawMeshColliderEdges = value;
        }

        [CreateProperty]
        public bool DrawTerrainColliderEdges
        {
            get => this.Value.DrawTerrainColliderEdges;
            set => this.Value.DrawTerrainColliderEdges = value;
        }

        [Serializable]
        public partial struct Data
        {
            [SerializeField]
            [SystemProperty]
            private bool drawColliderEdges;

            [SerializeField]
            [SystemProperty]
            private bool drawColliderAabbs;

            [SerializeField]
            [SystemProperty]
            private bool drawCollisionEvents;

            [SerializeField]
            [SystemProperty]
            private bool drawTriggerEvents;

            [SerializeField]
            [SystemProperty]
            private bool drawMeshColliderEdges;

            [SerializeField]
            [SystemProperty]
            private bool drawTerrainColliderEdges;
        }
    }
}
#endif
