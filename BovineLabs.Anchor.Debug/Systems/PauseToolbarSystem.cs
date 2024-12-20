// <copyright file="PauseToolbarSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_PAUSE
namespace BovineLabs.Anchor.Debug.Systems
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core.Pause;
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Networking.Transport;

    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    public partial struct PauseToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<PauseToolbarView, PauseToolbarViewModel, PauseToolbarViewModel.Data> toolbar;

        /// <inheritdoc />
        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<PauseToolbarView, PauseToolbarViewModel, PauseToolbarViewModel.Data>(ref state, "Pause");
        }

        /// <inheritdoc />
        public void OnStartRunning(ref SystemState state)
        {
            this.toolbar.Load();
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

            var isPaused = state.EntityManager.HasComponent<PauseGame>(state.SystemHandle);

            if (isPaused)
            {
                if (!this.toolbar.Binding.Pause)
                {
                    state.EntityManager.RemoveComponent<PauseGame>(state.SystemHandle);
                }
            }
            else
            {
                if (this.toolbar.Binding.Pause)
                {
                    state.EntityManager.AddComponent<PauseGame>(state.SystemHandle);
                }
            }
        }
    }
}
#endif
