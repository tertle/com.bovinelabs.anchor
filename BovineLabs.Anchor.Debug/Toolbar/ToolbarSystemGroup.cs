// <copyright file="ToolbarSystemGroup.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System.Collections.Generic;
    using BovineLabs.Core;
    using Unity.Entities;

    /// <summary>Group that all Toolbar linked systems should be placed into.</summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default | Worlds.Service)]
    [UpdateInGroup(typeof(DebugSystemGroup))]
    public partial class ToolbarSystemGroup : ComponentSystemGroup
    {
        private readonly HashSet<SystemHandle> suspendedSystems = new();
        private Toolbar currentToolbar;

        /// <inheritdoc/>
        protected override void OnUpdate()
        {
            this.SortSystems();
            using var systems = this.GetAllSystems();
            var toolbar = Toolbar.Current;
            var available = toolbar != null && AnchorApp.Current != null;

            if (available && !ReferenceEquals(this.currentToolbar, toolbar))
            {
                this.SuspendSystems(systems);
                base.OnUpdate();

                this.ResumeSystems(systems);
                this.currentToolbar = toolbar;
                base.OnUpdate();
                return;
            }

            if (available)
            {
                this.ResumeSystems(systems);
                this.currentToolbar = toolbar;
            }
            else
            {
                this.SuspendSystems(systems);
                this.currentToolbar = null;
            }

            base.OnUpdate();
        }

        private void SuspendSystems(Unity.Collections.NativeList<SystemHandle> systems)
        {
            for (var i = 0; i < systems.Length; i++)
            {
                var handle = systems[i];
                ref var state = ref this.World.Unmanaged.ResolveSystemStateRef(handle);

                if (state.Enabled)
                {
                    state.Enabled = false;
                    this.suspendedSystems.Add(handle);
                }
            }
        }

        private void ResumeSystems(Unity.Collections.NativeList<SystemHandle> systems)
        {
            for (var i = 0; i < systems.Length; i++)
            {
                var handle = systems[i];
                if (this.suspendedSystems.Remove(handle))
                {
                    ref var state = ref this.World.Unmanaged.ResolveSystemStateRef(handle);
                    state.Enabled = true;
                }
            }

            this.suspendedSystems.Clear();
        }
    }
}
