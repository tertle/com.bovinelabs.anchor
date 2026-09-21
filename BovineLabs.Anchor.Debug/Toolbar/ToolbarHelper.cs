namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;
    using BovineLabs.Anchor.Binding;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;

    public unsafe struct ToolbarHelper<TM, TD>
        where TM : class, IToolbarElement, IBindingObjectNotify<TD>, new()
        where TD : unmanaged
    {
        private readonly FixedString32Bytes _tabName;
        private readonly FixedString32Bytes _groupName;

        private ToolbarRegistrationHandle _handle;

        private TD* _data;

        public ToolbarHelper(FixedString32Bytes tabName, FixedString32Bytes groupName)
        {
            _tabName = tabName;
            _groupName = groupName;
            _data = null;
            _handle = default;
        }

        public ToolbarHelper(ref SystemState state, FixedString32Bytes groupName)
            : this(FormatWorld(state.World), groupName)
        {
        }

        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(_data);

        public void Load()
        {
            _handle = Toolbar.GetRequired().Register<TM, TD>(
                _tabName.ToString(),
                _groupName.ToString(),
                out _data);
        }

        public void Unload()
        {
            try
            {
                Toolbar.Current?.Remove(_handle);
            }
            finally
            {
                _handle = default;
                _data = null;
            }
        }

        public bool IsVisible()
        {
            return ToolbarViewData.ActiveTab.Data == _tabName;
        }

        private static string FormatWorld(World world)
        {
            var name = world.Name;
            return name.EndsWith("World") ? name[..name.LastIndexOf("World", StringComparison.Ordinal)] : name;
        }
    }
}
