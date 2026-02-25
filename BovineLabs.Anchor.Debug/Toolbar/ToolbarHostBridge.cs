// <copyright file="ToolbarHostBridge.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using BovineLabs.Anchor.Toolbar;

    /// <summary>
    /// Global bridge used by toolbar systems/helpers to access the active toolbar host.
    /// </summary>
    public static class ToolbarHostBridge
    {
        /// <summary>Gets the currently active toolbar host.</summary>
        public static IAnchorToolbarHost Host { get; private set; }

        /// <summary>Gets whether a host is available and ready.</summary>
        public static bool IsReady => Host is { IsReady: true };

        /// <summary>
        /// Registers a host instance.
        /// </summary>
        /// <param name="host">Host to register.</param>
        public static void Register(IAnchorToolbarHost host)
        {
            Host = host;
        }

        /// <summary>
        /// Unregisters the host if it matches the current one.
        /// </summary>
        /// <param name="host">Host instance being removed.</param>
        public static void Unregister(IAnchorToolbarHost host)
        {
            if (ReferenceEquals(Host, host))
            {
                Host = null;
            }
        }
    }
}
