# ChangeLog

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