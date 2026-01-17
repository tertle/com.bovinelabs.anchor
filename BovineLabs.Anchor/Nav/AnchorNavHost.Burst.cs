// <copyright file="AnchorNavHost.Burst.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using AOT;
    using BovineLabs.Core.Utility;
    using Unity.Burst;
    using Unity.Collections;

    public partial class AnchorNavHost
    {
        static AnchorNavHost()
        {
            Burst.NavigateFunc.Data = new BurstTrampoline<FixedString32Bytes>(NavigateForwarding);
            Burst.CurrentFunc.Data = new BurstTrampolineOut<FixedString32Bytes>(CurrentForwarding);
            Burst.ClearBackStackFunc.Data = new BurstTrampoline(ClearBackStackForwarding);
            Burst.ClearNavigationFunc.Data = new BurstTrampoline<FixedString32Bytes>(ClearNavigationForwarding);
            Burst.PopBackStackFunc.Data = new BurstTrampolineOut<bool>(PopBackStackForwarding);
            Burst.PopBackStackToPanelFunc.Data = new BurstTrampolineOut<bool>(PopBackStackToPanelForwarding);
            Burst.CloseAllPopupsFunc.Data = new BurstTrampolineOut<FixedString32Bytes, bool>(CloseAllPopupsForwarding);
            Burst.ClosePopupFunc.Data = new BurstTrampolineOut<FixedString32Bytes, FixedString32Bytes, bool>(ClosePopupForwarding);
            Burst.HasActivePopupsFunc.Data = new BurstTrampolineOut<bool>(HasActivePopupsForwarding);
            Burst.CanGoBackFunc.Data = new BurstTrampolineOut<bool>(CanGoBackForwarding);
        }

        [MonoPInvokeCallback(typeof(BurstTrampoline<FixedString32Bytes>.Delegate))]
        private static void NavigateForwarding(in FixedString32Bytes screen)
        {
            AnchorApp.current.NavHost.Navigate(screen.ToString());
        }

        [MonoPInvokeCallback(typeof(BurstTrampoline.Delegate))]
        private static void ClearBackStackForwarding()
        {
            AnchorApp.current.NavHost.ClearBackStack();
        }

        [MonoPInvokeCallback(typeof(BurstTrampoline<FixedString32Bytes>.Delegate))]
        private static void ClearNavigationForwarding(in FixedString32Bytes exitAnimation)
        {
            AnchorApp.current.NavHost.ClearNavigation(exitAnimation);
        }

        [MonoPInvokeCallback(typeof(BurstTrampolineOut<bool>.Delegate))]
        private static void PopBackStackForwarding(out bool popped)
        {
            popped = AnchorApp.current.NavHost.PopBackStack();
        }

        [MonoPInvokeCallback(typeof(BurstTrampolineOut<bool>.Delegate))]
        private static void PopBackStackToPanelForwarding(out bool popped)
        {
            popped = AnchorApp.current.NavHost.PopBackStackToPanel();
        }

        [MonoPInvokeCallback(typeof(BurstTrampolineOut<FixedString32Bytes, bool>.Delegate))]
        private static void CloseAllPopupsForwarding(in FixedString32Bytes exitAnimation, out bool closed)
        {
            closed = AnchorApp.current.NavHost.CloseAllPopups(exitAnimation);
        }

        [MonoPInvokeCallback(typeof(BurstTrampolineOut<FixedString32Bytes, FixedString32Bytes, bool>.Delegate))]
        private static void ClosePopupForwarding(in FixedString32Bytes destination, in FixedString32Bytes exitAnimation, out bool closed)
        {
            closed = AnchorApp.current.NavHost.ClosePopup(destination.ToString(), exitAnimation);
        }

        [MonoPInvokeCallback(typeof(BurstTrampolineOut<bool>.Delegate))]
        private static void HasActivePopupsForwarding(out bool hasActivePopups)
        {
            hasActivePopups = AnchorApp.current.NavHost.HasActivePopups;
        }

        [MonoPInvokeCallback(typeof(BurstTrampolineOut<bool>.Delegate))]
        private static void CanGoBackForwarding(out bool canGoBack)
        {
            canGoBack = AnchorApp.current.NavHost.CanGoBack;
        }

        [MonoPInvokeCallback(typeof(BurstTrampolineOut<FixedString32Bytes>.Delegate))]
        private static void CurrentForwarding(out FixedString32Bytes name)
        {
            name = AnchorApp.current.NavHost.CurrentDestination ?? default(FixedString32Bytes);
        }

        public static class Burst
        {
            internal static readonly SharedStatic<BurstTrampoline<FixedString32Bytes>> NavigateFunc =
                SharedStatic<BurstTrampoline<FixedString32Bytes>>.GetOrCreate<AnchorNavHost, NavigateType>();

            internal static readonly SharedStatic<BurstTrampolineOut<FixedString32Bytes>> CurrentFunc =
                SharedStatic<BurstTrampolineOut<FixedString32Bytes>>.GetOrCreate<AnchorNavHost, CurrentType>();

            internal static readonly SharedStatic<BurstTrampoline> ClearBackStackFunc =
                SharedStatic<BurstTrampoline>.GetOrCreate<AnchorNavHost, ClearBackStackType>();

            internal static readonly SharedStatic<BurstTrampoline<FixedString32Bytes>> ClearNavigationFunc =
                SharedStatic<BurstTrampoline<FixedString32Bytes>>.GetOrCreate<AnchorNavHost, ClearNavigationType>();

            internal static readonly SharedStatic<BurstTrampolineOut<bool>> PopBackStackFunc =
                SharedStatic<BurstTrampolineOut<bool>>.GetOrCreate<AnchorNavHost, PopBackStackType>();

            internal static readonly SharedStatic<BurstTrampolineOut<bool>> PopBackStackToPanelFunc =
                SharedStatic<BurstTrampolineOut<bool>>.GetOrCreate<AnchorNavHost, PopBackStackToPanelType>();

            internal static readonly SharedStatic<BurstTrampolineOut<FixedString32Bytes, bool>> CloseAllPopupsFunc =
                SharedStatic<BurstTrampolineOut<FixedString32Bytes, bool>>.GetOrCreate<AnchorNavHost, CloseAllPopupsType>();

            internal static readonly SharedStatic<BurstTrampolineOut<FixedString32Bytes, FixedString32Bytes, bool>> ClosePopupFunc =
                SharedStatic<BurstTrampolineOut<FixedString32Bytes, FixedString32Bytes, bool>>.GetOrCreate<AnchorNavHost, ClosePopupType>();

            internal static readonly SharedStatic<BurstTrampolineOut<bool>> HasActivePopupsFunc =
                SharedStatic<BurstTrampolineOut<bool>>.GetOrCreate<AnchorNavHost, HasActivePopupsType>();

            internal static readonly SharedStatic<BurstTrampolineOut<bool>> CanGoBackFunc =
                SharedStatic<BurstTrampolineOut<bool>>.GetOrCreate<AnchorNavHost, CanGoBackType>();

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
                    CurrentFunc.Data.Invoke(out var name);
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
            public static void ClearNavigation()
            {
                if (ClearNavigationFunc.Data.IsCreated)
                {
                    ClearNavigationFunc.Data.Invoke(default);
                }
            }

            /// <inheritdoc cref="AnchorNavHost.ClearNavigation" />
            public static void ClearNavigation(in FixedString32Bytes exitAnimation)
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
                    PopBackStackFunc.Data.Invoke(out var popped);
                    return popped;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.PopBackStackToPanel" />
            public static bool PopBackStackToPanel()
            {
                if (PopBackStackToPanelFunc.Data.IsCreated)
                {
                    PopBackStackToPanelFunc.Data.Invoke(out var popped);
                    return popped;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.CloseAllPopups" />
            public static bool CloseAllPopups()
            {
                if (CloseAllPopupsFunc.Data.IsCreated)
                {
                    CloseAllPopupsFunc.Data.Invoke(default, out var closed);
                    return closed;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.CloseAllPopups" />
            public static bool CloseAllPopups(in FixedString32Bytes exitAnimation)
            {
                if (CloseAllPopupsFunc.Data.IsCreated)
                {
                    CloseAllPopupsFunc.Data.Invoke(exitAnimation, out var closed);
                    return closed;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.ClosePopup(string)" />
            public static bool ClosePopup(in FixedString32Bytes destination)
            {
                if (ClosePopupFunc.Data.IsCreated)
                {
                    ClosePopupFunc.Data.Invoke(destination, default, out var closed);
                    return closed;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.ClosePopup" />
            public static bool ClosePopup(in FixedString32Bytes destination, in FixedString32Bytes exitAnimation)
            {
                if (ClosePopupFunc.Data.IsCreated)
                {
                    ClosePopupFunc.Data.Invoke(destination, exitAnimation, out var closed);
                    return closed;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.HasActivePopups" />
            public static bool HasActivePopups()
            {
                if (HasActivePopupsFunc.Data.IsCreated)
                {
                    HasActivePopupsFunc.Data.Invoke(out var hasActivePopups);
                    return hasActivePopups;
                }

                return false;
            }

            /// <inheritdoc cref="AnchorNavHost.CanGoBack" />
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
            public struct ClearNavigationType { }
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
