// <copyright file="SubSceneToolbarSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_SUBSCENE
namespace BovineLabs.Anchor.Debug.Systems
{
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Core;
    using BovineLabs.Core.SubScenes;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Scenes;

    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    public partial struct SubSceneToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<SubSceneToolbarView, SubSceneToolbarViewModel, SubSceneToolbarViewModel.Data> toolbar;

        private NativeList<int> values;
        private NativeList<Entity> subScenes;
        private NativeList<FixedString64Bytes> subScenesBuffer;

        /// <inheritdoc/>
        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<SubSceneToolbarView, SubSceneToolbarViewModel, SubSceneToolbarViewModel.Data>(ref state, "SubScene");

            this.values = new NativeList<int>(16, Allocator.Persistent);
            this.subScenes = new NativeList<Entity>(16, Allocator.Persistent);
            this.subScenesBuffer = new NativeList<FixedString64Bytes>(Allocator.Persistent);
        }

        /// <inheritdoc/>
        public void OnDestroy(ref SystemState state)
        {
            this.values.Dispose();
            this.subScenes.Dispose();
            this.subScenesBuffer.Dispose();
        }

        /// <inheritdoc/>
        public void OnStartRunning(ref SystemState state)
        {
            this.toolbar.Load();
            ref var data = ref this.toolbar.Binding;

            data.SubScenes = new NativeList<FixedString64Bytes>(Allocator.Persistent);
            data.Values = new NativeList<int>(Allocator.Persistent);
        }

        /// <inheritdoc/>
        public void OnStopRunning(ref SystemState state)
        {
            ref var data = ref this.toolbar.Binding;
            data.SubScenes.Dispose();
            data.Values.Dispose();
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

            this.values.Clear();
            this.subScenesBuffer.Clear();
            this.subScenes.Clear();

            foreach (var (_, e) in SystemAPI.Query<RefRO<SceneReference>>().WithEntityAccess())
            {
                var index = this.subScenes.Length;
                this.subScenes.Add(e);

                state.EntityManager.GetName(e, out var name);
                if (name == default)
                {
                    name = e.ToFixedString();
                }

                this.subScenesBuffer.Add(name);

                var loaded = SceneSystem.IsSceneLoaded(state.WorldUnmanaged, e);
                if (loaded)
                {
                    this.values.Add(index);
                }
            }

            ref var data = ref this.toolbar.Binding;

            if (data.SubSceneSelectedChanged)
            {
                data.SubSceneSelectedChanged = false;
                var ecb = SystemAPI.GetSingleton<InstantiateCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
                var resolvedSectionEntities = SystemAPI.GetBufferLookup<ResolvedSectionEntity>(true);

                for (var index = 0; index < this.subScenes.Length; index++)
                {
                    var entity = this.subScenes[index];
                    var isOpen = this.values.Contains(index);

                    if (isOpen == SceneSystem.IsSceneLoaded(state.WorldUnmanaged, entity))
                    {
                        continue;
                    }

                    if (isOpen)
                    {
                        SubSceneUtil.LoadScene(ecb, entity, ref resolvedSectionEntities);
                    }
                    else
                    {
                        SubSceneUtil.UnloadScene(ecb, entity, ref resolvedSectionEntities);
                    }
                }
            }

            if (!this.values.AsArray().ArraysEqual(data.Values.AsArray()))
            {
                data.Values.Clear();
                data.Values.AddRange(this.values.AsArray());
                data.NotifyExplicit($"{nameof(SubSceneToolbarViewModel.Data.Values)}");
            }

            if (!this.subScenesBuffer.AsArray().ArraysEqual(data.SubScenes.AsArray()))
            {
                data.SubScenes.Clear();
                data.SubScenes.AddRange(this.subScenesBuffer.AsArray());
                data.NotifyExplicit($"{nameof(SubSceneToolbarViewModel.Data.SubScenes)}");
            }
        }
    }
}
#endif
