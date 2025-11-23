# BovineLabs Anchor
BovineLabs Anchor builds on Unity's AppUI stack to provide a UXML-first workflow, a Burst-safe MVVM layer for DOTS, and tooling such as the AnchorNavHost and the debug ribbon. It enables fully declarative interfaces where every screen, popup, and toolbar tab is authored entirely with UXML and AppUI bindings.

For support and discussions, join the [Discord](https://discord.gg/RTsw6Cxvw3) community.

If you want to support my work or get access to a few private libraries, [Buy Me a Coffee](https://buymeacoffee.com/bovinelabs).

## Table of Contents
- [Installation](#installation)
- [Project Setup](#project-setup)
- [Services and Dependency Injection](#services-and-dependency-injection)
- [Authoring Screens in UXML](#authoring-screens-in-uxml)
- [View Models and AppUI Commands](#view-models-and-appui-commands)
- [ECS-Ready Data Binding](#ecs-ready-data-binding)
- [Working with AnchorNavHost](#working-with-anchornavhost)
- [Navigation-Aware Systems](#navigation-aware-systems)
- [Debug Ribbon Toolbar](#debug-ribbon-toolbar)
- [UI Building Blocks and Utilities](#ui-building-blocks-and-utilities)

## Installation
Install the package from Unity Package Manager using the git URL:

```
https://gitlab.com/tertle/com.bovinelabs.anchor.git
```

Or add it directly to `manifest.json`:

```json
{
  "dependencies": {
    "com.bovinelabs.anchor": "https://gitlab.com/tertle/com.bovinelabs.anchor.git"
  }
}
```

Anchor targets Unity 6.3 and depends on:

- [Unity AppUI 2.2.0-pre.2+](https://docs.unity3d.com/Packages/com.unity.dt.app-ui@latest) for MVVM, bindings, and commands.
- [Unity Entities / Burst](https://docs.unity3d.com/Packages/com.unity.entities@latest/) for DOTS integration.
- [BovineLabs Core](https://gitlab.com/tertle/com.bovinelabs.core) for settings, configvars, and helper utilities.

## Project Setup

### 1. Wire up the panel
1. Add a `UIDocument` to your scene.
2. Point it at a `PanelSettings` asset that uses the **Anchor UI** theme found at `/Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss`. You can extend this theme with your own styles. Leave the `UIDocument` source asset empty—the `AnchorAppBuilder` injects the navigation host, popup containers, and toolbars at runtime.

### 2. Provide an `AnchorAppBuilder`
Attach `AnchorAppBuilder` (or a derived type) to the same GameObject as the `UIDocument`. Override it whenever you need to register project-specific services or eagerly warm up state:

```csharp
public class GameAppBuilder : AnchorAppBuilder
{
    protected override void OnConfiguringApp(AppBuilder builder)
    {
        base.OnConfiguringApp(builder);
        builder.services.AddSingleton<IGameStore, GameStore>();
        builder.services.AddSingleton<IInputService, InputService>();
    }

    protected override void OnAppInitialized(AnchorApp anchor)
    {
        base.OnAppInitialized(anchor);
        anchor.services.GetRequiredService<OptionsViewModel>();
    }
}
```

`AnchorAppBuilder` saves the `AnchorNavHost` state when the UI tears down so UI Toolkit Live Reload can rehydrate your navigation stack after a UXML hot reload. Anchor fully supports Live Reload, letting you iterate on UXML/USS while the same app instance keeps running.

### 3. Configure `AnchorSettings`
Open **BovineLabs → Settings** and select the Anchor group. The Core settings framework automatically creates and manages the backing assets (see [Core Settings](https://gitlab.com/tertle/com.bovinelabs.core/-/blob/master/Documentation~/Settings.md) for details). Important options you will tweak in the settings window:

- **Toolbar Only** - skip runtime initialization outside the editor/dev environments.
- **Debug Style Sheets** - additional `.tss` files injected when `BL_DEBUG` or the editor is active.
- **Start Destination** - the view key to load automatically.
- **Views** - a list of `Key` -> `VisualTreeAsset`. Keys must match the strings you pass to `AnchorNavHost`.
- **Actions** - references to `AnchorNamedAction` ScriptableObjects stored in your UI folder. These define action names, destinations, and default navigation options.

### AnchorNamedAction assets
Navigation in Anchor is centrally configured through `AnchorNamedAction` assets. They matter for more than just keeping string literals organized:

- **Single source of truth** – every button, toolbar command, or Burst system calls `AnchorApp.current.Navigate("action_name")`. The nav host resolves the action, merges its default arguments, and applies the stored `AnchorNavOptions`, so all call sites behave consistently.
- **Designer-friendly editing** – you can tweak stack strategy, popup behaviour, or animations directly in the action asset without touching code. Changes take effect the next time the action runs.
- **Argument defaults** – assets can include an initial `Argument[]` (e.g., `Argument.Int("saveSlot", 0)`). When code passes additional arguments, the nav host merges them, letting you override only the values that matter.
- **Popups and transitions** – popup routing (`EnsureBaseAndPopup`, `PopupOnCurrent`), base destinations, and animation sets all live on the asset so the behaviour matches both in-game commands and toolbar shortcuts.

When you call `Navigate("action_name", args...)`, AnchorNavHost looks up the matching asset, merges any arguments, and clones the configured options before performing the navigation. A typical popup asset might look like:

```yaml
# PopUpInventory.asset
actionName: pop_up_inventory
action:
  destination: inventory
  options:
    popupStrategy: EnsureBaseAndPopup
    popupBaseDestination: game
    popupExistingStrategy: CloseOtherPopups
```

You can also define actions in code for rapid iteration:

```csharp
[AnchorNavAction(Actions.GoToGame)]
public static AnchorNavAction BuildGameAction()
{
    return new AnchorNavAction(
        destination: "game",
        options: new AnchorNavOptions
        {
            StackStrategy = AnchorStackStrategy.PopToRoot,
            Animations = new AnchorAnimations
            {
                EnterAnim = NavigationAnimation.SlideLeft,
                ExitAnim = NavigationAnimation.SlideRight,
            },
        });
}
```

Key `AnchorNavOptions` fields:

- `StackStrategy` – prune back-stack entries before pushing the destination (`None`, `PopToRoot`, `PopAll`, `PopToSpecificDestination` + `PopupToDestination`).
- `PopupStrategy` – treat the navigation as a popup overlay (`None`, `PopupOnCurrent`, `EnsureBaseAndPopup`) with optional base destination and existing popup strategy.
- `Animations` – per-destination enter/exit/pop animation set.
- `PopupBaseDestination`/`PopupBaseArguments` – ensure a particular base screen is active before showing the popup.
- Default `Argument[]` – initial arguments merged with the runtime payload so you can override values selectively.

The code sample above uses `[AnchorNavAction]` to register an action without creating an asset—handy for prototypes or testing utilities.

## Services and Dependency Injection
Anchor relies on a lightweight service container:

- `AnchorAppBuilder` registers `ILocalStorageService`, `IViewModelService`, `IUXMLService`, toolbar services, and every type marked with `[IsService]`.
- Add `[Transient]` when you need a fresh instance per resolution; otherwise services behave as singletons.
- Resolve services via `AnchorApp.current.services.GetRequiredService<T>()`.
- You can override proved services such as `AnchorAppBuilder.UXMLService` to supply a custom loader (Addressables, bundles, etc.).

When the default `UXMLService` instantiates a `VisualTreeAsset`, it walks the visual tree and automatically assigns `dataSource` for every element that declares a `data-source-type`. That is what bridges UXML to your view models.

## Authoring Screens in UXML
Anchor encourages building your runtime UI entirely in UXML. Each `VisualTreeAsset` corresponds to a key in `AnchorSettings.Views`, and `AnchorNavHost` clones the asset when a destination becomes active.

Example view snippet:

```xml
<ui:VisualElement data-source-type="MyGame.UI.Menu.HomeViewModel, MyGame">
    <Unity.AppUI.UI.Heading>
        <Bindings>
            <ui:DataBinding property="text"
                            binding-mode="ToTarget"
                            data-source-path="Title"/>
        </Bindings>
    </Unity.AppUI.UI.Heading>
    <Unity.AppUI.UI.Label class="subtitle">
        <Bindings>
            <ui:DataBinding property="text"
                            binding-mode="ToTarget"
                            data-source-path="Subtitle"/>
        </Bindings>
    </Unity.AppUI.UI.Label>
    <BovineLabs.Anchor.Elements.AnchorActionButton label="@UI:play">
        <Bindings>
            <ui:DataBinding property="clickable.command"
                            binding-mode="ToTarget"
                            data-source-path="PlayCommand"/>
        </Bindings>
    </BovineLabs.Anchor.Elements.AnchorActionButton>
    <BovineLabs.Anchor.Elements.AnchorActionButton label="@UI:quit">
        <Bindings>
            <ui:DataBinding property="commandWithEventInfo"
                            binding-mode="ToTarget"
                            data-source-path="QuitCommand"/>
        </Bindings>
    </BovineLabs.Anchor.Elements.AnchorActionButton>
</ui:VisualElement>
```

Key points:

- Set `data-source-type` on any element that needs a view model. `UXMLService` resolves the type through the service container and assigns it to `dataSource`.
- Use standard AppUI bindings (`ui:DataBinding`) to synchronize properties such as `text`, `enabledSelf`, `value`, etc.
- Anchor ships extended controls (`AnchorActionButton`, `AnchorButton`, `AnchorGridView`, ...) that expose extra bindable properties like `commandWithEventInfo`.
- Complex screens mix multiple `data-source-type` values to pull in nested view models, `ListView` item templates, and AppUI navigation controls such as `SwipeView`.

## View Models and AppUI Commands
Screens bind to regular AppUI `ObservableObject` classes. Decorate them with `[IsService]` so Anchor registers them automatically.

```csharp
[IsService]
public partial class HomeViewModel : ObservableObject
{
    private const string QuitText = "@UI:quit";

    [CreateProperty(ReadOnly = true)]
    public string Title => "@UI:welcomeTitle";

    [CreateProperty(ReadOnly = true)]
    public string Subtitle => "@UI:welcomeSubtitle";

    [ICommand]
    private static void Quit(EventBase evt)
    {
        if (evt.target is ExVisualElement button)
        {
            var dialog = new AlertDialog { title = QuitText };
            dialog.SetPrimaryAction(99, QuitText, Application.Quit);
            Modal.Build(button, dialog).Show();
        }
    }

    [ICommand]
    private void Play()
    {
        AnchorApp.current.Navigate(Actions.HomeToPlay);
    }

    [ICommand]
    private void Options()
    {
        AnchorApp.current.Navigate(Actions.HomeToOptions);
    }
}
```

The AppUI source generators emit `QuitCommand`, `PlayCommand`, etc., so you only have to bind `clickable.command` in UXML. Use `AnchorActionButton.commandWithEventInfo` when you need access to the original `EventBase`.

Because the view models are resolved through the container you can inject services (`ILocalStorageService`, `IStoreService`, `IControlService`, ...) directly through constructors. This makes it easy to persist options, drive localization changes, or talk to custom systems as soon as bound UI state changes.

## ECS-Ready Data Binding
`SystemObservableObject<T>` exposes an unmanaged `Data` struct that Burst systems can mutate. Combine it with `[SystemProperty]` so the source generator emits change notifications.

```csharp
[IsService]
public partial class PlayViewModel : SystemObservableObject<PlayViewModel.Data>
{
    [CreateProperty(ReadOnly = true)]
    public bool OnlineAvailable => Application.internetReachability != NetworkReachability.NotReachable;

    [ICommand] private void Private() => Start(ref this.Value.Private);
    [ICommand] private void Host()    => Start(ref this.Value.Host);
    [ICommand] private void Join()    => Start(ref this.Value.Join);

    private static void Start(ref ButtonEvent evt)
    {
        AnchorApp.current.Navigate(Actions.GoToLoading);
        evt.TryProduce();
    }

    public struct Data
    {
        [SystemProperty] public ButtonEvent Private;
        [SystemProperty] public ButtonEvent Host;
        [SystemProperty] public ButtonEvent Join;
    }
}
```

Supported field types inside the `Data` struct include:

- Plain unmanaged values (int, float, structs).
- `Changed<T>` for one-shot events (`TryConsume()`).
- `ChangedList<T>` and `NativeList<T>` for collections.
- Custom unmanaged structs such as `ButtonEvent`.

Burst systems interact with these view models through `UIHelper<TViewModel, TData>`:

```csharp
public partial struct ServiceHomeStateSystem : ISystem, ISystemStartStop
{
    private UIHelper<PlayViewModel, PlayViewModel.Data> helper;

    public void OnStartRunning(ref SystemState state)
    {
        App.current.services.GetRequiredService<SplashViewModel>().IsInitialized = true;
        this.helper.Bind();
    }

    public void OnStopRunning(ref SystemState state) => this.helper.Unbind();

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        ref var binding = ref this.helper.Binding;
        if (binding.Private.TryConsume())
        {
            // Trigger your own state transition or gameplay logic here.
        }
    }
}
```

`UIHelper` pins the unmanaged data and automatically loads/unloads the view model, so Burst-compiled `ISystem` code can interact with UI state directly.

If a view model needs to react when a screen is shown or hidden, implement `IAnchorNavigationScreen`:

```csharp
[IsService]
public partial class UserReportingViewModel : ObservableObject, IAnchorNavigationScreen
{
    [ICommand] private void Submit() { /* send report */ }

    void IAnchorNavigationScreen.OnEnter(Argument[] args)
    {
        this.Reset();
        UserReportingService.Instance.CreateNewUserReport();
    }

    void IAnchorNavigationScreen.OnExit(Argument[] args)
    {
        UserReportingService.Instance.ClearOngoingReport();
    }
}
```

`AnchorNavHost` searches the instantiated visual tree for data sources that implement the interface and forwards `Argument[]` from the navigation request.

## Working with AnchorNavHost
`AnchorNavHost` replaces AppUI's built-in NavHost. It is a `VisualElement` that:

- Instantiates screens from the `AnchorSettings.Views` map using `IUXMLService`.
- Tracks the active destination stack and a separate back stack snapshot for `PopBackStack()`.
- Supports popups layered on top of the base screen.
- Exposes `EnteredDestination`, `ExitedDestination`, `ActionTriggered`, and `DestinationChanged` events.
- Saves and restores its entire state so the UI survives assembly reloads.

Navigate by calling the static helper or by targeting the host directly:

```csharp
AnchorApp.current.Navigate(Actions.HomeToPlay);
AnchorApp.current.NavHost.Navigate(Actions.GoToLoading, Argument.String("saveId", saveGuid));
```

### Burst navigation entry points
Use the `AnchorNavHost.Burst` wrappers from Burst-compiled systems. They forward into the live `AnchorApp.current.NavHost` via shared statics, so they remain Burst-safe while still driving the managed UI:

- `Navigate(FixedString32Bytes screen)` mirrors `Navigate(string, Argument[])`.
- `CurrentDestination()` returns the active destination as `FixedString32Bytes`.
- `ClearBackStack()`, `PopBackStack()`, `PopBackStackToPanel()` match the instance stack helpers.
- `CloseAllPopups(NavigationAnimation exitAnimation)`, `ClosePopup(FixedString32Bytes destination, NavigationAnimation exitAnimation)` close overlays from Burst.
- `HasActivePopups()` and `CanGoBack()` expose the state flags used by UI/input systems.

Example:

```csharp
public partial struct ClientGameStateSystem : ISystem, ISystemStartStop
{
    [BurstCompile]
    public void OnStartRunning(ref SystemState state)
    {
        AnchorNavHost.Burst.Navigate(Actions.GoToGame);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (input.WantsBack && AnchorNavHost.Burst.CanGoBack())
        {
            AnchorNavHost.Burst.PopBackStackToPanel();
        }
    }
}
```

Navigation behaviour (stack manipulation, popups, animations, defaults) is defined entirely by the `AnchorNamedAction` assets described earlier.

At runtime you can:

- `AnchorNavHost.PopBackStack()` - return to the previous snapshot.
- `PopBackStackToPanel()` - pop and close any overlays captured with that snapshot.
- `CloseAllPopups()` - dismiss only the stacked popups.

## Navigation-Aware Systems
`NavigationStateSystem` keeps DOTS systems in sync with the currently visible destination. To opt in:

1. Create lightweight `IComponentData` tags (e.g., `UIGame`, `UIOptions`) in a UI/ViewStates folder within your project.
2. Open the `UISystemTypes` settings asset (available via **BovineLabs → Settings → Anchor → UISystemTypes**) and map destination names to those components.
3. In your systems, call `StateAPI.Register` (if you use the BovineLabs state machine) or pass the destination name to `UIHelper`.

When `AnchorNavHost` activates `home`, `NavigationStateSystem` adds the mapped component to the systems that registered for it. You can call `state.Enabled`/`RequireForUpdate` based on that component or rely on the string-based `UIHelper` constructor, which already resolves the component for you.

## Debug Ribbon Toolbar
In the editor or when `BL_DEBUG` is defined, `AnchorApp` injects the ribbon toolbar at the top of the panel. It provides tabs for Memory, FPS, Entities, Physics, Localization, Quality, Time, UI themes, and any tabs you add.

Two ways to add a tab:

1. **`[AutoToolbar]` for UI-only panels**

```csharp
[AutoToolbar("Save")]
public class SaveToolbarView : View<SaveToolbarViewModel>
{
    public SaveToolbarView() : base(new SaveToolbarViewModel())
    {
        var label = new Text();
        label.dataSource = this.ViewModel;
        label.SetBindingToUI(nameof(Text.text), nameof(SaveToolbarViewModel.Status));
        this.Add(label);
    }
}
```

2. **`ToolbarHelper` for ECS-driven tabs**

```csharp
public partial struct EntitiesToolbarSystem : ISystem, ISystemStartStop
{
    private ToolbarHelper<EntitiesToolbarView, EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data> toolbar;

    public void OnCreate(ref SystemState state)
    {
        this.toolbar = new ToolbarHelper<EntitiesToolbarView, EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data>(ref state, "Entities");
    }

    public void OnStartRunning(ref SystemState state) => this.toolbar.Load();
    public void OnStopRunning(ref SystemState state)  => this.toolbar.Unload();

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (!this.toolbar.IsVisible())
        {
            return;
        }

        ref var binding = ref this.toolbar.Binding;
        binding.EntityCount = state.EntityManager.UniversalQuery.CalculateEntityCountWithoutFiltering();
    }
}
```

`ToolbarHelper` mirrors `UIHelper`: it pins the unmanaged data, restores serialized view-model state, and only updates when the tab is visible, so you can drive toolbar data from Bursted systems just like any other DOTS system.

## UI Building Blocks and Utilities
Anchor ships several helper types used throughout larger AppUI projects:

- **Elements** - `AnchorActionButton`, `AnchorButton`, `AnchorAccordion`, `AnchorGridView`, `KeyValueElement`, and `KeyValueGroup` provide extra bindable properties and convenience behaviour (dynamic item templates, command bindings, etc.).
- **Collections** - `AnchorObservableCollection<T>` adds `AddRange`/`Replace` helpers and properly batches `INotifyCollectionChanged` events, which is ideal for inventory lists, quest logs, or other frequently refreshed data sets.
- **Converters** - common AppUI binding converters (DisplayStyle <-> bool, inverted bool, etc.) live under `BovineLabs.Anchor.Binding`.
- **Settings hooks** - `AnchorSettings.DebugStyleSheets` inject extra TSS files when debugging, and `ToolbarOnly` lets you ship the package without constructing the runtime UI in release builds.
- **Custom UXML loading** - override `AnchorAppBuilder.UXMLService` if you need to instantiate assets from a different source; the rest of the stack (AnchorNavHost, bindings, commands) stays untouched.

With these pieces you can author complex AppUI experiences end-to-end in UXML with Burst-aware view models, robust navigation, and a cohesive debug workflow.
