﻿// <copyright file="ToolbarSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if (BL_DEBUG || UNITY_EDITOR) && UNITY_ENTITIES
namespace BovineLabs.Anchor.Toolbar
{
    using Unity.Entities;

    /// <summary> Group that all Toolbar linked systems should be placed. </summary>
#if BL_CORE && BL_CORE_EXTENSIONS
    [WorldSystemFilter(WorldSystemFilterFlags.Default | BovineLabs.Core.Worlds.Service)]
    [UpdateInGroup(typeof(BovineLabs.Core.DebugSystemGroup))]
#else
    using WorldFlag = Unity.Entities.WorldSystemFilterFlags;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(
        WorldFlag.LocalSimulation | WorldFlag.ClientSimulation | WorldFlag.ServerSimulation | WorldFlag.ThinClientSimulation,
        WorldFlag.LocalSimulation | WorldFlag.ClientSimulation | WorldFlag.ServerSimulation | WorldFlag.ThinClientSimulation)]
#endif
    public partial class ToolbarSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
            if (ToolbarView.Instance == null)
            {
                return;
            }

            base.OnUpdate();
        }
    }
}
#endif
