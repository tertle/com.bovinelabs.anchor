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
    using Unity.Scenes;

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
                    var isCurrentlyOpen = SubSceneUtil.IsSectionLoadingOrLoaded(ref state, subScene.Entity);

                    if (isOpen == isCurrentlyOpen)
                    {
                        continue;
                    }

                    if (isOpen)
                    {
                        SubSceneUtil.OpenSection(ref state, subScene.Entity);
                    }
                    else
                    {
                        SubSceneUtil.CloseSection(ref state, subScene.Entity);
                    }
                }
            }

            this.values.Clear();
            this.subScenesBuffer.Clear();

            foreach (var (sections, e) in SystemAPI.Query<DynamicBuffer<ResolvedSectionEntity>>()
                         .WithAll<SceneReference>().WithNone<RequiredSubScene, PrefabRoot>().WithEntityAccess())
            {
                if (sections.Length == 1)
                {
                    state.EntityManager.GetName(e, out var sectionName);

                    if (sectionName == default)
                    {
                        sectionName = e.ToFixedString();
                    }

                    this.subScenesBuffer.Add(new SubSceneToolbarViewModel.Data.SubSceneName { Entity = sections[0].SectionEntity, Name = sectionName });
                }
                else
                {
                    foreach (var section in sections)
                    {
                        state.EntityManager.GetName(section.SectionEntity, out var sectionName);

                        if (sectionName == default)
                        {
                            sectionName = e.ToFixedString();
                        }

                        this.subScenesBuffer.Add(new SubSceneToolbarViewModel.Data.SubSceneName { Entity = section.SectionEntity, Name = sectionName });
                    }
                }
            }

            this.subScenesBuffer.Sort();

            for (var index = 0; index < this.subScenesBuffer.Length; index++)
            {
                var loaded = SubSceneUtil.IsSectionLoadingOrLoaded(ref state, this.subScenesBuffer[index].Entity);
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
