// <copyright file="ToolbarSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using BovineLabs.Core;
    using Unity.Entities;

    /// <summary>Group that all Toolbar linked systems should be placed into.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default | Worlds.Service)]
    [UpdateInGroup(typeof(DebugSystemGroup))]
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
