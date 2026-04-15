// <copyright file="AnchorNavHost.Burst.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using BovineLabs.Core.Utility;
    using Unity.Burst;
    using Unity.Collections;

    public unsafe partial class AnchorNavHost
    {
        static AnchorNavHost()
        {
            Burst.NavigateFunc.Data = new BurstTrampoline(&NavigateForwardingPacked);
            Burst.ToggleFunc.Data = new BurstTrampoline(&ToggleForwardingPacked);
            Burst.CurrentFunc.Data = new BurstTrampoline(&CurrentForwardingPacked);
            Burst.ClearBackStackFunc.Data = new BurstTrampoline(&ClearBackStackForwardingPacked);
            Burst.ClearNavigationFunc.Data = new BurstTrampoline(&ClearNavigationForwardingPacked);
            Burst.PopBackStackFunc.Data = new BurstTrampoline(&PopBackStackForwardingPacked);
            Burst.PopBackStackToPanelFunc.Data = new BurstTrampoline(&PopBackStackToPanelForwardingPacked);
            Burst.CloseAllPopupsFunc.Data = new BurstTrampoline(&CloseAllPopupsForwardingPacked);
            Burst.ClosePopupFunc.Data = new BurstTrampoline(&ClosePopupForwardingPacked);
            Burst.HasActivePopupsFunc.Data = new BurstTrampoline(&HasActivePopupsForwardingPacked);
            Burst.CanGoBackFunc.Data = new BurstTrampoline(&CanGoBackForwardingPacked);
            Burst.SaveStateFunc.Data = new BurstTrampoline(&SaveStateForwardingPacked);
            Burst.ReleaseStateFunc.Data = new BurstTrampoline(&ReleaseStateForwardingPacked);
        }

        private static void NavigateForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var screen = ref BurstTrampoline.ArgumentsFromPtr<FixedString32Bytes>(argumentsPtr, argumentsSize);
            AnchorApp.Current?.NavHost.Navigate(screen.ToString());
        }

        private static void ToggleForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var arguments = ref BurstTrampoline.ArgumentsFromPtr<BurstManagedPair<FixedString32Bytes, bool>>(argumentsPtr, argumentsSize);
            arguments.Second = AnchorApp.Current?.NavHost.Toggle(arguments.First.ToString()) ?? false;
        }

        private static void ClearBackStackForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            _ = argumentsPtr;
            _ = argumentsSize;
            AnchorApp.Current?.NavHost.ClearBackStack();
        }

        private static void ClearNavigationForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var exitAnimation = ref BurstTrampoline.ArgumentsFromPtr<int>(argumentsPtr, argumentsSize);
            AnchorApp.Current?.NavHost.ClearNavigation(exitAnimation);
        }

        private static void PopBackStackForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var popped = ref BurstTrampoline.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            popped = AnchorApp.Current?.NavHost.PopBackStack() ?? false;
        }

        private static void PopBackStackToPanelForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var popped = ref BurstTrampoline.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            popped = AnchorApp.Current?.NavHost.PopBackStackToPanel() ?? false;
        }

        private static void CloseAllPopupsForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var arguments = ref BurstTrampoline.ArgumentsFromPtr<BurstManagedPair<int, bool>>(argumentsPtr, argumentsSize);
            arguments.Second = AnchorApp.Current?.NavHost.CloseAllPopups(arguments.First) ?? false;
        }

        private static void ClosePopupForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var arguments = ref BurstTrampoline.ArgumentsFromPtr<BurstManagedTriple<FixedString32Bytes, int, bool>>(argumentsPtr, argumentsSize);
            arguments.Third = AnchorApp.Current?.NavHost.ClosePopup(arguments.First.ToString(), arguments.Second) ?? false;
        }

        private static void HasActivePopupsForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var hasActivePopups = ref BurstTrampoline.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            hasActivePopups = AnchorApp.Current?.NavHost.HasActivePopups ?? false;
        }

        private static void CanGoBackForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var canGoBack = ref BurstTrampoline.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            canGoBack = AnchorApp.Current?.NavHost.CanGoBack ?? false;
        }

        private static void CurrentForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var name = ref BurstTrampoline.ArgumentsFromPtr<FixedString32Bytes>(argumentsPtr, argumentsSize);
            name = AnchorApp.Current?.NavHost.CurrentDestination ?? default(FixedString32Bytes);
        }

        private static void SaveStateForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var handle = ref BurstTrampoline.ArgumentsFromPtr<int>(argumentsPtr, argumentsSize);
            handle = AnchorApp.Current?.NavHost?.SaveStateHandle() ?? 0;
        }

        private static void ReleaseStateForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var arguments = ref BurstTrampoline.ArgumentsFromPtr<BurstManagedPair<int, bool>>(argumentsPtr, argumentsSize);
            AnchorApp.Current?.NavHost?.ReleaseStateHandle(arguments.First, arguments.Second);
        }

        public static class Burst
        {
            internal static readonly SharedStatic<BurstTrampoline> NavigateFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, NavigateType>();

            internal static readonly SharedStatic<BurstTrampoline> ToggleFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, ToggleType>();

            internal static readonly SharedStatic<BurstTrampoline> CurrentFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, CurrentType>();

            internal static readonly SharedStatic<BurstTrampoline> ClearBackStackFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, ClearBackStackType>();

            internal static readonly SharedStatic<BurstTrampoline> ClearNavigationFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, ClearNavigationType>();

            internal static readonly SharedStatic<BurstTrampoline> PopBackStackFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, PopBackStackType>();

            internal static readonly SharedStatic<BurstTrampoline> PopBackStackToPanelFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, PopBackStackToPanelType>();

            internal static readonly SharedStatic<BurstTrampoline> CloseAllPopupsFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, CloseAllPopupsType>();

            internal static readonly SharedStatic<BurstTrampoline> ClosePopupFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, ClosePopupType>();

            internal static readonly SharedStatic<BurstTrampoline> HasActivePopupsFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, HasActivePopupsType>();

            internal static readonly SharedStatic<BurstTrampoline> CanGoBackFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, CanGoBackType>();

            internal static readonly SharedStatic<BurstTrampoline> SaveStateFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, SaveStateType>();

            internal static readonly SharedStatic<BurstTrampoline> ReleaseStateFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, ReleaseStateType>();

            /// <inheritdoc cref="AnchorNavHost.Navigate(string, AnchorNavArgument[])" />
            public static void Navigate(in FixedString32Bytes screen)
            {
                if (NavigateFunc.Data.IsCreated)
                {
                    NavigateFunc.Data.Invoke(screen);
                }
            }

            /// <inheritdoc cref="AnchorNavHost.Toggle(string, AnchorNavArgument[])" />
            public static bool Toggle(in FixedString32Bytes actionOrDestination)
            {
                if (ToggleFunc.Data.IsCreated)
                {
                    ToggleFunc.Data.InvokeOut(actionOrDestination, out bool toggled);
                    return toggled;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.CurrentDestination" />
            public static FixedString32Bytes CurrentDestination()
            {
                if (CurrentFunc.Data.IsCreated)
                {
                    CurrentFunc.Data.InvokeOut(out FixedString32Bytes name);
                    return name;
                }

                return default;
            }

            /// <inheritdoc cref="AnchorNavHost.ClearBackStack" />
            public static void ClearBackStack()
            {
                if (ClearBackStackFunc.Data.IsCreated)
                {
                    ClearBackStackFunc.Data.Invoke();
                }
            }

            /// <inheritdoc cref="AnchorNavHost.ClearNavigation" />
            public static void ClearNavigation(in int exitAnimation = 0)
            {
                if (ClearNavigationFunc.Data.IsCreated)
                {
                    ClearNavigationFunc.Data.Invoke(exitAnimation);
                }
            }

            /// <inheritdoc cref="AnchorNavHost.PopBackStack" />
            public static bool PopBackStack()
            {
                if (PopBackStackFunc.Data.IsCreated)
                {
                    PopBackStackFunc.Data.InvokeOut(out bool popped);
                    return popped;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.PopBackStackToPanel" />
            public static bool PopBackStackToPanel()
            {
                if (PopBackStackToPanelFunc.Data.IsCreated)
                {
                    PopBackStackToPanelFunc.Data.InvokeOut(out bool popped);
                    return popped;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.CloseAllPopups" />
            public static bool CloseAllPopups(int exitAnimation = 0)
            {
                if (CloseAllPopupsFunc.Data.IsCreated)
                {
                    CloseAllPopupsFunc.Data.InvokeOut(exitAnimation, out bool closed);
                    return closed;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.ClosePopup" />
            public static bool ClosePopup(in FixedString32Bytes destination, in int exitAnimation = 0)
            {
                if (ClosePopupFunc.Data.IsCreated)
                {
                    ClosePopupFunc.Data.InvokeOut(destination, exitAnimation, out bool closed);
                    return closed;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.HasActivePopups" />
            public static bool HasActivePopups()
            {
                if (HasActivePopupsFunc.Data.IsCreated)
                {
                    HasActivePopupsFunc.Data.InvokeOut(out bool hasActivePopups);
                    return hasActivePopups;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.CanGoBack" />
            public static bool CanGoBack()
            {
                if (CanGoBackFunc.Data.IsCreated)
                {
                    CanGoBackFunc.Data.InvokeOut(out bool canGoBack);
                    return canGoBack;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.SaveStateHandle" />
            public static int SaveStateHandle()
            {
                if (SaveStateFunc.Data.IsCreated)
                {
                    SaveStateFunc.Data.InvokeOut(out int handle);
                    return handle;
                }

                return 0;
            }

            /// <inheritdoc cref="AnchorNavHost.ReleaseStateHandle" />
            public static void ReleaseStateHandle(int handle, bool restore = true)
            {
                if (ReleaseStateFunc.Data.IsCreated)
                {
                    ReleaseStateFunc.Data.Invoke(handle, restore);
                }
            }

#pragma warning disable SA1502
#pragma warning disable SA1516
#pragma warning disable SA1515
// @formatter:off
            public struct NavigateType { }
            public struct ToggleType { }
            public struct CurrentType { }
            public struct ClearBackStackType { }
            public struct ClearNavigationType { }
            public struct PopBackStackType { }
            public struct PopBackStackToPanelType { }
            public struct CloseAllPopupsType { }
            public struct ClosePopupType { }
            public struct HasActivePopupsType { }
            public struct CanGoBackType { }
            public struct SaveStateType { }
            public struct ReleaseStateType { }
// @formatter:on
#pragma warning restore SA1515
#pragma warning restore SA1516
#pragma warning restore SA1502
        }
    }
}

