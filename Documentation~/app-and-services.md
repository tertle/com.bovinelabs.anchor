# Application and services

`AnchorAppBuilder` owns the managed application lifetime. It binds a UI Toolkit host, creates the service provider and panel, initializes `AnchorApp`, preserves navigation state across disable and enable, and disposes app-owned services on shutdown.

## Hosting model

Anchor separates the Unity component that owns the panel from the visual element used as the app root:

- Unity 6000.5 or newer uses `PanelRenderer`.
- Unity 6000.3 and 6000.4 use `UIDocument`.
- `IAnchorPanel` abstracts the app root, theme, and scale.
- `AnchorPanel` is the default panel implementation. It uses the App UI panel when App UI is available and a plain `VisualElement` otherwise.
- `AnchorApp` owns the navigation host and exposes the running app through `AnchorApp.Current`.

Place the host component and builder on the same GameObject. The builder will auto-find the host when its serialized reference is empty. Anchor treats that host as exclusive: it clears the host root before attaching or reattaching its app root, and clears it again during shutdown. See [Getting started](getting-started.md) for the scene setup.

## Builder lifecycle

The default lifecycle is:

| Stage | Builder behavior |
| --- | --- |
| `OnEnable` | Binds the host, configures services, builds the provider, creates the app and panel, attaches the app root, then initializes the app. |
| `Update` | Polls screen and safe-area metrics and raises `AnchorApp.ScreenMetricsChanged` when they change. |
| `OnDisable` | Calls the shutdown hook, saves navigation state when not toolbar-only, clears the host, disposes the app, then disposes the service provider. |
| Host reload | On Unity 6000.5 or newer, reattaches the existing app root when `PanelRenderer` reloads its visual tree. |

The protected hooks have distinct responsibilities:

- `OnConfigureServices` runs before the provider exists. Call `base` first, then add or replace project registrations.
- `OnAppInitialized` runs after `AnchorApp.Current`, `Services`, `Panel`, and `RootVisualElement` are assigned. Call `base` first so navigation and toolbar initialization finish before project startup work.
- `OnAppShuttingDown` runs while the app and services are still available. Preserve the base call so the default builder can save navigation state.

The default `OnAppInitialized` creates the navigation host, navigates to `AnchorSettings.StartDestination`, initializes the toolbar, and then restores any state saved by an earlier disable. In editor or `BL_DEBUG` builds it also adds the configured debug style sheets.

See [Navigation](navigation.md) for destination setup, state semantics, actions, popups, and the Burst entry points.

When `AnchorSettings.ToolbarOnly` is enabled, normal app and navigation initialization is skipped and only the toolbar is initialized.

## Configure a project app

The following builder adds an application service with constructor injection and eagerly resolves it after normal app initialization. In a project, place each top-level type in its own same-named file; they are shown together here to keep the example readable.

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;

    public interface IPlayerProfileService
    {
        string DisplayName { get; }
    }

    public sealed class PlayerProfileService : IPlayerProfileService
    {
        public PlayerProfileService(ILocalStorageService storage)
        {
            this.DisplayName = storage.GetValue("profile.display-name", "Player");
        }

        public string DisplayName { get; }
    }

    public sealed class GameAppBuilder : AnchorAppBuilder
    {
        protected override void OnConfigureServices(AnchorServiceCollection services)
        {
            base.OnConfigureServices(services);
            services.AddSingleton<IPlayerProfileService, PlayerProfileService>();
        }

        protected override void OnAppInitialized(AnchorApp app)
        {
            base.OnAppInitialized(app);
            app.Services.GetRequiredService<IPlayerProfileService>();
        }

        protected override void OnAppShuttingDown(AnchorApp app)
        {
            // Stop project-owned subscriptions while app services are still valid.
            base.OnAppShuttingDown(app);
        }
    }
}
```

Most applications only need a custom builder. To replace the app type itself, derive from `AnchorApp` and host it with `AnchorAppBuilder<TApp>`; `TApp` must have a public parameterless constructor.

## Runtime access

`AnchorApp.Current` is the managed singleton for the running app:

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;

    public static class AnchorAccess
    {
        public static bool HasSeenIntro()
        {
            var storage = AnchorApp.Current.Services.GetRequiredService<ILocalStorageService>();
            return storage.GetValue("intro.seen", false);
        }

        public static void OpenOptions()
        {
            AnchorApp.Current.NavHost.Navigate("options");
        }
    }
}
```

Useful app properties include:

- `Services`: the app's `IServiceProvider`.
- `Panel` and `RootVisualElement`: the app's panel abstraction and visual root.
- `NavHost`: the current navigation host.
- `PopupContainer`, `NotificationContainer`, and `TooltipContainer`: named containers discovered during `AnchorApp.Initialize`, when the panel supplies them.
- `ScreenMetricsChanged` and `AnchorApp.SafeArea`: screen and safe-area information for responsive elements.

`AnchorApp.Current` and its service provider are managed objects. Do not access them directly from Burst-compiled code; use Anchor's Burst-specific interop APIs instead.

## Service registrations

`AnchorServiceCollection` supports singleton, transient, instance, and alias registrations. For example, inside `OnConfigureServices`:

```csharp
services.AddSingleton(typeof(GameClock));
services.AddSingleton<IPlayerProfileService, PlayerProfileService>();

services.AddTransient(typeof(DialogController));
services.AddTransient<IDialogController, DialogController>();

services.AddSingletonInstance(typeof(GameSession), session);

services.AddSingleton(typeof(GameSettings));
services.AddAlias<IGameSettings, GameSettings>();
```

The provider resolves the public constructor with the most parameters whose full dependency graph can be resolved. A constructor can request `IServiceProvider` or `AnchorServiceProvider` directly. Circular dependencies throw an `InvalidOperationException`.

Registrations are searched from last to first. A registration added after `base.OnConfigureServices` therefore replaces a default registration for the same service type.

Resolve an optional service with `GetService<T>()` and a required service with `GetRequiredService<T>()`. Inside a managed method:

```csharp
var optional = AnchorApp.Current.Services.GetService<IOptionalService>();
var required = AnchorApp.Current.Services.GetRequiredService<IPlayerProfileService>();
```

`GetService<T>()` returns `null` when the type is not registered. `GetRequiredService<T>()` throws an `InvalidOperationException` instead.

### Lifetimes and disposal

- Singletons are constructed lazily and cached for the lifetime of the enabled builder.
- Transients create a new instance on every resolution.
- The provider disposes each resolved singleton that implements `IDisposable` once when the builder shuts down.
- Transient instances are not tracked or disposed by the provider.
- Treat objects passed to `AddSingletonInstance` as provider-owned if they implement `IDisposable`; once resolved and cached, the provider will dispose them.
- Aliases forward to the already registered target service. An alias to a singleton returns that singleton; an alias to a transient follows transient creation rules.

Anchor does not provide nested service scopes. If a shorter lifetime needs cleanup, make that ownership explicit in the service or feature that creates it.

## Automatic service discovery

Concrete types marked with `[IsService]` are registered automatically when the builder configures services. They are singletons unless also marked `[Transient]`:

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;

    [IsService]
    public sealed class OptionsViewModel
    {
    }

    [IsService]
    [Transient]
    public sealed class ConfirmationDialogViewModel
    {
    }
}
```

Auto-registered types still need a resolvable public constructor. Non-transient services that implement `IAnchorToolbarHost` are also registered as an `IAnchorToolbarHost` alias so the app can attach the toolbar during initialization.

Use explicit registration when an interface mapping, prebuilt instance, or deliberate replacement is part of the application contract. Use `[IsService]` for concrete UI types that are naturally discovered with the app.

## Built-in services

Calling `base.OnConfigureServices` registers these singleton services. `IUXMLService` is omitted when a custom builder returns `null` from its `UXMLService` property:

| Contract | Default behavior |
| --- | --- |
| `ILocalStorageService` | Stores string, integer, and boolean values with Unity `PlayerPrefs`. |
| `IViewModelService` | Loads registered view models and caches loaded instances by type for `UIHelper<TViewModel, TData>`. |
| `IAudioService` | Resolves hover and activation clips from `AnchorSettings.Audio` and plays them through a lazily created 2D `AudioSource`. |
| `IUXMLService` | When enabled, looks up `VisualTreeAsset` entries in `AnchorSettings.Views`, instantiates them, and assigns services to declared UXML data sources. |

### Local storage

`ILocalStorageService` exposes `HasKey`, `DeleteKey`, and typed `GetValue` and `SetValue` overloads for `string`, `int`, and `bool`. The default implementation follows `PlayerPrefs` storage semantics.

Inject the interface instead of resolving it globally when a service or view model owns persisted settings:

```csharp
using BovineLabs.Anchor.Services;

public sealed class AccessibilityPreferences
{
    private readonly ILocalStorageService storage;

    public AccessibilityPreferences(ILocalStorageService storage)
    {
        this.storage = storage;
    }

    public bool Subtitles
    {
        get => this.storage.GetValue("accessibility.subtitles", true);
        set => this.storage.SetValue("accessibility.subtitles", value);
    }
}
```

### View-model service

`IViewModelService.Load<T>()` resolves `T` from the app provider on first use and remembers it. `Get<T>()` only returns an already loaded entry, and `Unload<T>()` removes that entry from the view-model cache.

Unloading does not dispose the object or remove it from the underlying service provider. Loading a singleton again returns the provider's same singleton; loading a transient after unloading creates a new instance. The service is used by Anchor's ECS-oriented `UIHelper`; see [MVVM and binding](mvvm-and-binding.md).

### UXML service

`IUXMLService.GetAsset(key)` searches `AnchorSettings.Views`. `Instantiate(key)` clones the asset, then walks the cloned tree and resolves a service for every element whose `dataSourceType` was declared in UXML.

Navigation normally owns screen instantiation and attachment. It also applies screen sizing and picking behavior, so application code should navigate to a configured key instead of manually attaching a screen created by `IUXMLService`.

`Instantiate` assumes the key maps to a non-null asset. Keep destination names and view keys in sync; `GetAsset` logs an error and returns `null` for a missing key.

### UI audio

Configure UI audio under **BovineLabs > Settings > Anchor > Audio**. Each profile maps a case-sensitive key to optional hover and activation clips. The built-in profile key is `default`, exposed as `AnchorAudioSettings.DefaultProfileKey`.

When the App UI adapters are installed, `AnchorButton` and `AnchorActionButton` use the default profile unless another profile is assigned. Each cue can:

- `Inherit` the clip from the selected profile.
- `Disabled` suppress playback for that cue.
- `Custom` play a per-element clip instead of the profile clip.

Code can use the same service through the safe `AnchorAudio` facade:

```csharp
using BovineLabs.Anchor.Audio;
using UnityEngine;

public static class MenuAudio
{
    public static void PlayDefaultActivate()
    {
        AnchorAudio.Play(
            AnchorAudioSettings.DefaultProfileKey,
            AnchorAudioCue.Activate,
            AnchorAudioCueOverride.Inherit);
    }

    public static void PlayCustomActivate(AudioClip customClip)
    {
        AnchorAudio.Play(
            string.Empty,
            AnchorAudioCue.Activate,
            AnchorAudioCueOverride.Custom(customClip));
    }
}
```

`AnchorAudio.Play` is a no-op when there is no current app or no registered `IAudioService`. The default service creates its audio GameObject only when it has a non-null clip to play, and disposes that object with the app provider. See [Adapter elements](adapter-elements.md) for the button attributes and properties.

## Replace a built-in service

Override a builder type property when the replacement is fixed for the entire app. Here `JsonLocalStorageService` and `GameAudioService` stand for project implementations of the corresponding interfaces:

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;

    public sealed class GameAppBuilder : AnchorAppBuilder
    {
        protected override System.Type LocalStorageService => typeof(JsonLocalStorageService);

        protected override System.Type AudioService => typeof(GameAudioService);
    }
}
```

The replacement type must implement the corresponding service interface and have a resolvable public constructor. The available properties are `LocalStorageService`, `ViewModelService`, `AudioService`, and `UXMLService`.

Alternatively, call `base.OnConfigureServices` and add another registration for the same contract. Because the last registration wins, this is convenient for a prebuilt instance or a replacement selected by project code.

Set `UXMLService` to `null` only when the app deliberately does not use settings-backed UXML, such as a toolbar-only host or a fully custom app. Default navigation requires an `IUXMLService` to create destination screens.

## Custom app and panel types

Derive from `AnchorApp` when application initialization itself must change. Put these types in separate same-named files:

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;

    public sealed class GameAnchorApp : AnchorApp
    {
        public override void Initialize()
        {
            base.Initialize();
            // Add project-level app behavior after Anchor navigation is ready.
        }
    }
}
```

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;

    public sealed class GameAppBuilder : AnchorAppBuilder<GameAnchorApp>
    {
    }
}
```

Keep the base initialization unless the custom app intentionally replaces Anchor's navigation setup.

For a different visual root, override `PanelType` or `CreatePanel`. A `PanelType` must implement `IAnchorPanel` and have a public parameterless constructor; the builder validates both requirements before creating it.

## Shutdown events

`AnchorApp.ShuttingDown` is raised at the start of app disposal, while the app references are still valid. After event handlers return, the app clears its navigation, panel, service, container, and screen-metric references; the builder then disposes the provider. Prefer the builder's shutdown hook for app-specific teardown that needs service access, and always unsubscribe static event handlers that outlive an app instance.
