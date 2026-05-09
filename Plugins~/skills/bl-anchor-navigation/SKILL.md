---
name: bl-anchor-navigation
description: "Use when creating, wiring, refactoring, or debugging com.bovinelabs.anchor navigation, including AnchorNavHost, destination and action routing, popup semantics, back-stack behavior, navigation arguments, IAnchorNavigationScreen callbacks, animations, save/restore state, and Burst navigation entry points."
---

# Anchor Navigation

Use this skill for `AnchorNavHost` behavior and callers. Resolve Anchor source from the installed package root; in source checkouts this is commonly `Packages/com.bovinelabs.anchor`.

## Workflow

1. Inspect `IAnchorNavHost`, `AnchorNavHost.Navigation`, `AnchorNavHost.State`, `AnchorNavHost.Burst`, and the affected tests before changing behavior.
2. Decide whether the caller should use a destination key directly or a named `AnchorNavAction`.
3. Keep popup behavior in `AnchorNavOptions`; callers should not maintain a separate popup-open boolean when `Toggle` or `ClosePopup` expresses the operation.
4. Use `AnchorNavArgument` for route arguments and implement `IAnchorNavigationScreen` on the bound view model when enter/exit callbacks need those arguments.
5. Use `AnchorNavHost.Burst` for Burst-compatible navigation calls; use managed `AnchorApp.Current.NavHost` only from managed UI code.
6. Validate navigation changes with targeted `BovineLabs.Anchor.Tests` nav tests first.

## Actions And Destinations

- Destinations are UXML keys registered in `AnchorSettings.Views`.
- Serializable actions live in `AnchorSettings.Actions` as `AnchorAction` assets.
- Code actions can be registered with a static parameterless method marked `[AnchorNavAction("name")]` that returns `AnchorNavAction`.
- `Navigate(actionOrDestination, args)` first resolves a named action, then falls back to a destination key.
- `AnchorNavAction.MergeArguments` applies default arguments first and lets explicit arguments replace matching names.

## Popup Semantics

- Use `Toggle(actionOrDestination, args)` for toolbar/menu popup buttons. It resolves actions the same way as `Navigate`, closes the matching active popup branch when present, and otherwise navigates.
- `AnchorPopupStrategy.PopupOnCurrent` overlays a popup on the active visual stack.
- `AnchorPopupStrategy.EnsureBaseAndPopup` first ensures `PopupBaseDestination`, then overlays the popup. It requires a non-empty base destination.
- `AnchorPopupExistingStrategy.CloseOtherPopups` removes current popups before showing the new popup.
- `AnchorPopupExistingStrategy.PushNew` archives the current stack and rebuilds the base/popup branch.
- When a popup is active, `CurrentDestination` is the popup destination. Use `HasActivePopups` or `ClosePopup` instead of inferring base-panel state from `CurrentDestination`.

## Back Stack And State

- Normal navigation pushes the current snapshot onto the back stack before activating the new panel.
- `ClearBackStack()` leaves the current destination active.
- `ClearNavigation(exitAnimation)` clears the active stack and back stack.
- `PopBackStack()` restores the previous snapshot, including popup layers captured with it.
- `PopBackStackToPanel()` restores the previous snapshot without popup layers.
- `ClosePopup(destination, exitAnimation)` removes a matching active popup and updates the current destination.
- `CloseAllPopups(exitAnimation)` removes every active popup and restores the base destination.
- `SaveState()`, `RestoreState(...)`, `SaveStateHandle()`, and `ReleaseStateHandle(...)` are for temporary UI state capture and app shutdown restoration.

## Arguments And Screen Callbacks

- Create arguments with `AnchorNavArgument.String(name, value)` or `AnchorNavArgument.From(name, value)`.
- `AnchorNavHost` calls `IAnchorNavigationScreen.OnEnter(args)` and `OnExit(args)` on elements whose `dataSource` implements the interface.
- If arguments are supplied and no bound screen handles them, Anchor logs a warning. Fix the data source, not the warning text.

## Burst Entry Points

- `AnchorNavHost.Burst` exposes `Navigate`, `Toggle`, `CurrentDestination`, `ClearBackStack`, `ClearNavigation`, `PopBackStack`, `PopBackStackToPanel`, `CloseAllPopups`, `ClosePopup`, `HasActivePopups`, `CanGoBack`, `SaveStateHandle`, and `ReleaseStateHandle`.
- Burst route names use `FixedString32Bytes`; keep string conversion at the managed boundary.
- Burst entry points are no-op/false/default when no current app/nav host exists, which is intentional for static initialization and tests.

## Guardrails

- Do not call `AnchorApp.Current.NavHost` directly from Burst jobs.
- Do not reimplement popup toggling in a view model; prefer `Toggle`.
- Do not move `PickingMode.Ignore` expectations from `AnchorNavHost` down into `UXMLService`.
- Do not add separate ECS state just to mirror the active navigation destination unless another system genuinely consumes it.
