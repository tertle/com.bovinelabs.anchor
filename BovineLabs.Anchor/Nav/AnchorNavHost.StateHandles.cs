// <copyright file="AnchorNavHost.StateHandles.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System.Collections.Generic;

    public partial class AnchorNavHost
    {
        private readonly Dictionary<int, AnchorNavHostSaveState> savedStates = new();
        private int nextStateHandle = 1;

        /// <summary> Captures a navigation state snapshot and returns a handle to restore it later. </summary>
        /// <returns> The handle. </returns>
        public int SaveStateHandle()
        {
            var state = this.SaveState();
            var handle = this.nextStateHandle++;
            this.savedStates.Add(handle, state);
            return handle;
        }

        /// <summary> Releases a navigation state snapshot, optionally restoring it first. </summary>
        /// <param name="handle"> The handle to release. </param>
        /// <param name="restore"> Should the UI state also be restored to the saved snapshot. </param>
        /// <returns> True if the handle existed. </returns>
        public bool ReleaseStateHandle(int handle, bool restore = true)
        {
            if (!this.savedStates.Remove(handle, out var state))
            {
                return false;
            }

            if (restore)
            {
                this.RestoreState(state);
            }

            return true;
        }
    }
}
