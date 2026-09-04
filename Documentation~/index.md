# BovineLabs Anchor

Anchor is a UI Toolkit application framework for ECS projects. It keeps managed UI composition and navigation in one app host while exposing a small unmanaged data surface that ECS systems can read and update safely.

Anchor provides:

- An `AnchorAppBuilder` host for `PanelRenderer`.
- A lightweight service container with singleton, transient, instance, and alias registrations.
- Keyed UXML instantiation with automatic view-model assignment.
- An Anchor-owned navigation stack with destinations, actions, popups, animations, saved state, and Burst trampolines.
- Managed MVVM primitives and source generators.
- `SystemObservableObject<T>` and `UIHelper<TViewModel, TData>` for ECS-to-UI communication.
- AppUI adapter controls and a debug toolbar.
- Shared UI audio, safe-area, collection, and utility helpers.

## Start here

1. Follow [Getting started](getting-started.md) to install the package, add the correct Unity UI host, and configure the first destination.
2. Read [Application and services](app-and-services.md) when adding app-specific services, UXML templates, storage, or UI audio.
3. Use [Navigation](navigation.md) for screen flow and [MVVM and binding](mvvm-and-binding.md) for view-model state.
4. Add [adapter elements](adapter-elements.md) when the project needs Anchor's App UI-backed controls.
5. Add the [debug toolbar](debug-toolbar.md) to development builds that need live ECS and app controls.

## Requirements and assemblies

Anchor 2.0.0-pre.2 declares Unity 6000.7, `com.bovinelabs.core` 2.0.0-pre.2, Unity App UI 3.0.0-pre.1, and Universal Render Pipeline 17.7.0 in `package.json`.

All Anchor assemblies have `autoReferenced` disabled. Reference only the surfaces the consuming assembly uses:

| Assembly | Add it when |
|---|---|
| `BovineLabs.Anchor` | The code hosts an app, defines a view model, navigates, binds ECS data, or uses core services/elements. |
| `BovineLabs.Anchor.Adapters` | UXML or C# uses Anchor's AppUI-backed controls. |
| `BovineLabs.Anchor.Debug` | A development assembly adds or controls toolbar panels when `UNITY_INCLUDE_INSTRUMENTATION` is defined. |
| `BovineLabs.Anchor.Editor` | Editor-only code extends Anchor authoring or inspectors. |

The app, navigation, DI, and MVVM APIs are Anchor-owned rather than AppUI navigation or MVVM wrappers. App UI and URP are direct package requirements. Assembly references such as `Unity.AppUI` and `Unity.RenderPipelines.Universal.Runtime` provide compile-time access but remain separate from the package dependencies.

## Runtime shape

`AnchorAppBuilder` owns the application lifetime:

1. `OnEnable` registers the scene's `PanelRenderer` reload callback and asks the renderer to validate and supply a visual generation.
2. The first renderer callback registers Anchor defaults and app-specific services, creates the provider, and initializes `AnchorApp.Current` once.
3. It creates `AnchorPanel`, attaches it to the Unity host, then creates `AnchorNavHost`, opens the configured start destination, and attaches an
   available toolbar host.
4. `OnDisable` captures navigation state and releases only the current visual generation. The durable app, provider, and services remain alive for
   the next enable.
5. `OnDestroy` shuts down the final visual generation and app, then disposes the provider and its services.

Managed UI code resolves services from `AnchorApp.Current.Services` and navigates through `AnchorApp.Current.NavHost`. Burst-compatible code uses the explicit `AnchorNavHost.Burst` entry points. ECS systems exchange data with view models through `UIHelper` rather than reading managed UI objects in jobs.

## Themes

Anchor's default theme entry point is:

- `/Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss` imports AppUI and Anchor toolbar styles.

Assign it through the panel settings used by the scene host, or provide an equivalent custom App UI theme that includes Anchor's styles.

## Guides

- [Getting started](getting-started.md)
- [Application and services](app-and-services.md)
- [Navigation](navigation.md)
- [MVVM and binding](mvvm-and-binding.md)
- [Adapter elements](adapter-elements.md)
- [Debug toolbar](debug-toolbar.md)
- [Troubleshooting](troubleshooting.md)
