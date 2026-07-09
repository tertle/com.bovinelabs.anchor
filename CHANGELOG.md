# Changelog

## [2.0.4] - Unreleased

### Changed
* Updated dependencies
* Reworked the package README and added task-focused guides for setup, services, navigation, MVVM, adapter elements, the debug toolbar, and troubleshooting
* Corrected stale navigation and app API documentation

### Fixed
* Guarded `GroupedMenuBuilder` behind `UNITY_APPUI` so the core runtime continues to compile without App UI installed

## [2.0.3] - 2026-07-07

### Added
* `ICommandAttribute.CanExecuteProperty` for generating command can-execute delegates from bindable bool properties
* MVVM dependency notifications for generated observable properties and relay command can-execute updates
* Anchor UI audio feedback for `AnchorButton` and `AnchorActionButton` hover and activation cues
* `GroupedMenuBuilder` and `GroupedMenuBuilderOptions` for building sorted, grouped menu trees

### Changed
* Anchor navigation assets now use Core `BovineLabs.Core.Asset` and no longer require Nerve
* MVVM dependency declarations now support backing field names
* Debug toolbar view models now use the simplified notification flow
* Pruned low-value tests
* Anchor audio playback now uses `IAudioService` with opt-in named profiles and per-element cue overrides
* Packaged Anchor skill definitions were synced with current agent metadata

### Fixed
* Unity 6.5+ PanelRenderer setup no longer emits the obsolete API warning
* Anchor panel reload tests now match the current Unity 6.5+ PanelRenderer reload callback signature
* Reflection-discovered Anchor app, debug toolbar, service, and view-model types are now preserved for managed stripping
* Anchor UI audio host objects are hidden in the editor and kept across scene loads during play mode
* Burst object notification delegates now declare the unmanaged calling convention required by Unity 6.6

### Removed
* Obsolete `IView` app interface

## [2.0.2] - 2026-05-09

### Added
* `AnchorNavHost.Toggle` and `AnchorNavHost.Burst.Toggle` for action-resolved popup toggles
* Packaged Anchor workflow skills for app hosting, navigation, MVVM binding, adapter elements, and debug toolbar usage

### Changed
* AnchorActionButton updated for 2.2.0-pre.8
* Core dependency updated to 1.6.1
* AnchorNamedAction renamed to AnchorAction
* Popup toggles now dismiss the matched popup and any active popup descendants above it
* AnchorAppBuilder now uses PanelRenderer on Unity 6.5+ while keeping UIDocument support on Unity 6.3 and 6.4

### Fixed
* Burst navigation trampolines now initialize through editor and runtime load hooks instead of the static constructor

## [2.0.1] - 2026-03-28

### Added
* AnchorSafeArea with configurable safe-area edges
* AnchorScreenMetrics and ScreenMetricsChanged for runtime safe-area updates
* IAnchorNavHost for navigation host abstraction

### Changed
* Safe-area padding centralized in AnchorSafeAreaUtility
* Toolbar safe-area handling now uses the shared safe-area utility

### Fixed
* PhysicsToolbarSystem compatibility with Quill
* Toolbar panel sizing NaN handling

## [2.0.0] - 2026-03-13

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
* IAnchorPanel and AnchorPanel
* AnchorNavAnimation assets with built-in fade and scale fade animations
* OptionPager
* AnchorTouchSliderFloat and AnchorTouchSliderInt
* Extensive automated test coverage across app, binding, navigation, services, toolbar, and utilities
* Anchor No AppUI.tss

### Changed
* Core dependency 1.6.0
* AnchorApp lifecycle standardized around AnchorApp.Current, Services, RootVisualElement, and ShuttingDown
* AnchorAppBuilder now builds directly from UIDocument and restores navigation state without AppUI builder types
* Navigation stack reworked with ID based animations, popup base arguments, menu support, and improved saved state handling
* Toolbar hosting reworked around IAnchorToolbarHost so core systems stay AppUI independent
* Theme entry points split between AppUI and non AppUI setups
* Package metadata and docs updated for optional AppUI usage

### Fixed
* Navigation stack and animation edge cases
* Toolbar only mode
* Toolbar canvas offset lifecycle
* Panel sizing when not stretched full
* First frame NaN issues
* Tests without AppUI installed
* URP support
* Root picking on the app host

### Removed
* Hard package dependency on com.unity.dt.app-ui
* NavigationView
* AnchorDestinationTemplate
* Legacy DependencyInjection namespace
* ToolbarHostBridge
* JSON and byte storage helpers from ILocalStorageService

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
