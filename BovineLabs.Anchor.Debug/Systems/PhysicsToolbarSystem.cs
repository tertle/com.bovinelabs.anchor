// <copyright file="PhysicsToolbarSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_PHYSICS && !BL_QUILL && UNITY_EDITOR // Default physics doesn't support builds
namespace BovineLabs.Anchor.Debug.Systems
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Debug.Views;
    using Unity.Burst;
    using Unity.Burst.CompilerServices;
    using Unity.Entities;
    using Unity.Physics.Authoring;
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
        }

        /// <inheritdoc />
        public void OnStartRunning(ref SystemState state)
        {
            this.toolbar.Load();

            this.UpdateData(ref state, ref this.toolbar.Binding);
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

            this.UpdateData(ref state, ref this.toolbar.Binding);
        }

        private void UpdateData(ref SystemState state, ref PhysicsToolbarViewModel.Data data)
        {
            if (Hint.Unlikely(!SystemAPI.HasSingleton<PhysicsDebugDisplayData>()))
            {
                state.EntityManager.CreateSingleton<PhysicsDebugDisplayData>();
            }

            var c = SystemAPI.GetSingleton<PhysicsDebugDisplayData>();
            if (c.DrawColliderEdges != (data.DrawColliderEdges ? 1 : 0) || c.DrawColliderAabbs != (data.DrawColliderAabbs ? 1 : 0) ||
                c.DrawCollisionEvents != (data.DrawCollisionEvents ? 1 : 0) || c.DrawTriggerEvents != (data.DrawTriggerEvents ? 1 : 0))
            {
                ref var rw = ref SystemAPI.GetSingletonRW<PhysicsDebugDisplayData>().ValueRW;
                rw.DrawColliderEdges = data.DrawColliderEdges ? 1 : 0;
                rw.DrawColliderAabbs = data.DrawColliderAabbs ? 1 : 0;
                rw.DrawCollisionEvents = data.DrawCollisionEvents ? 1 : 0;
                rw.DrawTriggerEvents = data.DrawTriggerEvents ? 1 : 0;
            }
        }
    }
}
#endif
