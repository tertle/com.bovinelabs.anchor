// <copyright file="ToolbarSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Toolbar
{
    using Unity.Entities;

#if BL_CORE
    [WorldSystemFilter(WorldSystemFilterFlags.Default | BovineLabs.Core.Worlds.Service)]
    [UpdateInGroup(typeof(BovineLabs.Core.DebugSystemGroup))]
#else
    using WorldFlag = Unity.Entities.WorldSystemFilterFlags;

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [WorldSystemFilter(
        WorldFlag.LocalSimulation | WorldFlag.ClientSimulation | WorldFlag.ServerSimulation | WorldFlag.ThinClientSimulation | WorldFlag.Editor,
        WorldFlag.LocalSimulation | WorldFlag.ClientSimulation | WorldFlag.ServerSimulation | WorldFlag.ThinClientSimulation)]
#endif
    public partial class ToolbarSystemGroup : ComponentSystemGroup
    {
    }
}
#endif
