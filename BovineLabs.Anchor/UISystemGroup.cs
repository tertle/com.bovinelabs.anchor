// <copyright file="UISystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.Entities;

    /// <summary>
    /// Presentation system group used to run UI-related systems after the main simulation.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation, WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UISystemGroup : ComponentSystemGroup
    {
    }
}
