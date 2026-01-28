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

        public int SaveStateHandle()
        {
            var state = this.SaveState();
            var handle = this.nextStateHandle++;
            this.savedStates.Add(handle, state);
            return handle;
        }

        public void RestoreStateHandle(int handle)
        {
            if (!this.savedStates.Remove(handle, out var state))
            {
                return;
            }

            this.RestoreState(state);
        }
    }
}
