// <copyright file="ToolbarViewData.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
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
#endif
