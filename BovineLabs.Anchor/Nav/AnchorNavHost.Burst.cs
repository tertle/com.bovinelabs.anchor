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
            Burst.NavigateFunc.Data = new BurstDelegate<FixedString32Bytes>(NavigateForwarding);
            Burst.CurrentFunc.Data = new BurstOutDelegate<FixedString32Bytes>(CurrentForwarding);
        }

        [MonoPInvokeCallback(typeof(BurstDelegate<FixedString32Bytes>.ChangedDelegate))]
        private static void NavigateForwarding(in FixedString32Bytes screen)
        {
            AnchorApp.current.NavHost.Navigate(screen.ToString());
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

            /// <summary> A burst compatible way to Navigate to a new screen. </summary>
            /// <param name="screen"> The screen to navigate to. </param>
            public static void Navigate(in FixedString32Bytes screen)
            {
                if (NavigateFunc.Data.IsCreated)
                {
                    NavigateFunc.Data.Invoke(screen);
                }
            }

            /// <summary> A burst compatible way to get the <see cref="currentDestination"/>. </summary>
            /// <returns>The name of the current destination, or default if null.</returns>
            public static FixedString32Bytes CurrentDestination()
            {
                if (CurrentFunc.Data.IsCreated)
                {
                    CurrentFunc.Data.Invoke(out var name);
                    return name;
                }

                return default;
            }

#pragma warning disable SA1502
#pragma warning disable SA1516
#pragma warning disable SA1515
// @formatter:off
            public struct NavigateType { }
            public struct CurrentType { }
// @formatter:on
#pragma warning restore SA1515
#pragma warning restore SA1516
#pragma warning restore SA1502
        }
    }
}
