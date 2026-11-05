namespace BovineLabs.Anchor
{
    using BovineLabs.Core.Utility;
    using Unity.Burst;
    using Unity.Scripting.LifecycleManagement;

    public unsafe partial class AnchorApp
    {
        [OnCodeInitializing]
        private static void InitializeTrampolines()
        {
            Burst.SetUIVisibleFunc.Data = new BurstTrampoline(&SetUIVisibleForwardingPacked);
            Burst.ToggleUIVisibleFunc.Data = new BurstTrampoline(&ToggleUIVisibleForwardingPacked);
        }

        private static void SetUIVisibleForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var visible = ref BurstTrampoline.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            if (Current != null)
            {
                Current.UIVisible = visible;
            }
        }

        private static void ToggleUIVisibleForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var visible = ref BurstTrampoline.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            if (Current == null)
            {
                visible = false;
                return;
            }

            Current.UIVisible = !Current.UIVisible;
            visible = Current.UIVisible;
        }

        /// <summary>
        /// Burst-compatible access to <see cref="UIVisible"/>, such as for a hide UI hotkey handled in a system.
        /// </summary>
        public static class Burst
        {
            // Trampolines are reassigned whenever code initializes.
            [NoAutoStaticsCleanup]
            internal static readonly SharedStatic<BurstTrampoline> SetUIVisibleFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorApp, SetUIVisibleType>();

            [NoAutoStaticsCleanup]
            internal static readonly SharedStatic<BurstTrampoline> ToggleUIVisibleFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorApp, ToggleUIVisibleType>();

            public static void SetUIVisible(bool visible)
            {
                if (SetUIVisibleFunc.Data.IsCreated)
                {
                    SetUIVisibleFunc.Data.Invoke(visible);
                }
            }

            /// <summary>
            /// Toggles <see cref="UIVisible"/>.
            /// </summary>
            /// <returns>The new visibility, or false when no app exists.</returns>
            public static bool ToggleUIVisible()
            {
                if (ToggleUIVisibleFunc.Data.IsCreated)
                {
                    ToggleUIVisibleFunc.Data.InvokeOut(out bool visible);
                    return visible;
                }

                return false;
            }

// @formatter:off
            public struct SetUIVisibleType { }
            public struct ToggleUIVisibleType { }
// @formatter:on
        }
    }
}
