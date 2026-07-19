---
name: bl-anchor-app
description: "Use for AnchorAppBuilder hosting, AnchorSettings, PanelRenderer setup, services, lifecycle hooks, or UXML service ownership."
---

# Anchor App Hosting

Use this skill for Anchor app root setup, lifecycle, settings, and services. Resolve Anchor source from the installed package root; in source checkouts this is commonly `Packages/com.bovinelabs.anchor`.

## Workflow

1. Inspect the target `AnchorAppBuilder` subclass, `AnchorSettings`, and the target assembly references first.
2. Use `PanelRenderer` as the app host component.
3. Use `AnchorAppBuilder` directly unless the app needs custom services, a custom `AnchorApp`, or lifecycle hooks.
4. Override `OnConfigureServices` for app services and call `base.OnConfigureServices(services)` unless intentionally replacing Anchor defaults.
5. Configure `AnchorSettings` for `Views`, `StartDestination`, `Actions`, `Animations`, `Audio`, `DebugStyleSheets`, and `ToolbarOnly`.
6. Use `OnAppInitialized` and `OnAppShuttingDown` for one-time app startup/shutdown. Pair `OnVisualGenerationInitialized` with `OnVisualGenerationShuttingDown` for each replaceable visual generation.
7. Route navigation through `AnchorApp.Current.NavHost` from managed UI code, or through `AnchorNavHost.Burst` from Burst-compatible paths.
8. Run `BovineLabs.Anchor.Tests` when code changes touch package behavior; documentation-only skill edits only need skill validation.

## Host And Lifecycle

- `AnchorAppBuilder<T>` retains one `AnchorApp` and `AnchorServiceProvider` for its lifetime while replacing the panel, root, navigation host, and toolbar visuals after renderer reload.
- Assign or colocate a `PanelRenderer`. The builder registers a reload callback, calls `IPanelComponent.PerformValidation(true)`, and builds a fresh visual generation for each new renderer version.
- `OnVisualGenerationShuttingDown` runs before reload release, failed-generation cleanup, and final disposal. Unsubscribe from the current `NavHost` and release generation-owned resources there.
- `AnchorPanel` is the default `IAnchorPanel`. It is AppUI-backed when `UNITY_APPUI` is defined and a plain `VisualElement` panel otherwise.
- `ToolbarOnly` skips normal navigation initialization and initializes only the toolbar.
- `AnchorApp.Current` is a managed singleton. Do not call it directly from Burst-compiled jobs except behind an established managed trampoline or `[BurstDiscard]` boundary.

## Services

- Default services are `ILocalStorageService`, `IViewModelService`, and `IAudioService`; `IUXMLService` is also registered when `UXMLService` is not null.
- Types marked `[IsService]` are auto-registered. Add `[Transient]` when each resolution should create a fresh instance.
- `VisualElement` types and instances cannot be services. Register durable view models or factories and create fresh visuals for each generation.
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
- Do not treat README samples as more authoritative than the current package source; check `BovineLabs.Anchor/App` and tests first.
- Do not add defensive null/service checks when builder or settings invariants already guarantee the service.
- Do not use `Debug.Log*`; use `BLGlobalLogger` outside ECS systems or the project logging policy inside systems.
