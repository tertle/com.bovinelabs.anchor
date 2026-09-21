namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System.Collections.Generic;
    using BovineLabs.Core;
    using Unity.Entities;

    /// <summary>
    /// Runs debug toolbar systems only while the toolbar is available and restarts them when it is replaced.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default | Worlds.Service)]
    [UpdateInGroup(typeof(DebugSystemGroup))]
    public partial class ToolbarSystemGroup : ComponentSystemGroup
    {
        private readonly HashSet<SystemHandle> _suspendedSystems = new();
        private Toolbar _currentToolbar;

        protected override void OnUpdate()
        {
            SortSystems();
            using var systems = GetAllSystems();
            var toolbar = Toolbar.Current;
            var available = toolbar != null && AnchorApp.Current != null;

            if (available && !ReferenceEquals(_currentToolbar, toolbar))
            {
                SuspendSystems(systems);
                base.OnUpdate();

                ResumeSystems(systems);
                _currentToolbar = toolbar;
                base.OnUpdate();
                return;
            }

            if (available)
            {
                ResumeSystems(systems);
                _currentToolbar = toolbar;
            }
            else
            {
                SuspendSystems(systems);
                _currentToolbar = null;
            }

            base.OnUpdate();
        }

        private void SuspendSystems(Unity.Collections.NativeList<SystemHandle> systems)
        {
            for (var i = 0; i < systems.Length; i++)
            {
                var handle = systems[i];
                ref var state = ref World.Unmanaged.ResolveSystemStateRef(handle);

                if (state.Enabled)
                {
                    state.Enabled = false;
                    _suspendedSystems.Add(handle);
                }
            }
        }

        private void ResumeSystems(Unity.Collections.NativeList<SystemHandle> systems)
        {
            for (var i = 0; i < systems.Length; i++)
            {
                var handle = systems[i];
                if (_suspendedSystems.Remove(handle))
                {
                    ref var state = ref World.Unmanaged.ResolveSystemStateRef(handle);
                    state.Enabled = true;
                }
            }

            _suspendedSystems.Clear();
        }
    }
}
