// <copyright file="EntitiesToolbarSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Debug.ToolbarTabs
{
    using BovineLabs.Anchor.Debug.ToolbarTabs.ViewModels;
    using BovineLabs.Anchor.Debug.ToolbarTabs.Views;
    using BovineLabs.Core.Extensions;
    using Unity.Burst;
    using Unity.Entities;

    /// <summary> The toolbar for monitoring the number of entities, chunks and archetypes of a world. </summary>
    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    internal partial struct EntitiesToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<EntitiesToolbarView, EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data> toolbar;

        /// <inheritdoc/>
        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<EntitiesToolbarView, EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data>(ref state, "Entities");
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
            data.Entities = state.EntityManager.UniversalQuery.CalculateEntityCountWithoutFiltering();
            data.Archetypes = state.EntityManager.NumberOfArchetype();
            data.Chunks = state.EntityManager.UniversalQuery.CalculateChunkCountWithoutFiltering();
        }
    }
}
#endif
