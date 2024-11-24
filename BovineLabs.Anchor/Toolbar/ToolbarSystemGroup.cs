// <copyright file="ToolbarSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if (BL_DEBUG || UNITY_EDITOR) && UNITY_ENTITIES
namespace BovineLabs.Anchor.Toolbar
{
#if BL_CORE && BL_CORE_EXTENSIONS
    using BovineLabs.Core;
#endif
    using Unity.Entities;

    /// <summary> Group that all Toolbar linked systems should be placed. </summary>
#if BL_CORE && BL_CORE_EXTENSIONS
    [WorldSystemFilter(WorldSystemFilterFlags.Default | Worlds.Service)]
    [UpdateInGroup(typeof(DebugSystemGroup))]
#else
    using WorldFlag = Unity.Entities.WorldSystemFilterFlags;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(
        WorldFlag.LocalSimulation | WorldFlag.ClientSimulation | WorldFlag.ServerSimulation | WorldFlag.ThinClientSimulation,
        WorldFlag.LocalSimulation | WorldFlag.ClientSimulation | WorldFlag.ServerSimulation)]
#endif
    public partial class ToolbarSystemGroup : ComponentSystemGroup
    {
        protected override void OnUpdate()
        {
            if (ToolbarView.Instance == null || AnchorApp.current == null)
            {
                return;
            }

            base.OnUpdate();
        }
    }
}
#endif
