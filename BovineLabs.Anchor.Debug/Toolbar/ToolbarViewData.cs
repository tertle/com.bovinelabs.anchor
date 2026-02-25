// <copyright file="ToolbarViewData.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using Unity.Burst;
    using Unity.Collections;

    /// <summary> Burst data separate to avoid compiling issues with static variables. </summary>
    public static class ToolbarViewData
    {
        public static readonly SharedStatic<FixedString32Bytes> ActiveTab = SharedStatic<FixedString32Bytes>.GetOrCreate<ActiveTabVar>();

        private struct ActiveTabVar
        {
        }
    }
}
