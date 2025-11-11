// <copyright file="ToolbarSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
#if BL_CORE_EXTENSIONS
    using BovineLabs.Core;
#else
    using WorldFlag = Unity.Entities.WorldSystemFilterFlags;
#endif
    using Unity.Entities;

    /// <summary>Group that all Toolbar linked systems should be placed into.</summary>
#if BL_CORE_EXTENSIONS
    [WorldSystemFilter(WorldSystemFilterFlags.Default | Worlds.Service)]
    [UpdateInGroup(typeof(DebugSystemGroup))]
#else
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
