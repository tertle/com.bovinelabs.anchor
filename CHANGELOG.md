# Changelog

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
