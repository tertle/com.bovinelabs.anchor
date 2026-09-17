namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine.UIElements;

    public interface IAnchorNavHost
    {
        event Action<AnchorNavHost, VisualElement, AnchorNavArgument[]> EnteredDestination;

        event Action<AnchorNavHost, VisualElement, AnchorNavArgument[]> ExitedDestination;

        event Action<AnchorNavHost, AnchorNavAction> ActionTriggered;

        event Action<AnchorNavHost, string> DestinationChanged;

        bool CanGoBack { get; }

        bool HasActivePopups { get; }

        /// <summary>
        /// Setting this value does not navigate or update either stack.
        /// </summary>
        string CurrentDestination { get; set; }

        bool TryGetAnimation(int id, out AnchorNavAnimation animation);

        void ClearBackStack();

        void ClearNavigation(int exitAnimation = 0);

        bool Navigate(string actionOrDestination, AnchorNavArgument argument);

        bool Navigate(string actionOrDestination, AnchorNavArgument[] arguments = null);

        bool Navigate(string destination, AnchorNavOptions options, AnchorNavArgument argument);

        bool Navigate(string destination, AnchorNavOptions options, AnchorNavArgument[] arguments = null);

        /// <summary>
        /// If active, dismisses the matching popup and every popup above it; otherwise navigates to it.
        /// </summary>
        bool Toggle(string actionOrDestination, AnchorNavArgument argument);

        /// <summary>
        /// If active, dismisses the matching popup and every popup above it; otherwise navigates to it.
        /// </summary>
        bool Toggle(string actionOrDestination, AnchorNavArgument[] arguments = null);

        bool PopBackStack();

        /// <summary>
        /// Also clears popup overlays captured with the destination.
        /// </summary>
        bool PopBackStackToPanel();

        bool CloseAllPopups(int exitAnimation = 0);

        bool ClosePopup(string destination, int exitAnimation = 0);

        AnchorNavHostSaveState SaveState();

        void RestoreState(AnchorNavHostSaveState state);

        int SaveStateHandle();

        bool ReleaseStateHandle(int handle, bool restore = true);

        IAnchorNavHostReloadState CaptureReloadState();

        /// <summary>
        /// State must come from the previous instance of the same host implementation.
        /// </summary>
        void RestoreReloadState(IAnchorNavHostReloadState state);
    }
}
