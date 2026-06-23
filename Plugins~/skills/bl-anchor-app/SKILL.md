---
name: bl-anchor-app
description: "Use for AnchorAppBuilder hosting, AnchorSettings, PanelRenderer or UIDocument setup, services, lifecycle hooks, or UXML service ownership."
---

# Anchor App Hosting

Use this skill for Anchor app root setup, lifecycle, settings, and services. Resolve Anchor source from the installed package root; in source checkouts this is commonly `Packages/com.bovinelabs.anchor`.

## Workflow

1. Inspect the target `AnchorAppBuilder` subclass, `AnchorSettings`, and the target assembly references first.
2. Keep the host split explicit: Unity 6.5+ uses `PanelRenderer`; Unity 6.3/6.4 uses `UIDocument`.
3. Use `AnchorAppBuilder` directly unless the app needs custom services, a custom `AnchorApp`, or lifecycle hooks.
4. Override `OnConfigureServices` for app services and call `base.OnConfigureServices(services)` unless intentionally replacing Anchor defaults.
5. Configure `AnchorSettings` for `Views`, `StartDestination`, `Actions`, `Animations`, `DebugStyleSheets`, and `ToolbarOnly`.
6. Use `OnAppInitialized` and `OnAppShuttingDown` for app-specific startup/shutdown around the base initialization and saved navigation state.
7. Route navigation through `AnchorApp.Current.NavHost` from managed UI code, or through `AnchorNavHost.Burst` from Burst-compatible paths.
8. Run `BovineLabs.Anchor.Tests` when code changes touch package behavior; documentation-only skill edits only need skill validation.

## Host And Lifecycle

- `AnchorAppBuilder<T>` creates the `AnchorServiceProvider`, creates the panel, initializes `AnchorApp`, attaches the app root to the host element, updates screen metrics, and disposes services on disable.
- On Unity 6.5+, assign or colocate a `PanelRenderer`. The builder registers a reload callback, calls `IPanelComponent.PerformValidation(true)`, and reattaches the app root when the panel reloads.
- On Unity 6.3/6.4, assign or colocate a `UIDocument`; the builder hosts Anchor under `rootVisualElement`.
- `AnchorPanel` is the default `IAnchorPanel`. It is AppUI-backed when `UNITY_APPUI` is defined and a plain `VisualElement` panel otherwise.
- `ToolbarOnly` skips normal navigation initialization and initializes only the toolbar.
- `AnchorApp.Current` is a managed singleton. Do not call it directly from Burst-compiled jobs except behind an established managed trampoline or `[BurstDiscard]` boundary.

## Services

- Default services are `ILocalStorageService`, `IViewModelService`, and `IUXMLService` when `UXMLService` is not null.
- Types marked `[IsService]` are auto-registered. Add `[Transient]` when each resolution should create a fresh instance.
- Non-transient services implementing `IAnchorToolbarHost` are also aliased as `IAnchorToolbarHost`.
- Register app-specific services with `AnchorServiceCollection` using `AddSingleton`, `AddTransient`, `AddSingletonInstance`, or `AddAlias`.
- Resolve services through `AnchorApp.Current.Services.GetRequiredService<T>()` when absence is an invariant.

## UXML Ownership

- `AnchorSettings.Views` maps destination keys to `VisualTreeAsset`s.
- `UXMLService.GetAsset(key)` performs the settings lookup and logs through `BLGlobalLogger` when a key is missing.
- `UXMLService.Instantiate(key)` instantiates the asset and assigns `dataSource` on elements that declare `data-source-type`.
- `AnchorNavHost`, not `UXMLService`, owns screen attachment, `StretchToParentSize()`, and `PickingMode.Ignore` on created navigation items.
- Do not put row-item `data-source-type` declarations on templates whose parent control supplies row data; `AnchorGridView` and `AnchorAccordion` set item `dataSource` themselves.

## Guardrails

- Do not invent a parallel service container or app singleton for Anchor screens.
- Do not collapse the Unity 6.5 `PanelRenderer` path into a `UIDocument`-only implementation.
- Do not treat README samples as more authoritative than the current package source; check `BovineLabs.Anchor/App` and tests first.
- Do not add defensive null/service checks when builder or settings invariants already guarantee the service.
- Do not use `Debug.Log*`; use `BLGlobalLogger` outside ECS systems or the project logging policy inside systems.
