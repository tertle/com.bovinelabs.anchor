# Navigation

Anchor navigation is managed by `BovineLabs.Anchor.Nav.AnchorNavHost`. The host instantiates registered UXML destinations, keeps the active and back stacks, presents popup layers, forwards route arguments to bound view models, and exposes managed and Burst-compatible entry points.

For application and service-container setup, see [App and services](app-and-services.md).

## How navigation is initialized

`AnchorApp.Initialize()` performs the standard setup:

1. It creates `new AnchorNavHost(AnchorSettings.I.Actions, AnchorSettings.I.Animations)`.
2. It adds the host to the app root visual element.
3. If `AnchorSettings.StartDestination` is not empty, it navigates to that destination.

The resulting managed access point is:

```csharp
IAnchorNavHost nav = AnchorApp.Current.NavHost;
```

`AnchorAppBuilder` captures an opaque navigation reload state before replacing a panel visual generation. The replacement host restores the active and back stacks, popup layering, and outstanding state handles through the same `IAnchorNavHost` contract.

If you construct `AnchorNavHost` yourself, the parameterless constructor discovers attributed code actions, but it does not read action or animation assets from `AnchorSettings`. Pass those collections to `AnchorNavHost(IEnumerable<AnchorAction>, IEnumerable<AnchorNavAnimation>)` when the custom host needs them.

## Register destinations

A destination is a string key mapped to a `VisualTreeAsset` in **BovineLabs > Settings > Anchor > Views**. The default `UXMLService` looks up that key, instantiates the asset, and resolves every UXML `data-source-type` from `AnchorApp.Current.Services`.

For example, register the key `profile` against a profile UXML asset, then navigate with:

```csharp
using BovineLabs.Anchor;
using BovineLabs.Anchor.Nav;

AnchorApp.Current.NavHost.Navigate(
    "profile",
    AnchorNavArgument.String("userId", "42"));
```

Destination keys should be non-empty and unique. Action names and destination keys share the first `Navigate` overload's lookup space: a registered action with the same name is resolved before the destination key.

The host does not validate that a non-empty destination exists before attempting to instantiate it. A missing key is logged by `UXMLService`, after which instantiation cannot continue. Keep settings and route constants in sync.

Set **Start Destination** to a registered destination when the app should open on a particular screen. Leave it empty when another system owns initial navigation.

## Navigate directly

The managed navigation overloads are:

```csharp
bool Navigate(string actionOrDestination, params AnchorNavArgument[] arguments);

bool Navigate(
    string destination,
    AnchorNavOptions options,
    params AnchorNavArgument[] arguments);
```

The first overload resolves a named action and falls back to treating the string as a destination key. The second overload always treats its first argument as a destination and applies the supplied options directly.

```csharp
var nav = AnchorApp.Current.NavHost;

// Direct destination with default options.
nav.Navigate("home");

// Direct destination with explicit stack behavior.
nav.Navigate(
    "results",
    new AnchorNavOptions
    {
        StackStrategy = AnchorStackStrategy.PopAll,
    },
    AnchorNavArgument.String("matchId", "1042"));
```

Both overloads return `false` for a null, empty, or whitespace destination. Popup validation can also return `false`, such as `EnsureBaseAndPopup` without a base destination.

Assigning `CurrentDestination` does not navigate or change either stack. The public setter only changes the reported value and raises `DestinationChanged`; call `Navigate` to display a destination.

## Register named actions

A named action packages a destination, navigation options, and default arguments. Named actions are useful for commands such as `open-settings`, `replace-with-results`, or `toggle-inventory`, where callers should not repeat stack and popup policy.

There are two registration paths.

### Action assets

Author `AnchorAction` assets and include them in `AnchorSettings.Actions`. Each asset contains an action name and an `AnchorNavAction`. The standard `AnchorApp` passes these assets to the host during initialization.

### Attributed code actions

The host discovers static, parameterless methods marked with `[AnchorNavAction("name")]`. The method must return a non-null `AnchorNavAction`.

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor.Nav;

    public static class NavigationActions
    {
        [AnchorNavAction("open-profile")]
        public static AnchorNavAction OpenProfile()
        {
            return new AnchorNavAction(
                "profile",
                new AnchorNavOptions(),
                new[]
                {
                    AnchorNavArgument.String("tab", "summary"),
                });
        }

        [AnchorNavAction("toggle-inventory")]
        public static AnchorNavAction ToggleInventory()
        {
            return new AnchorNavAction(
                "inventory-popup",
                new AnchorNavOptions
                {
                    PopupStrategy = AnchorPopupStrategy.PopupOnCurrent,
                    PopupExistingStrategy = AnchorPopupExistingStrategy.CloseOtherPopups,
                });
        }
    }
}
```

Call an action by name through the same API:

```csharp
nav.Navigate("open-profile");
nav.Toggle("toggle-inventory");
```

Action resolution has these behaviors:

- `AnchorNavAction.Options` is cloned for each request.
- Default arguments are applied first.
- Explicit arguments replace defaults with the same case-sensitive `Name`; new names are appended.
- Resolving an action raises `ActionTriggered`. Direct destination navigation does not.
- Attributed actions are registered before settings assets. All action names must therefore be unique across both sources.
- A registered action name shadows a destination key with the same name in `Navigate(string, ...)` and `Toggle(string, ...)`. Use the options overload to force direct destination navigation.

The public action constructor and merge method are:

```csharp
AnchorNavAction(
    string destination,
    AnchorNavOptions options,
    IEnumerable<AnchorNavArgument> defaultArguments = null);

AnchorNavArgument[] MergeArguments(params AnchorNavArgument[] arguments);
```

## Stack options

`AnchorNavOptions.StackStrategy` adjusts the back stack before the new non-popup destination becomes active.

| Strategy | Behavior |
| --- | --- |
| `AnchorStackStrategy.None` | Leaves existing back entries in place. |
| `AnchorStackStrategy.PopToSpecificDestination` | Pops until `PopupToDestination` is the top back entry. If the target is absent, the current implementation empties the back stack. |
| `AnchorStackStrategy.PopToRoot` | Pops until the oldest back entry is on top. |
| `AnchorStackStrategy.PopAll` | Clears every back entry before activating the destination. |

Despite its name, `AnchorNavOptions.PopupToDestination` is the target used by `PopToSpecificDestination`; it is not a popup destination setting.

A normal transition snapshots the current active stack before it activates the new panel. That snapshot can include popup layers. The main back-stack API is:

```csharp
bool CanGoBack { get; }

bool PopBackStack();
bool PopBackStackToPanel();
void ClearBackStack();
void ClearNavigation(int exitAnimation = 0);
```

- `PopBackStack()` restores the previous snapshot, including any popup layers captured in it.
- `PopBackStackToPanel()` removes popup layers from the snapshot it restores. If there is no back entry, it attempts `CloseAllPopups()` instead.
- `ClearBackStack()` leaves the current destination and active visual stack untouched.
- `ClearNavigation()` removes the active stack and clears the back stack, leaving `CurrentDestination` null.

`CurrentDestination` normally reports the top active entry. While a popup is open, it is the popup destination rather than the underlying panel. Use `HasActivePopups` to test for overlay state.

> **Current behavior:** after `PopBackStackToPanel()` strips popup layers from a stored snapshot, `HasActivePopups` is false, but `CurrentDestination` remains the popped back-stack entry's destination. Do not use `CurrentDestination` alone to infer whether a popup is active.

## Popups

Anchor popups are layers in the nav host's active visual stack. They are distinct from `AnchorApp.PopupContainer` and preserve the underlying destination while displayed.

Configure popup navigation through `AnchorNavOptions.PopupStrategy`:

| Strategy | Behavior |
| --- | --- |
| `AnchorPopupStrategy.None` | Performs normal destination navigation. |
| `AnchorPopupStrategy.PopupOnCurrent` | Adds the destination above the active stack. With no active destination, Anchor logs a warning and falls back to normal navigation. |
| `AnchorPopupStrategy.EnsureBaseAndPopup` | Ensures `PopupBaseDestination` is the active base, using `PopupBaseArguments` when it must navigate there, then adds the popup. A blank base destination logs an error and returns `false`. |

`PopupExistingStrategy` controls what happens when another popup is already active:

| Strategy | Behavior |
| --- | --- |
| `AnchorPopupExistingStrategy.None` | Keeps existing popups and adds the new one. Re-navigating to the same top popup updates it instead of adding a duplicate layer. |
| `AnchorPopupExistingStrategy.CloseOtherPopups` | Removes existing popup layers, keeps the base, and displays the new popup. |
| `AnchorPopupExistingStrategy.PushNew` | Saves the full current stack, rebuilds the base plus the new popup, and allows `PopBackStack()` to restore the previous popup stack. |

Example:

```csharp
nav.Navigate(
    "confirm-delete",
    new AnchorNavOptions
    {
        PopupStrategy = AnchorPopupStrategy.EnsureBaseAndPopup,
        PopupBaseDestination = "inventory",
        PopupBaseArguments = new[]
        {
            AnchorNavArgument.String("category", "weapons"),
        },
        PopupExistingStrategy = AnchorPopupExistingStrategy.CloseOtherPopups,
    },
    AnchorNavArgument.String("itemId", "sword-01"));
```

### Toggle and close

```csharp
bool Toggle(string actionOrDestination, params AnchorNavArgument[] arguments);
bool ClosePopup(string destination, int exitAnimation = 0);
bool CloseAllPopups(int exitAnimation = 0);
bool HasActivePopups { get; }
```

`Toggle` resolves an action or destination. If the resolved destination is in the active popup segment, it closes that popup and every popup above it. Otherwise, it navigates using the resolved action's options.

To make one call both open and close a popup, prefer a named action whose `PopupStrategy` is configured. A bare destination has default options when resolved by `Toggle`, so `Toggle("inventory-popup")` opens it as a normal panel unless an action with that name supplies popup options.

The close path used by `Toggle` does not play the action's exit animation. Use `ClosePopup(destination, exitAnimation)` or `CloseAllPopups(exitAnimation)` when dismissal needs a registered animation ID.

`ClosePopup` only searches currently active popup layers and removes the matching layer. It requires the popup destination; there is no parameterless overload. `CloseAllPopups` removes all active popup layers and restores the base. Both return `false` when there is nothing matching to close.

## Arguments and screen callbacks

Navigation arguments are immutable name/value string pairs:

```csharp
new AnchorNavArgument(string name, string value);
AnchorNavArgument.String(string name, string value);
AnchorNavArgument.From(string name, string value);
```

`String` and `From` currently have the same behavior. There are no typed overloads, so parse numeric or structured values in the receiver.

Implement `IAnchorNavigationScreen` on a view model that needs enter and exit callbacks:

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;

    [IsService]
    public partial class ProfileViewModel : ObservableObject, IAnchorNavigationScreen
    {
        [ObservableProperty]
        private string userId;

        void IAnchorNavigationScreen.OnEnter(AnchorNavArgument[] args)
        {
            foreach (var argument in args)
            {
                if (argument.Name == "userId")
                {
                    this.UserId = argument.Value;
                }
            }
        }

        void IAnchorNavigationScreen.OnExit(AnchorNavArgument[] args)
        {
        }
    }
}
```

Bind the view model in the destination UXML:

```xml
<ui:VisualElement
    xmlns:ui="UnityEngine.UIElements"
    data-source-type="MyGame.UI.ProfileViewModel, MyGame">
    <!-- Screen contents. -->
</ui:VisualElement>
```

When a destination enters or exits, Anchor queries the instantiated tree and calls every bound data source that implements `IAnchorNavigationScreen`:

```csharp
void OnEnter(AnchorNavArgument[] args);
void OnExit(AnchorNavArgument[] args);
```

If non-empty arguments are supplied but no bound screen handles them, Anchor logs a warning. Register the view model as a service and verify the UXML `data-source-type` rather than suppressing the warning.

Navigating to the same active destination with different arguments reuses the visual element and calls `OnEnter` again with the new arguments. It does not call `OnExit` first.

## Host events

`IAnchorNavHost` exposes these managed events:

```csharp
event Action<AnchorNavHost, VisualElement, AnchorNavArgument[]> EnteredDestination;
event Action<AnchorNavHost, VisualElement, AnchorNavArgument[]> ExitedDestination;
event Action<AnchorNavHost, AnchorNavAction> ActionTriggered;
event Action<AnchorNavHost, string> DestinationChanged;
```

- `EnteredDestination` and `ExitedDestination` provide the instantiated root and that entry's arguments.
- Screen `OnEnter` and `OnExit` callbacks run before their corresponding host event.
- During a normal panel transition, exit notifications occur before enter notifications.
- `DestinationChanged` fires only when the string value actually changes.
- `ActionTriggered` fires when a named action resolves, including a `Toggle` request that closes an already-active popup.

## Save and restore state

Use a direct snapshot when managed code owns its lifetime:

```csharp
AnchorNavHostSaveState state = nav.SaveState();

nav.ClearNavigation();
nav.RestoreState(state);
```

The exact methods are:

```csharp
AnchorNavHostSaveState SaveState();
void RestoreState(AnchorNavHostSaveState state);
```

The snapshot contains the active stack, back stack, popup flags, arguments, options, and current pop-animation references. Restoration recreates destination visual trees without navigation animations and replays the normal exit/enter callbacks for removed and restored elements.

`AnchorNavHostSaveState` is an in-memory snapshot object. It is not marked with Unity or .NET serialization attributes and should not be treated as a save-game format.

State handles provide the same operation without passing a managed snapshot through Burst-compatible code:

```csharp
int handle = nav.SaveStateHandle();

nav.Navigate("temporary-screen");

bool restored = nav.ReleaseStateHandle(handle, restore: true);
```

```csharp
int SaveStateHandle();
bool ReleaseStateHandle(int handle, bool restore = true);
```

Handles start above zero, increase for the lifetime of a host, and are consumed by `ReleaseStateHandle`. Passing `restore: false` discards the snapshot without changing navigation. A raw handle is owned by the host that created it; visual reload transfers the complete handle table through an opaque reload state:

```csharp
IAnchorNavHostReloadState reloadState = nav.CaptureReloadState();
replacementNav.RestoreReloadState(reloadState);
```

`CaptureReloadState` and `RestoreReloadState` are implementation-owned memento operations. Callers transfer the opaque state without inspecting it, and both operations are required by `IAnchorNavHost`.

## Animations

`AnchorNavOptions.Animations` is an `AnchorAnimations` set with four asset references:

| Property | Phase |
| --- | --- |
| `EnterAnim` | The new top destination entering during forward navigation. |
| `ExitAnim` | Entries leaving during forward navigation. |
| `PopEnterAnim` | The restored destination entering when this navigation is later popped. |
| `PopExitAnim` | The current destination leaving when this navigation is later popped. |

The package includes `FadeInAnimation`, `FadeOutAnimation`, `ScaleFadeInAnimation`, and `ScaleFadeOutAnimation` in `BovineLabs.Anchor.Nav.Animations`. Animation assets derive from `AnchorNavAnimation`; their duration is stored in milliseconds. Fade assets default to 150 ms when reset, and scale-fade assets default to 500 ms.

Add animation assets to `AnchorSettings.Animations` when they must be resolved by integer ID. ID-based resolution is used by:

```csharp
bool TryGetAnimation(int id, out AnchorNavAnimation animation);
void ClearNavigation(int exitAnimation = 0);
bool ClosePopup(string destination, int exitAnimation = 0);
bool CloseAllPopups(int exitAnimation = 0);
```

ID `0` means no animation and resolves successfully to `null`. An unknown non-zero ID logs a warning and falls back to no animation. Keep `AnchorNavOptions.Animations` non-null; the default constructor initializes it.

## Burst entry points

Use `AnchorNavHost.Burst` from Burst-compiled ECS code instead of dereferencing `AnchorApp.Current`:

```csharp
using BovineLabs.Anchor.Nav;
using Unity.Collections;

var action = new FixedString32Bytes("open-pause");
AnchorNavHost.Burst.Navigate(action);

if (AnchorNavHost.Burst.CanGoBack())
{
    AnchorNavHost.Burst.PopBackStack();
}
```

The complete Burst surface is:

```csharp
static void Navigate(in FixedString32Bytes screen);
static bool Toggle(in FixedString32Bytes actionOrDestination);
static FixedString32Bytes CurrentDestination();
static void ClearBackStack();
static void ClearNavigation(in int exitAnimation = 0);
static bool PopBackStack();
static bool PopBackStackToPanel();
static bool CloseAllPopups(int exitAnimation = 0);
static bool ClosePopup(in FixedString32Bytes destination, in int exitAnimation = 0);
static bool HasActivePopups();
static bool CanGoBack();
static int SaveStateHandle();
static void ReleaseStateHandle(int handle, bool restore = true);
```

Burst route names must fit in `FixedString32Bytes`. The Burst `Navigate` and `Toggle` overloads do not accept arguments or direct `AnchorNavOptions`; use a registered named action when the call needs default options or default arguments.

These methods cross to the managed host through initialized trampolines. If no current app or nav host exists, or a trampoline is not initialized, commands are no-ops and query methods return `false`, `0`, or a default `FixedString32Bytes`. In particular, handle `0` means no state was captured.

`NavigationStateSystem` also reads `AnchorNavHost.Burst.CurrentDestination()` and maps configured `UISystemTypes` destination names to ECS components on the UI system entity. Use that settings map when systems should start or stop according to the top navigation destination. Because a popup becomes `CurrentDestination`, configure popup mappings deliberately.

## Common mistakes

- Setting `CurrentDestination` instead of calling `Navigate` changes only the reported string.
- Calling `Toggle` with a bare destination opens a normal panel unless a named action supplies popup options.
- Calling `ClosePopup()` without a destination does not compile; use `ClosePopup("destination")` or `CloseAllPopups()`.
- Assuming `CurrentDestination` is the underlying panel while a popup is active produces incorrect state; check `HasActivePopups`.
- Passing arguments to a screen with no bound `IAnchorNavigationScreen` produces a warning.
- Using duplicate action names or an action name that unintentionally shadows a destination makes route resolution ambiguous.
- Using `PopToSpecificDestination` with a target that is not already in the back stack empties the back stack.
- Persisting `AnchorNavHostSaveState` or carrying a raw state handle into a different host without its reload state is unsupported.
