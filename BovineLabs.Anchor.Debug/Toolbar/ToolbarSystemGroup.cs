// <copyright file="ToolbarSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using BovineLabs.Core;
    using Unity.Entities;
#if BL_CORE_EXTENSIONS
#else
    using WorldFlag = Unity.Entities.WorldSystemFilterFlags;
#endif

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
        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            if (ToolbarView.Instance == null || AnchorApp.Current == null)
            {
                return;
            }

            base.OnUpdate();
        }
    }
}
