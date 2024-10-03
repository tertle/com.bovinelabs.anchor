// <copyright file="SubSceneToolbarSystem.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_SUBSCENE
namespace BovineLabs.Anchor.Debug.Systems
{
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.Debug.ViewModels;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core.SubScenes;
    using BovineLabs.Core.Utility;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Entities;

    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    public partial struct SubSceneToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<SubSceneToolbarView, SubSceneToolbarViewModel, SubSceneToolbarViewModel.Data> toolbar;

        private NativeList<int> values;
        private NativeList<SubSceneToolbarViewModel.Data.SubSceneName> subScenesBuffer;

        /// <inheritdoc/>
        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<SubSceneToolbarView, SubSceneToolbarViewModel, SubSceneToolbarViewModel.Data>(ref state, "SubScene");

            this.values = new NativeList<int>(16, Allocator.Persistent);
            this.subScenesBuffer = new NativeList<SubSceneToolbarViewModel.Data.SubSceneName>(Allocator.Persistent);
        }

        /// <inheritdoc/>
        public void OnDestroy(ref SystemState state)
        {
            this.values.Dispose();
            this.subScenesBuffer.Dispose();
        }

        /// <inheritdoc/>
        public void OnStartRunning(ref SystemState state)
        {
            this.toolbar.Load();
            ref var data = ref this.toolbar.Binding;

            data.SubScenes = new NativeList<SubSceneToolbarViewModel.Data.SubSceneName>(Allocator.Persistent);
            data.SubSceneValues = new NativeList<int>(Allocator.Persistent);
        }

        /// <inheritdoc/>
        public void OnStopRunning(ref SystemState state)
        {
            ref var data = ref this.toolbar.Binding;
            data.SubScenes.Dispose();
            data.SubSceneValues.Dispose();
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

            if (data.SubSceneSelectedChanged)
            {
                data.SubSceneSelectedChanged = false;

                for (var index = 0; index < data.SubScenes.Length; index++)
                {
                    var subScene = data.SubScenes[index];
                    var isOpen = data.SubSceneValues.Contains(index);
                    var isCurrentlyOpen = SubSceneUtil.IsLoadingOrLoaded(ref state, subScene.Entity);

                    if (isOpen == isCurrentlyOpen)
                    {
                        continue;
                    }

                    if (isOpen)
                    {
                        SubSceneUtil.OpenScene(ref state, subScene.Entity);
                    }
                    else
                    {
                        SubSceneUtil.CloseScene(ref state, subScene.Entity);
                    }
                }
            }

            this.values.Clear();
            this.subScenesBuffer.Clear();

            foreach (var (_, e) in SystemAPI.Query<RefRO<SceneReference>>().WithNone<RequiredSubScene>().WithEntityAccess())
            {
                state.EntityManager.GetName(e, out var name);
                if (name == default)
                {
                    name = e.ToFixedString();
                }

                this.subScenesBuffer.Add(new SubSceneToolbarViewModel.Data.SubSceneName { Entity = e, Name = name });
            }

            this.subScenesBuffer.Sort();

            for (var index = 0; index < this.subScenesBuffer.Length; index++)
            {
                var loaded = SubSceneUtil.IsLoadingOrLoaded(ref state, this.subScenesBuffer[index].Entity);
                if (loaded)
                {
                    this.values.Add(index);
                }
            }

            if (!this.values.AsArray().ArraysEqual(data.SubSceneValues.AsArray()))
            {
                data.SubSceneValues.Clear();
                data.SubSceneValues.AddRange(this.values.AsArray());
                data.NotifyExplicit($"{nameof(SubSceneToolbarViewModel.Data.SubSceneValues)}");
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
