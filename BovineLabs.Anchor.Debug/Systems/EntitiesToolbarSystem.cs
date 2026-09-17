namespace BovineLabs.Anchor.Debug.Systems
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Core.Extensions;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    internal partial struct EntitiesToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data> toolbar;

        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data>(ref state, "Entities");
        }

        public void OnStartRunning(ref SystemState state)
        {
            this.toolbar.Load();
        }

        public void OnStopRunning(ref SystemState state)
        {
            this.toolbar.Unload();
        }

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
