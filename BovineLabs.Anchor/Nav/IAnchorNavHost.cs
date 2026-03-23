// <copyright file="IAnchorNavHost.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine.UIElements;

    /// <summary>
    /// Contract for Anchor navigation operations without exposing the full <see cref="VisualElement"/> surface.
    /// </summary>
    public interface IAnchorNavHost
    {
        /// <summary>Event that is triggered when a new destination is entered.</summary>
        event Action<AnchorNavHost, VisualElement, AnchorNavArgument[]> EnteredDestination;

        /// <summary>Event that is triggered when a destination is exited.</summary>
        event Action<AnchorNavHost, VisualElement, AnchorNavArgument[]> ExitedDestination;

        /// <summary>Event that is invoked when an action is triggered.</summary>
        event Action<AnchorNavHost, AnchorNavAction> ActionTriggered;

        /// <summary>Event that is invoked when the current destination changes.</summary>
        event Action<AnchorNavHost, string> DestinationChanged;

        /// <summary>Gets a value indicating whether there is a destination on the back stack that can be popped.</summary>
        bool CanGoBack { get; }

        /// <summary>Gets a value indicating whether there are popup overlays active on top of the base panel.</summary>
        bool HasActivePopups { get; }

        /// <summary>Gets or sets the current destination.</summary>
        string CurrentDestination { get; set; }

        /// <summary>Try to resolve a registered animation by id.</summary>
        /// <param name="id">The animation id.</param>
        /// <param name="animation">The animation definition.</param>
        /// <returns>True if the animation was found.</returns>
        bool TryGetAnimation(int id, out AnchorNavAnimation animation);

        /// <summary>Clear the back stack entirely.</summary>
        void ClearBackStack();

        /// <summary>Clear the active stack and back stack so no destination remains active.</summary>
        /// <param name="exitAnimation">Optional animation to use when removing the last active entry.</param>
        void ClearNavigation(int exitAnimation = 0);

        /// <summary>Navigate to the destination or action with the given name.</summary>
        /// <param name="actionOrDestination">The name of the action or destination.</param>
        /// <param name="arguments">The arguments to pass to the destination.</param>
        /// <returns>True if the navigation was successful.</returns>
        bool Navigate(string actionOrDestination, params AnchorNavArgument[] arguments);

        /// <summary>Navigate to the destination with the provided options.</summary>
        /// <param name="destination">The destination.</param>
        /// <param name="options">The options to use for the navigation.</param>
        /// <param name="arguments">The arguments to pass to the destination.</param>
        /// <returns>True if the navigation was successful.</returns>
        bool Navigate(string destination, AnchorNavOptions options, params AnchorNavArgument[] arguments);

        /// <summary>Pop the current destination from the back stack and navigate to the previous destination.</summary>
        /// <returns>True if the back stack was popped; otherwise, false.</returns>
        bool PopBackStack();

        /// <summary>Pop the current destination and clear any popup overlays that were captured with it.</summary>
        /// <returns>True if the back stack or popup stack was updated; otherwise, false.</returns>
        bool PopBackStackToPanel();

        /// <summary>Close all currently-displayed popup overlays.</summary>
        /// <param name="exitAnimation">Optional animation id to play when dismissing each popup.</param>
        /// <returns>True if at least one popup was closed.</returns>
        bool CloseAllPopups(int exitAnimation = 0);

        /// <summary>Close a popup in the active stack that matches the provided destination.</summary>
        /// <param name="destination">The destination key of the popup to close.</param>
        /// <param name="exitAnimation">Optional animation id to play when dismissing the popup.</param>
        /// <returns>True if the popup was closed; otherwise, false.</returns>
        bool ClosePopup(string destination, int exitAnimation = 0);

        /// <summary>Captures the current visual stack, back stack, and popup configuration so it can be restored later.</summary>
        /// <returns>A snapshot containing the navigation state.</returns>
        AnchorNavHostSaveState SaveState();

        /// <summary>Restores a previously captured navigation snapshot.</summary>
        /// <param name="state">Snapshot created via <see cref="SaveState"/>.</param>
        void RestoreState(AnchorNavHostSaveState state);

        /// <summary>Captures a navigation state snapshot and returns a handle to restore it later.</summary>
        /// <returns>The handle.</returns>
        int SaveStateHandle();

        /// <summary>Releases a navigation state snapshot, optionally restoring it first.</summary>
        /// <param name="handle">The handle to release.</param>
        /// <param name="restore">Should the UI state also be restored to the saved snapshot.</param>
        /// <returns>True if the handle existed.</returns>
        bool ReleaseStateHandle(int handle, bool restore = true);
    }
}
