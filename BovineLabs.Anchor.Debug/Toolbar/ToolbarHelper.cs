// <copyright file="ToolbarHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;
    using BovineLabs.Anchor.Binding;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;

    /// <summary>
    /// Utility that manages the lifecycle of a toolbar tab bound to a burst-compatible view model.
    /// </summary>
    /// <typeparam name="TM">Managed view-model type that exposes binding data.</typeparam>
    /// <typeparam name="TD">Unmanaged data struct pinned for burst access.</typeparam>
    public unsafe struct ToolbarHelper<TM, TD>
        where TM : class, IToolbarElement, IBindingObjectNotify<TD>, new()
        where TD : unmanaged
    {
        private readonly FixedString32Bytes tabName;
        private readonly FixedString32Bytes groupName;

        private ToolbarRegistrationHandle handle;

        private TD* data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarHelper{TM, TD}"/> struct.
        /// </summary>
        /// <param name="tabName">Name of the toolbar tab.</param>
        /// <param name="groupName">Name of the group inside the tab.</param>
        public ToolbarHelper(FixedString32Bytes tabName, FixedString32Bytes groupName)
        {
            this.tabName = tabName;
            this.groupName = groupName;
            this.data = null;
            this.handle = default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarHelper{TM, TD}"/> struct.
        /// </summary>
        /// <param name="state">System state whose world name will become the tab name.</param>
        /// <param name="groupName">Name of the group inside the tab.</param>
        public ToolbarHelper(ref SystemState state, FixedString32Bytes groupName)
            : this(FormatWorld(state.World), groupName)
        {
        }

        /// <summary>Gets access to the unmanaged binding data pinned for burst.</summary>
        public ref TD Binding => ref UnsafeUtility.AsRef<TD>(this.data);

        /// <summary>
        /// Registers the toolbar model and obtains its pinned binding data. Usually called from OnStartRunning.
        /// </summary>
        public void Load()
        {
            this.handle = Toolbar.GetRequired().Register<TM, TD>(
                this.tabName.ToString(),
                this.groupName.ToString(),
                out this.data);
        }

        /// <summary>
        /// Removes the toolbar registration and releases its managed and pinned state.
        /// </summary>
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

        /// <summary>Determines whether the helper’s tab is currently selected.</summary>
        /// <returns><c>true</c> if the helper’s tab is the active tab.</returns>
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
