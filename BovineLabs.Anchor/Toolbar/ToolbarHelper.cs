// <copyright file="ToolbarHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using System;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.UIElements;

    public unsafe struct ToolbarHelper<TV, TM, TD>
        where TV : VisualElement, IView
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
            this.data = default;
            this.key = 0;
        }

#if UNITY_ENTITIES
        public ToolbarHelper(ref Unity.Entities.SystemState state, FixedString32Bytes groupName)
            : this(FormatWorld(state.World), groupName)
        {
        }
#endif

        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(this.data);

        // Load the tab onto the group. Usually called from OnStartRunning.
        public void Load()
        {
            ToolbarView.Instance.AddTab<TV>(this.tabName.ToString(), this.groupName.ToString(), out this.key, out var view);

            var binding = (TM)view.ViewModel;
            this.handle = GCHandle.Alloc(binding, GCHandleType.Pinned);
            this.data = (TD*)UnsafeUtility.AddressOf(ref binding.Value);
        }

        public void Unload()
        {
            ToolbarView.Instance.RemoveTab(this.key);
            this.handle.Free();
            this.handle = default;
            this.data = default;
        }

        public bool IsVisible()
        {
            return ToolbarViewrData.ActiveTab.Data == this.tabName;
        }

        /// <remarks> Not burst compatible. </remarks>
        public TV GetView()
        {
            return (TV)ToolbarView.Instance.GetPanel(this.key);
        }

#if UNITY_ENTITIES
        private static string FormatWorld(Unity.Entities.World world)
        {
            var name = world.Name;
            return name.EndsWith("World") ? name[..name.LastIndexOf("World", StringComparison.Ordinal)] : name;
        }
#endif
    }
}
#endif
