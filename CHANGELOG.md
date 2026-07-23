# Changelog

## [2.0.0] - Unreleased

### Breaking
* Unity 6.7+ only
* BovineLabs Core 2.0.0 or newer is required
* Unity App UI 2.2.0 or newer is required
* Universal Render Pipeline 17.7.0 or newer is required
* `APP_UI_EDITOR_ONLY` is no longer supported
* `AnchorSettings.ToolbarOnly` and toolbar-only app initialization have been removed
* Anchor app setup now separates durable app initialization from replaceable visual generations through
  `OnVisualGenerationInitialized` and `OnVisualGenerationShuttingDown`; visual elements can no longer be registered as services
* Debug toolbar panels now register durable `IToolbarElement` models that create fresh visual elements instead of registering `View<T>` instances

### Added
* Rendering debug toolbar with triangles, vertices, draw calls, SetPass calls, and instances

### Changed
* Anchor app services, navigation state, toolbar registrations, and persisted toolbar state now survive panel reloads while the visual tree is recreated
* Memory and rendering toolbar values now use compact GB and K/M/B formatting
* Rendering toolbar draw-call and instance totals now aggregate Unity 6.7's standard, SRP Batcher, BRG, and null-geometry counters

## [1.5.0] - 2026-07-13

### Breaking
* Unity 6.3+ only
* AppUI is now optional instead of a required package dependency
* BovineLabs.Anchor runtime no longer depends on AppUI app, MVVM, or navigation types
* AppUI controls moved into BovineLabs.Anchor.Adapters
* AnchorAppBuilder no longer derives from the AppUI builder stack
* Navigation APIs now use AnchorNavArgument
* IInitializable renamed to ILoadable
* ILocalStorageService now only supports primitive preference values

### Added
* BovineLabs.Anchor.Adapters assembly for optional AppUI integrations
* Anchor-owned dependency injection with AnchorServiceCollection and AnchorServiceProvider
* Anchor-owned MVVM runtime and generators
* `ICommandAttribute.CanExecuteProperty` and MVVM dependency notifications for generated observable properties and relay command can-execute updates, including backing field declarations
* IAnchorPanel and AnchorPanel
* IAnchorNavHost for navigation host abstraction
* AnchorNavAnimation assets with built-in fade and scale fade animations
* `AnchorNavHost.Toggle` and `AnchorNavHost.Burst.Toggle` for action-resolved popup toggles
* OptionPager
* AnchorTouchSliderFloat and AnchorTouchSliderInt
* AnchorSafeArea with configurable safe-area edges
* AnchorScreenMetrics and ScreenMetricsChanged for runtime safe-area updates
* `AnchorLinearProgress` for horizontal or vertical App UI progress with optional fill textures and alpha masking
* Anchor UI audio feedback for `AnchorButton` and `AnchorActionButton` hover and activation cues through `IAudioService`, with opt-in named profiles and per-element cue overrides
* `GroupedMenuBuilder` and `GroupedMenuBuilderOptions` for building sorted, grouped menu trees
* PanelRenderer ViewModel editor toolbar
* Anchor No AppUI.tss
* Automated test coverage across app, binding, navigation, services, toolbar, and utilities
* Packaged Anchor workflow skills for app hosting, navigation, MVVM binding, adapter elements, and debug toolbar usage

### Changed
* Updated dependencies, including Core 1.6.4 as a mandatory dependency
* AnchorApp lifecycle standardized around AnchorApp.Current, Services, RootVisualElement, and ShuttingDown
* AnchorAppBuilder now restores navigation state without AppUI builder types and uses PanelRenderer on Unity 6.5+ or UIDocument on Unity 6.3 and 6.4
* Navigation stack reworked with ID based animations, popup base arguments, menu support, and improved saved state handling
* Anchor navigation assets now use Core `BovineLabs.Core.Asset` and no longer require Nerve
* AnchorNamedAction renamed to AnchorAction
* Popup toggles now dismiss the matched popup and any active popup descendants above it
* Toolbar hosting reworked around IAnchorToolbarHost so core systems stay AppUI independent
* Safe-area padding centralized in AnchorSafeAreaUtility, including toolbar safe-area handling
* Debug toolbar view models now use the simplified notification flow
* Theme entry points split between AppUI and non AppUI setups
* AnchorActionButton updated for App UI 2.2.0-pre.8
* Package metadata, workflow skills, README, setup guides, and API documentation updated for the current optional AppUI workflows

### Fixed
* Navigation stack and animation edge cases
* Burst navigation trampolines now initialize through editor and runtime load hooks instead of the static constructor
* Toolbar only mode
* Toolbar canvas offset lifecycle
* Panel sizing when not stretched full
* First frame and toolbar panel sizing NaN issues
* PhysicsToolbarSystem compatibility with Quill
* Tests without AppUI installed
* URP support
* Root picking on the app host
* Unity 6.5+ PanelRenderer setup and reload callback compatibility
* Reflection-discovered Anchor app, debug toolbar, service, and view-model types are now preserved for managed stripping
* Anchor UI audio host objects are hidden in the editor and kept across scene loads during play mode
* Burst object notification delegates now declare the unmanaged calling convention required by Unity 6.6
* `GroupedMenuBuilder` is guarded behind `UNITY_APPUI` so the core runtime continues to compile without App UI installed
* `SystemObservableObject` pinning now works with serialized data and CoreCLR

### Removed
* Hard package dependency on com.unity.dt.app-ui
* NavigationView
* AnchorDestinationTemplate
* Legacy DependencyInjection namespace
* ToolbarHostBridge
* JSON and byte storage helpers from ILocalStorageService
* Obsolete `IView` app interface

## [1.3.0] - 2025-10-03

### Breaking
* Now requires BovineLabs Core, no longer optional

### Added
* Support for APP_UI_EDITOR_ONLY
* Full custom AnchorNavHost replacing AppUI NavHost. Automates a lot of ViewModel workflow and adds support for popups, UI Toolkit Live Reloading, asset based transitions
* IAnchorNavigationScreen that works on ViewModel
* IUXMLService workflow for managing VisualTreeAsset
* AnchorAccordion - Accordian with itemTemplate uxml and itemsSource binding
* AnchorActionButton - ActionButton with clickable binding and commandWithEventInfo
* AnchorButton - Button with commandWithEventInfo
* AnchorGridView - GridView with itemTemplate uxml and selectIndex, seletedIndices, itemsSource, columnCount, makeItem, bindItem binding. makeItem and bindItem also have default implementations that auto bind to the itemsSource
* AnchorObservableCollection - ObservableCollection with Replace and AddRange methods to stop per element events
* A few built in converts for DisplayStyle and inverting bools

### Changed
* Updated to AppUI 2.0.0-pre.22

## [1.2.1] - 2025-06-07

### Fixed
* Source generators when using the same field on multiple view models
* URP

### Removed
* Logging package
* Support for non notify binding, doesn't work with observable pattern and should be avoided

### Changed
* Core dependency 1.4.3
* Moved generators into asmdef to limit scope
* UISystemTypes now uses ComponentAsset from Core
