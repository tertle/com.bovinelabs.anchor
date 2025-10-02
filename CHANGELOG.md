# ChangeLog

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