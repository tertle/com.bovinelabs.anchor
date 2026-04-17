# BovineLabs Anchor

BovineLabs Anchor is a Unity UI framework for ECS projects.

It provides:

- A core runtime (`BovineLabs.Anchor`) with app lifecycle, DI, navigation, and MVVM.
- Optional AppUI adapters (`BovineLabs.Anchor.Adapters`) for AppUI-backed controls and panel integration.
- Optional debug toolbar (`BovineLabs.Anchor.Debug`) for editor/debug workflows.

## Table of Contents

- [Installation](#installation)
- [Assembly Layout and Compile Gates](#assembly-layout-and-compile-gates)
- [Project Setup](#project-setup)
- [Services and Dependency Injection](#services-and-dependency-injection)
- [Authoring Screens in UXML](#authoring-screens-in-uxml)
- [View Models and Commands](#view-models-and-commands)
- [ECS-Ready Data Binding](#ecs-ready-data-binding)
- [Working with AnchorNavHost](#working-with-anchornavhost)
- [Navigation-Aware View Models](#navigation-aware-view-models)
- [Debug Toolbar and ToolbarHelper](#debug-toolbar-and-toolbarhelper)
- [UI Building Blocks](#ui-building-blocks)
- [Theme Notes](#theme-notes)

## Installation

Add Anchor in `manifest.json`:

```json
{
  "dependencies": {
    "com.bovinelabs.anchor": "https://gitlab.com/tertle/com.bovinelabs.anchor.git"
  }
}
```

Required dependencies:

- Unity 6+
- `com.bovinelabs.core`

AppUI is optional. Add AppUI only when you use AppUI-backed adapters/debug UI:

```json
{
  "dependencies": {
    "com.unity.dt.app-ui": "2.2.0-pre.4"
  }
}
```

## Assembly Layout and Compile Gates

- `BovineLabs.Anchor`: AppUI-free core runtime.
- `BovineLabs.Anchor.Adapters`: AppUI-specific adapters (elements and AppUI panel host).
- `BovineLabs.Anchor.Debug`: Debug toolbar and AppUI-based debug views.

AppUI-dependent assemblies are compile-gated with `BL_HAS_APPUI` via asmdef `versionDefines`.

## Project Setup

### 1. Add a runtime UI host

On Unity 6.5 and newer, add a `PanelRenderer` to your scene.

On Unity 6.3 and 6.4, add a `UIDocument` instead.

### 2. Add an app builder

Attach `AnchorAppBuilder` (or a subclass) to the same GameObject:

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;
    using BovineLabs.Core.Utility;
    using BovineLabs.Anchor.DependencyInjection;

    public sealed class MyAppBuilder : AnchorAppBuilder
    {
        protected override void OnConfigureServices(AnchorServiceCollection services)
        {
            base.OnConfigureServices(services);
            services.AddSingleton<IMyService, MyService>();
        }

        protected override void OnAppInitialized(AnchorApp app)
        {
            base.OnAppInitialized(app);
            app.Services.GetRequiredService<IMyService>();
        }
    }
}
```

If AppUI is installed and you want an AppUI-backed root panel:

```csharp
namespace MyGame.UI
{
    using System;
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.Adapters;

    public sealed class MyAppBuilder : AnchorAppBuilder
    {
        protected override Type PanelType { get; } = typeof(AnchorAppUIPanel);
    }
}
```

Runtime access points:

- `AnchorApp.Current`
- `AnchorApp.Current.Services`
- `AnchorApp.Current.NavHost`

### 3. Configure Anchor settings

Use **BovineLabs -> Settings** and configure Anchor settings (views/actions/animations).

## Services and Dependency Injection

Anchor uses `AnchorServiceCollection` and `AnchorServiceProvider`.

Common registrations:

```csharp
services.AddSingleton(typeof(IMyService), typeof(MyService));
services.AddSingleton<IMyService, MyService>();
services.AddTransient(typeof(MyTransientService));

var shared = new SharedSettings();
services.AddSingletonInstance(typeof(SharedSettings), shared);
services.AddAlias(typeof(IFooSettings), typeof(SharedSettings));
services.AddAlias(typeof(IBarSettings), typeof(SharedSettings));
```

Resolution:

```csharp
var provider = AnchorApp.Current.Services;
var service = provider.GetRequiredService<IMyService>();
```

## Authoring Screens in UXML

Anchor uses `IUXMLService` + `data-source-type` for view-model binding.

Example:

```xml
<ui:VisualElement data-source-type="MyGame.UI.Menu.HomeViewModel, MyGame">
    <Unity.AppUI.UI.Heading>
        <Bindings>
            <ui:DataBinding property="text" binding-mode="ToTarget" data-source-path="Title"/>
        </Bindings>
    </Unity.AppUI.UI.Heading>

    <BovineLabs.Anchor.Elements.AnchorActionButton label="@UI:play">
        <Bindings>
            <ui:DataBinding property="clickable.command" binding-mode="ToTarget" data-source-path="PlayCommand"/>
        </Bindings>
    </BovineLabs.Anchor.Elements.AnchorActionButton>
</ui:VisualElement>
```

Notes:

- AppUI controls in UXML require AppUI package + adapter/debug assemblies.
- `BovineLabs.Anchor.Elements.*` types are AppUI adapter elements.

## View Models and Commands

Use `BovineLabs.Anchor.MVVM`:

- `ObservableObject`
- `RelayCommand`, `RelayCommand<T>`
- `[ObservableProperty]`, `[ICommand]`

Example:

```csharp
namespace MyGame.UI.Menu
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;

    [IsService]
    public partial class HomeViewModel : ObservableObject
    {
        [ICommand]
        private void Play()
        {
            AnchorApp.Current.NavHost.Navigate("play");
        }
    }
}
```

Generated command naming follows the existing pattern (`Play` -> `PlayCommand`).

## ECS-Ready Data Binding

`SystemObservableObject<T>` exposes unmanaged data for Burst-friendly UI interaction.

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;

    [IsService]
    public partial class PlayViewModel : SystemObservableObject<PlayViewModel.Data>
    {
        public struct Data
        {
            [SystemProperty]
            public ButtonEvent Start;
        }
    }
}
```

Use `UIHelper<TViewModel, TData>` in systems:

```csharp
public partial struct MyMenuSystem : ISystem, ISystemStartStop
{
    private UIHelper<PlayViewModel, PlayViewModel.Data> helper;

    public void OnStartRunning(ref SystemState state)
    {
        this.helper.Bind();
    }

    public void OnStopRunning(ref SystemState state)
    {
        this.helper.Unbind();
    }

    public void OnUpdate(ref SystemState state)
    {
        ref var binding = ref this.helper.Binding;
        if (binding.Start.TryConsume())
        {
            // Trigger gameplay/state transition
        }
    }
}
```

## Working with AnchorNavHost

Navigate with destination or action names:

```csharp
AnchorApp.Current.NavHost.Navigate("home");
AnchorApp.Current.NavHost.Navigate("profile", AnchorNavArgument.String("userId", "42"));
```

`AnchorNavArgument` replaces AppUI `Argument`.

Common runtime helpers:

- `CurrentDestination`
- `CanGoBack`
- `HasActivePopups`
- `PopBackStack()` / `PopBackStackToPanel()`
- `ClearBackStack()` / `ClearNavigation()`
- `ClosePopup()` / `CloseAllPopups()`

Burst entry points are available via `AnchorNavHost.Burst`.

## Navigation-Aware View Models

Implement `IAnchorNavigationScreen` to receive enter/exit events:

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;

    [IsService]
    public partial class UserReportingViewModel : ObservableObject, IAnchorNavigationScreen
    {
        void IAnchorNavigationScreen.OnEnter(AnchorNavArgument[] args)
        {
            // Screen became active
        }

        void IAnchorNavigationScreen.OnExit(AnchorNavArgument[] args)
        {
            // Screen is leaving
        }
    }
}
```

## Debug Toolbar and ToolbarHelper

Debug toolbar code lives in `BovineLabs.Anchor.Debug` and is intended for editor/debug builds.

Two common patterns:

1. Auto-register a view with `[AutoToolbar]`

```csharp
[AutoToolbar("Save")]
public sealed class SaveToolbarView : View<SaveToolbarViewModel>
{
    public SaveToolbarView()
        : base(new SaveToolbarViewModel())
    {
    }
}
```

2. Use `ToolbarHelper<TView, TViewModel, TData>` for ECS-driven tabs

```csharp
public partial struct EntitiesToolbarSystem : ISystem, ISystemStartStop
{
    private ToolbarHelper<EntitiesToolbarView, EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data> toolbar;

    public void OnCreate(ref SystemState state)
    {
        this.toolbar = new ToolbarHelper<EntitiesToolbarView, EntitiesToolbarViewModel, EntitiesToolbarViewModel.Data>(ref state, "Entities");
    }

    public void OnStartRunning(ref SystemState state) => this.toolbar.Load();

    public void OnStopRunning(ref SystemState state) => this.toolbar.Unload();

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

## UI Building Blocks

Anchor provides reusable pieces for larger UI codebases:

- Navigation: `AnchorNavHost`, `AnchorNavAction`, `AnchorNavOptions`, `AnchorNavArgument`.
- MVVM: `ObservableObject`, relay commands, source-generator attributes.
- ECS binding: `SystemObservableObject<T>`, `UIHelper<TM, TD>`, `SystemPropertyAttribute`.
- AppUI adapters (optional): `AnchorActionButton`, `AnchorButton`, `AnchorAccordion`, `AnchorGridView`, `OptionPager`, `KeyValueElement`, `KeyValueGroup`.

## Theme Notes

Use one of these, depending on whether AppUI is installed:

- `/Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss`
  Includes AppUI theme import plus Anchor toolbar/styles.
- `/Packages/com.bovinelabs.anchor/PackageResources/Anchor No AppUI.tss`
  AppUI-free Anchor style entrypoint.
