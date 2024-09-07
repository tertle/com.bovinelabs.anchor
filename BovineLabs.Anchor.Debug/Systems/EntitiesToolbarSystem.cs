// <copyright file="EntitiesToolbarSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Debug.Systems
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.Toolbar;
    using Unity.Burst;
    using Unity.Entities;

    /// <summary> The toolbar for monitoring the number of entities, chunks and archetypes of a world. </summary>
    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    internal partial struct EntitiesToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<EntitiesToolbarView, EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data> toolbar;

#if !BL_CORE
        private Unity.Collections.NativeList<EntityArchetype> entityArchetypes;
#endif

        /// <inheritdoc/>
        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<EntitiesToolbarView, EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data>(ref state, "Entities");

#if !BL_CORE
            this.entityArchetypes = new Unity.Collections.NativeList<EntityArchetype>(1024, Unity.Collections.Allocator.Persistent);
#endif
        }

#if !BL_CORE
        public void OnDestroy(ref SystemState state)
        {
            this.entityArchetypes.Dispose();
        }
#endif
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
#if BL_CORE
            data.Archetypes = Core.Extensions.EntityManagerExtensions.NumberOfArchetype(state.EntityManager);
#else
            this.entityArchetypes.Clear();
            state.EntityManager.GetAllArchetypes(this.entityArchetypes);
            data.Archetypes = this.entityArchetypes.Length;
#endif
            data.Chunks = state.EntityManager.UniversalQuery.CalculateChunkCountWithoutFiltering();
        }
    }
}
#endif
