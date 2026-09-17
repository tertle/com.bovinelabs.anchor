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
        private readonly FixedString32Bytes tabName;
        private readonly FixedString32Bytes groupName;

        private ToolbarRegistrationHandle handle;

        private TD* data;

        public ToolbarHelper(FixedString32Bytes tabName, FixedString32Bytes groupName)
        {
            this.tabName = tabName;
            this.groupName = groupName;
            this.data = null;
            this.handle = default;
        }

        public ToolbarHelper(ref SystemState state, FixedString32Bytes groupName)
            : this(FormatWorld(state.World), groupName)
        {
        }

        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(this.data);

        public void Load()
        {
            this.handle = Toolbar.GetRequired().Register<TM, TD>(
                this.tabName.ToString(),
                this.groupName.ToString(),
                out this.data);
        }

        public void Unload()
        {
            try
            {
                Toolbar.Current?.Remove(this.handle);
            }
            finally
            {
                this.handle = default;
                this.data = null;
            }
        }

        public bool IsVisible()
        {
            return ToolbarViewData.ActiveTab.Data == this.tabName;
        }

        private static string FormatWorld(World world)
        {
            var name = world.Name;
            return name.EndsWith("World") ? name[..name.LastIndexOf("World", StringComparison.Ordinal)] : name;
        }
    }
}
