// <copyright file="ToolbarHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if (BL_DEBUG || UNITY_EDITOR) && UNITY_ENTITIES
namespace BovineLabs.Anchor.Toolbar
{
    using System;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using UnityEngine.UIElements;

    public unsafe struct ToolbarHelper<TV, TM, TD>
        where TV : VisualElement, IView<TM>
        where TM : class, IBindingObject<TD>
        where TD : unmanaged
    {
        private readonly FixedString32Bytes tabName;
        private readonly FixedString32Bytes groupName;

        private int key;

        private GCHandle handle;
        private TD* data;

        public ToolbarHelper(FixedString32Bytes tabName, FixedString32Bytes groupName)
        {
            this.tabName = tabName;
            this.groupName = groupName;
            this.handle = default;
            this.data = null;
            this.key = 0;
        }

        public ToolbarHelper(ref SystemState state, FixedString32Bytes groupName)
            : this(FormatWorld(state.World), groupName)
        {
        }

        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(this.data);

        // Load the tab onto the group. Usually called from OnStartRunning.
        public void Load()
        {
            ToolbarView.Instance.AddTab<TV>(this.tabName.ToString(), this.groupName.ToString(), out this.key, out var view);

            view.ViewModel.Load();
            this.handle = GCHandle.Alloc(view.ViewModel.Value, GCHandleType.Pinned);
            this.data = (TD*)UnsafeUtility.AddressOf(ref view.ViewModel.Value);
        }

        public void Unload()
        {
            var view = ToolbarView.Instance.RemoveTab<TV>(this.key);

            view.ViewModel.Unload();
            this.handle.Free();
            this.handle = default;
            this.data = null;
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
#endif
