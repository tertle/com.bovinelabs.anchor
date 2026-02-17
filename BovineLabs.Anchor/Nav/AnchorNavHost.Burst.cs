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
            Burst.NavigateFunc.Data = new BurstManagedCallWrapper(&NavigateForwardingPacked);
            Burst.CurrentFunc.Data = new BurstManagedCallWrapper(&CurrentForwardingPacked);
            Burst.ClearBackStackFunc.Data = new BurstManagedCallWrapper(&ClearBackStackForwardingPacked);
            Burst.ClearNavigationFunc.Data = new BurstManagedCallWrapper(&ClearNavigationForwardingPacked);
            Burst.PopBackStackFunc.Data = new BurstManagedCallWrapper(&PopBackStackForwardingPacked);
            Burst.PopBackStackToPanelFunc.Data = new BurstManagedCallWrapper(&PopBackStackToPanelForwardingPacked);
            Burst.CloseAllPopupsFunc.Data = new BurstManagedCallWrapper(&CloseAllPopupsForwardingPacked);
            Burst.ClosePopupFunc.Data = new BurstManagedCallWrapper(&ClosePopupForwardingPacked);
            Burst.HasActivePopupsFunc.Data = new BurstManagedCallWrapper(&HasActivePopupsForwardingPacked);
            Burst.CanGoBackFunc.Data = new BurstManagedCallWrapper(&CanGoBackForwardingPacked);
            Burst.SaveStateFunc.Data = new BurstManagedCallWrapper(&SaveStateForwardingPacked);
            Burst.ReleaseStateFunc.Data = new BurstManagedCallWrapper(&ReleaseStateForwardingPacked);
        }

        private static void NavigateForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var screen = ref BurstManagedCallWrapper.ArgumentsFromPtr<FixedString32Bytes>(argumentsPtr, argumentsSize);
            AnchorApp.current?.NavHost.Navigate(screen.ToString());
        }

        private static void ClearBackStackForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            _ = argumentsPtr;
            _ = argumentsSize;
            AnchorApp.current?.NavHost.ClearBackStack();
        }

        private static void ClearNavigationForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var exitAnimation = ref BurstManagedCallWrapper.ArgumentsFromPtr<int>(argumentsPtr, argumentsSize);
            AnchorApp.current?.NavHost.ClearNavigation(exitAnimation);
        }

        private static void PopBackStackForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var popped = ref BurstManagedCallWrapper.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            popped = AnchorApp.current?.NavHost.PopBackStack() ?? false;
        }

        private static void PopBackStackToPanelForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var popped = ref BurstManagedCallWrapper.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            popped = AnchorApp.current?.NavHost.PopBackStackToPanel() ?? false;
        }

        private static void CloseAllPopupsForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var arguments = ref BurstManagedCallWrapper.ArgumentsFromPtr<BurstManagedPair<int, bool>>(argumentsPtr, argumentsSize);
            arguments.Second = AnchorApp.current?.NavHost.CloseAllPopups(arguments.First) ?? false;
        }

        private static void ClosePopupForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var arguments = ref BurstManagedCallWrapper.ArgumentsFromPtr<BurstManagedTriple<FixedString32Bytes, int, bool>>(argumentsPtr, argumentsSize);
            arguments.Third = AnchorApp.current?.NavHost.ClosePopup(arguments.First.ToString(), arguments.Second) ?? false;
        }

        private static void HasActivePopupsForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var hasActivePopups = ref BurstManagedCallWrapper.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            hasActivePopups = AnchorApp.current?.NavHost.HasActivePopups ?? false;
        }

        private static void CanGoBackForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var canGoBack = ref BurstManagedCallWrapper.ArgumentsFromPtr<bool>(argumentsPtr, argumentsSize);
            canGoBack = AnchorApp.current?.NavHost.CanGoBack ?? false;
        }

        private static void CurrentForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var name = ref BurstManagedCallWrapper.ArgumentsFromPtr<FixedString32Bytes>(argumentsPtr, argumentsSize);
            name = AnchorApp.current?.NavHost.CurrentDestination ?? default(FixedString32Bytes);
        }

        private static void SaveStateForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var handle = ref BurstManagedCallWrapper.ArgumentsFromPtr<int>(argumentsPtr, argumentsSize);
            handle = AnchorApp.current?.NavHost?.SaveStateHandle() ?? 0;
        }

        private static void ReleaseStateForwardingPacked(void* argumentsPtr, int argumentsSize)
        {
            ref var arguments = ref BurstManagedCallWrapper.ArgumentsFromPtr<BurstManagedPair<int, bool>>(argumentsPtr, argumentsSize);
            AnchorApp.current?.NavHost?.ReleaseStateHandle(arguments.First, arguments.Second);
        }

        public static class Burst
        {
            internal static readonly SharedStatic<BurstManagedCallWrapper> NavigateFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, NavigateType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> CurrentFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, CurrentType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> ClearBackStackFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, ClearBackStackType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> ClearNavigationFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, ClearNavigationType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> PopBackStackFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, PopBackStackType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> PopBackStackToPanelFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, PopBackStackToPanelType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> CloseAllPopupsFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, CloseAllPopupsType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> ClosePopupFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, ClosePopupType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> HasActivePopupsFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, HasActivePopupsType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> CanGoBackFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, CanGoBackType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> SaveStateFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, SaveStateType>();

            internal static readonly SharedStatic<BurstManagedCallWrapper> ReleaseStateFunc =
                SharedStatic<BurstManagedCallWrapper>.GetOrCreate<AnchorNavHost, ReleaseStateType>();

            /// <inheritdoc cref="AnchorNavHost.Navigate(string, Unity.AppUI.Navigation.Argument[])" />
            public static void Navigate(in FixedString32Bytes screen)
            {
                if (NavigateFunc.Data.IsCreated)
                {
                    NavigateFunc.Data.Invoke(screen);
                }
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
