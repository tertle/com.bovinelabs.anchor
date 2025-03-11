// <copyright file="PhysicsToolbarSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_PHYSICS && (BL_QUILL || UNITY_EDITOR) // Default physics doesn't support builds
namespace BovineLabs.Anchor.Debug.Systems
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.Toolbar;
#if BL_QUILL
    using BovineLabs.Quill.Debug.Physics;
#endif
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Physics.Systems;

    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    public partial struct PhysicsToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<PhysicsToolbarView, PhysicsToolbarViewModel, PhysicsToolbarViewModel.Data> toolbar;

        /// <inheritdoc />
        public void OnCreate(ref SystemState state)
        {
            if (state.World.GetExistingSystem<BuildPhysicsWorld>() == SystemHandle.Null)
            {
                state.Enabled = false;
                return;
            }

            this.toolbar = new ToolbarHelper<PhysicsToolbarView, PhysicsToolbarViewModel, PhysicsToolbarViewModel.Data>(ref state, "Physics");

#if BL_QUILL
            state.EntityManager.AddComponent<PhysicsDebugDraw>(state.SystemHandle);
#endif
        }

        /// <inheritdoc />
        public void OnStartRunning(ref SystemState state)
        {
            this.toolbar.Load();
            this.toolbar.Binding.World = state.World.Name.GetHashCode();
        }

        /// <inheritdoc />
        public void OnStopRunning(ref SystemState state)
        {
            this.toolbar.Unload();
        }

        /// <inheritdoc />
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!this.toolbar.IsVisible())
            {
                return;
            }

            ref var data = ref this.toolbar.Binding;

#if BL_QUILL
            var c = SystemAPI.GetSingleton<PhysicsDebugDraw>();
            if (c.DrawColliderEdges != data.DrawColliderEdges || c.DrawColliderAabbs != data.DrawColliderAabbs ||
                c.DrawCollisionEvents != data.DrawCollisionEvents || c.DrawTriggerEvents != data.DrawTriggerEvents ||
                c.DrawMeshColliderEdges != data.DrawMeshColliderEdges || c.DrawTerrainColliderEdges != data.DrawTerrainColliderEdges)
            {
                ref var rw = ref SystemAPI.GetSingletonRW<PhysicsDebugDraw>().ValueRW;
                rw.DrawColliderEdges = data.DrawColliderEdges;
                rw.DrawColliderAabbs = data.DrawColliderAabbs;
                rw.DrawCollisionEvents = data.DrawCollisionEvents;
                rw.DrawTriggerEvents = data.DrawTriggerEvents;
                rw.DrawMeshColliderEdges = data.DrawMeshColliderEdges;
                rw.DrawTerrainColliderEdges = data.DrawTerrainColliderEdges;
            }
#else
            if (Unity.Burst.CompilerServices.Hint.Unlikely(!SystemAPI.HasSingleton<Unity.Physics.Authoring.PhysicsDebugDisplayData>()))
            {
                state.EntityManager.CreateSingleton<Unity.Physics.Authoring.PhysicsDebugDisplayData>();
            }

            var c = SystemAPI.GetSingleton<Unity.Physics.Authoring.PhysicsDebugDisplayData>();
            if (c.DrawColliderEdges != (data.DrawColliderEdges ? 1 : 0) || c.DrawColliderAabbs != (data.DrawColliderAabbs ? 1 : 0) ||
                c.DrawCollisionEvents != (data.DrawCollisionEvents ? 1 : 0) || c.DrawTriggerEvents != (data.DrawTriggerEvents ? 1 : 0))
            {
                ref var rw = ref SystemAPI.GetSingletonRW<Unity.Physics.Authoring.PhysicsDebugDisplayData>().ValueRW;
                rw.DrawColliderEdges = data.DrawColliderEdges ? 1 : 0;
                rw.DrawColliderAabbs = data.DrawColliderAabbs ? 1 : 0;
                rw.DrawCollisionEvents = data.DrawCollisionEvents ? 1 : 0;
                rw.DrawTriggerEvents = data.DrawTriggerEvents ? 1 : 0;
            }
#endif
        }
    }
}
#endif
