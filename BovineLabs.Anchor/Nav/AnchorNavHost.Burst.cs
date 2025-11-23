// <copyright file="AnchorNavHost.Burst.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using AOT;
    using BovineLabs.Core.Utility;
    using Unity.AppUI.Navigation;
    using Unity.Burst;
    using Unity.Collections;

    public partial class AnchorNavHost
    {
        static AnchorNavHost()
        {
            Burst.NavigateFunc.Data = new BurstDelegate<FixedString32Bytes>(NavigateForwarding);
            Burst.CurrentFunc.Data = new BurstOutDelegate<FixedString32Bytes>(CurrentForwarding);
            Burst.ClearBackStackFunc.Data = new BurstDelegate(ClearBackStackForwarding);
            Burst.PopBackStackFunc.Data = new BurstOutDelegate<bool>(PopBackStackForwarding);
            Burst.PopBackStackToPanelFunc.Data = new BurstOutDelegate<bool>(PopBackStackToPanelForwarding);
            Burst.CloseAllPopupsFunc.Data = new BurstOutDelegate<NavigationAnimation, bool>(CloseAllPopupsForwarding);
            Burst.ClosePopupFunc.Data = new BurstOutDelegate<FixedString32Bytes, NavigationAnimation, bool>(ClosePopupForwarding);
            Burst.HasActivePopupsFunc.Data = new BurstOutDelegate<bool>(HasActivePopupsForwarding);
            Burst.CanGoBackFunc.Data = new BurstOutDelegate<bool>(CanGoBackForwarding);
        }

        [MonoPInvokeCallback(typeof(BurstDelegate<FixedString32Bytes>.ChangedDelegate))]
        private static void NavigateForwarding(in FixedString32Bytes screen)
        {
            AnchorApp.current.NavHost.Navigate(screen.ToString());
        }

        [MonoPInvokeCallback(typeof(BurstDelegate.ChangedDelegate))]
        private static void ClearBackStackForwarding()
        {
            AnchorApp.current.NavHost.ClearBackStack();
        }

        [MonoPInvokeCallback(typeof(BurstOutDelegate<bool>.ChangedDelegate))]
        private static void PopBackStackForwarding(out bool popped)
        {
            popped = AnchorApp.current.NavHost.PopBackStack();
        }

        [MonoPInvokeCallback(typeof(BurstOutDelegate<bool>.ChangedDelegate))]
        private static void PopBackStackToPanelForwarding(out bool popped)
        {
            popped = AnchorApp.current.NavHost.PopBackStackToPanel();
        }

        [MonoPInvokeCallback(typeof(BurstOutDelegate<NavigationAnimation, bool>.ChangedDelegate))]
        private static void CloseAllPopupsForwarding(in NavigationAnimation exitAnimation, out bool closed)
        {
            closed = AnchorApp.current.NavHost.CloseAllPopups(exitAnimation);
        }

        [MonoPInvokeCallback(typeof(BurstOutDelegate<FixedString32Bytes, NavigationAnimation, bool>.ChangedDelegate))]
        private static void ClosePopupForwarding(in FixedString32Bytes destination, in NavigationAnimation exitAnimation, out bool closed)
        {
            closed = AnchorApp.current.NavHost.ClosePopup(destination.ToString(), exitAnimation);
        }

        [MonoPInvokeCallback(typeof(BurstOutDelegate<bool>.ChangedDelegate))]
        private static void HasActivePopupsForwarding(out bool hasActivePopups)
        {
            hasActivePopups = AnchorApp.current.NavHost.HasActivePopups;
        }

        [MonoPInvokeCallback(typeof(BurstOutDelegate<bool>.ChangedDelegate))]
        private static void CanGoBackForwarding(out bool canGoBack)
        {
            canGoBack = AnchorApp.current.NavHost.CanGoBack;
        }

        [MonoPInvokeCallback(typeof(BurstOutDelegate<FixedString32Bytes>.ChangedDelegate))]
        private static void CurrentForwarding(out FixedString32Bytes name)
        {
            name = AnchorApp.current.NavHost.CurrentDestination ?? default(FixedString32Bytes);
        }

        public static class Burst
        {
            internal static readonly SharedStatic<BurstDelegate<FixedString32Bytes>> NavigateFunc =
                SharedStatic<BurstDelegate<FixedString32Bytes>>.GetOrCreate<AnchorNavHost, NavigateType>();

            internal static readonly SharedStatic<BurstOutDelegate<FixedString32Bytes>> CurrentFunc =
                SharedStatic<BurstOutDelegate<FixedString32Bytes>>.GetOrCreate<AnchorNavHost, CurrentType>();

            internal static readonly SharedStatic<BurstDelegate> ClearBackStackFunc =
                SharedStatic<BurstDelegate>.GetOrCreate<AnchorNavHost, ClearBackStackType>();

            internal static readonly SharedStatic<BurstOutDelegate<bool>> PopBackStackFunc =
                SharedStatic<BurstOutDelegate<bool>>.GetOrCreate<AnchorNavHost, PopBackStackType>();

            internal static readonly SharedStatic<BurstOutDelegate<bool>> PopBackStackToPanelFunc =
                SharedStatic<BurstOutDelegate<bool>>.GetOrCreate<AnchorNavHost, PopBackStackToPanelType>();

            internal static readonly SharedStatic<BurstOutDelegate<NavigationAnimation, bool>> CloseAllPopupsFunc =
                SharedStatic<BurstOutDelegate<NavigationAnimation, bool>>.GetOrCreate<AnchorNavHost, CloseAllPopupsType>();

            internal static readonly SharedStatic<BurstOutDelegate<FixedString32Bytes, NavigationAnimation, bool>> ClosePopupFunc =
                SharedStatic<BurstOutDelegate<FixedString32Bytes, NavigationAnimation, bool>>.GetOrCreate<AnchorNavHost, ClosePopupType>();

            internal static readonly SharedStatic<BurstOutDelegate<bool>> HasActivePopupsFunc =
                SharedStatic<BurstOutDelegate<bool>>.GetOrCreate<AnchorNavHost, HasActivePopupsType>();

            internal static readonly SharedStatic<BurstOutDelegate<bool>> CanGoBackFunc =
                SharedStatic<BurstOutDelegate<bool>>.GetOrCreate<AnchorNavHost, CanGoBackType>();

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
                    CurrentFunc.Data.Invoke(out var name);
                    return name;
                }

                return default;
            }

            public static void ClearBackStack()
            {
                if (ClearBackStackFunc.Data.IsCreated)
                {
                    ClearBackStackFunc.Data.Invoke();
                }
            }

            public static bool PopBackStack()
            {
                if (PopBackStackFunc.Data.IsCreated)
                {
                    PopBackStackFunc.Data.Invoke(out var popped);
                    return popped;
                }

                return false;
            }

            public static bool PopBackStackToPanel()
            {
                if (PopBackStackToPanelFunc.Data.IsCreated)
                {
                    PopBackStackToPanelFunc.Data.Invoke(out var popped);
                    return popped;
                }

                return false;
            }

            public static bool CloseAllPopups(NavigationAnimation exitAnimation = NavigationAnimation.None)
            {
                if (CloseAllPopupsFunc.Data.IsCreated)
                {
                    CloseAllPopupsFunc.Data.Invoke(exitAnimation, out var closed);
                    return closed;
                }

                return false;
            }

            public static bool ClosePopup(in FixedString32Bytes destination, NavigationAnimation exitAnimation = NavigationAnimation.None)
            {
                if (ClosePopupFunc.Data.IsCreated)
                {
                    ClosePopupFunc.Data.Invoke(destination, exitAnimation, out var closed);
                    return closed;
                }

                return false;
            }

            public static bool HasActivePopups()
            {
                if (HasActivePopupsFunc.Data.IsCreated)
                {
                    HasActivePopupsFunc.Data.Invoke(out var hasActivePopups);
                    return hasActivePopups;
                }

                return false;
            }

            public static bool CanGoBack()
            {
                if (CanGoBackFunc.Data.IsCreated)
                {
                    CanGoBackFunc.Data.Invoke(out var canGoBack);
                    return canGoBack;
                }

                return false;
            }

#pragma warning disable SA1502
#pragma warning disable SA1516
#pragma warning disable SA1515
// @formatter:off
            public struct NavigateType { }
            public struct CurrentType { }
            public struct ClearBackStackType { }
            public struct PopBackStackType { }
            public struct PopBackStackToPanelType { }
            public struct CloseAllPopupsType { }
            public struct ClosePopupType { }
            public struct HasActivePopupsType { }
            public struct CanGoBackType { }
// @formatter:on
#pragma warning restore SA1515
#pragma warning restore SA1516
#pragma warning restore SA1502
        }
    }
}
