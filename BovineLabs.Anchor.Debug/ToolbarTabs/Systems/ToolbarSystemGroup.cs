// <copyright file="ToolbarSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ToolbarTabs
{
    using BovineLabs.Core;
    using Unity.Entities;

    [WorldSystemFilter(WorldSystemFilterFlags.Default | Worlds.Service)]
    [UpdateInGroup(typeof(DebugSystemGroup))]
    public partial class ToolbarSystemGroup : ComponentSystemGroup
    {
    }
}
