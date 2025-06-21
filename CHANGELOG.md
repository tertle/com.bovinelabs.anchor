# ChangeLog

## [1.2.2] - 2025-06-21

### Added
* Support for APP_UI_EDITOR_ONLY

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