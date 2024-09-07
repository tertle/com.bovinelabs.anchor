// <copyright file="PhysicsToolbarSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DRAW || UNITY_PHYSICS
namespace BovineLabs.Anchor.Debug.Systems
{
    using BovineLabs.Anchor.Debug;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.Toolbar;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    public partial struct PhysicsToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<PhysicsToolbarView, PhysicsToolbarViewModel, PhysicsToolbarViewModel.Data> toolbar;

        /// <inheritdoc/>
        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<PhysicsToolbarView, PhysicsToolbarViewModel, PhysicsToolbarViewModel.Data>(ref state, "Physics");

#if BL_DRAW
            state.EntityManager.AddComponent<BovineLabs.Draw.Debug.Physics.PhysicsDebugDraw>(state.SystemHandle);
#endif
        }

        /// <inheritdoc/>
        public void OnStartRunning(ref SystemState state)
        {
            this.toolbar.Load();
        }

        /// <inheritdoc/>
        public void OnStopRunning(ref SystemState state)
        {
            this.toolbar.Unload();
        }

        /// <inheritdoc/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!this.toolbar.IsVisible())
            {
                return;
            }

            ref var data = ref this.toolbar.Binding;

#if BL_DRAW
            ref var c = ref state.EntityManager.GetComponentDataRW<BovineLabs.Draw.Debug.Physics.PhysicsDebugDraw>(state.SystemHandle).ValueRW;
            c.DrawColliderEdges = data.DrawColliderEdges;
            c.DrawColliderAabbs = data.DrawColliderAabbs;
            c.DrawCollisionEvents = data.DrawCollisionEvents;
            c.DrawTriggerEvents = data.DrawTriggerEvents;
            c.DrawMeshColliderEdges = data.DrawMeshColliderEdges;
            c.DrawTerrainColliderEdges = data.DrawTerrainColliderEdges;
#else
            if (Unity.Burst.CompilerServices.Hint.Unlikely(!SystemAPI.HasSingleton<Unity.Physics.Authoring.PhysicsDebugDisplayData>()))
            {
                state.EntityManager.CreateSingleton<Unity.Physics.Authoring.PhysicsDebugDisplayData>();
            }

            ref var c = ref SystemAPI.GetSingletonRW<Unity.Physics.Authoring.PhysicsDebugDisplayData>().ValueRW;
            c.DrawColliderEdges = data.DrawColliderEdges ? 1 : 0;
            c.DrawColliderAabbs = data.DrawColliderAabbs ? 1 : 0;
            c.DrawCollisionEvents = data.DrawCollisionEvents ? 1 : 0;
            c.DrawTriggerEvents = data.DrawTriggerEvents ? 1 : 0;
#endif
        }
    }
}
#endif
