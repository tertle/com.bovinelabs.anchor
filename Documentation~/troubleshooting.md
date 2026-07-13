# Troubleshooting Anchor

Start with the first symptom that matches the project. Anchor logs runtime setup failures through BovineLabs logging rather than `Debug.Log`.

## Anchor types are not visible

Anchor's asmdefs have `autoReferenced` disabled. Add an explicit reference from the consuming asmdef:

- `BovineLabs.Anchor` for app, navigation, services, MVVM, binding, audio, and core elements.
- `BovineLabs.Anchor.Adapters` for AppUI-backed Anchor controls.
- `BovineLabs.Anchor.Debug` for toolbar views and helpers.

Adapter and debug code also needs Unity App UI. `BovineLabs.Anchor.Debug` is constrained to the Editor or builds with `BL_DEBUG`. When `APP_UI_EDITOR_ONLY` is defined, AppUI-backed assemblies are excluded from players.

Anchor asmdefs can list assemblies supplied by optional packages. Their `versionDefines`, such as `UNITY_APPUI` and `UNITY_URP`, enable the matching code only when that package is installed; the reference alone is not a package requirement. If a symbol-gated API is missing, verify that its package is installed and that the expected version define is active.

## The app does not start

Assign or colocate a `PanelRenderer` on the same GameObject as `AnchorAppBuilder`.

The builder aborts startup when it cannot bind the expected host. Also verify that the scene contains only one enabled Anchor builder; initializing a second app disposes and replaces `AnchorApp.Current`.

If a custom builder overrides `OnConfigureServices`, `OnAppInitialized`, or `OnAppShuttingDown`, call the base implementation unless the custom builder intentionally replaces Anchor's defaults, navigation initialization, or state persistence.

## A view key cannot be found

`UXMLService` resolves keys from `AnchorSettings.Views` with an exact, case-sensitive comparison. Confirm that:

1. The destination's view key exists in `AnchorSettings.Views`.
2. The mapped `VisualTreeAsset` is assigned.
3. Actions point to a registered destination.

`GetAsset` logs an error and returns `null` for a missing key. `Instantiate` expects a valid asset, so fix the settings entry rather than continuing after that error.

## A UXML data source is null

For each element with `data-source-type`, `UXMLService` asks the app service provider for that exact type. Verify that the value is assembly-qualified and that the type is registered with `[IsService]` or in `OnConfigureServices`.

Do not add `data-source-type` to row templates when `AnchorGridView` or `AnchorAccordion` owns the row. Those controls assign each row's `dataSource` from `itemsSource`.

## Generated properties or commands are missing

Generated owners must be `partial` and derive from `ObservableObject` or `SystemObservableObject<T>` as required by the feature. Check the attribute and generated naming rules:

- `[ObservableProperty]` on a backing field generates the public property.
- `[ICommand]` on a supported method generates `<MethodName>Command`.
- `[SystemProperty]` belongs on fields in the unmanaged `Data` struct used by `SystemObservableObject<T>`.

Read [MVVM and binding](mvvm-and-binding.md) for supported dependencies, can-execute properties, and the ECS binding lifecycle.

## Navigation returns false or opens the wrong screen

`Navigate` and `Toggle` return a success value. Check it while diagnosing configuration.

- Destination names and action names must match the configured assets exactly.
- An action must resolve to a destination.
- Popup behavior comes from `AnchorNavOptions`; do not assume a normal destination is a popup.
- `Toggle` only dismisses an existing popup that resolves to the same destination.
- Use `IAnchorNavigationScreen` on the resolved view model when enter/exit arguments must be observed.

See [Navigation](navigation.md) for stack and popup strategies.

## An ECS system cannot see its view model data

Bind `UIHelper<TViewModel, TData>` in `OnStartRunning`, unbind it in `OnStopRunning`, and access `Binding` only while bound. Keep the view model's `Data` struct unmanaged. UI updates and command events pass through generated notification data; do not capture managed visual elements in Burst jobs.

## The debug toolbar is missing

Confirm all of the following:

- The project has AppUI and the consuming asmdef references `BovineLabs.Anchor.Debug`.
- A player build defines `BL_DEBUG`; the toolbar assembly is always available in the Editor.
- The toolbar host is registered as a non-transient service implementing `IAnchorToolbarHost`.
- An ECS toolbar system calls `Load` and `Unload` from `ISystemStartStop`.
- A custom `AnchorAppBuilder.OnAppInitialized` calls the base implementation so `InitializeToolbar` runs.

## UI audio is silent

Audio feedback is opt-in. Assign hover or activate clips to a named profile in `AnchorSettings.Audio`, then select that profile or a per-control cue override. An inherited cue with no clip intentionally produces no sound. Custom builders that replace default service registration must register an `IAudioService` implementation.
