# Getting started

This guide creates the smallest useful Anchor application: one UI Toolkit screen, one Anchor app host, and one configured start destination.

## Requirements

The current package metadata targets Unity 6000.7 or newer and declares `com.bovinelabs.core` 2.0.0-pre.2, Unity App UI 3.0.0-pre.1, and Universal Render Pipeline 17.7.0 as dependencies.

Unity App UI (`com.unity.dt.app-ui`) is required and is installed with Anchor.

The package's assembly definitions are not auto-referenced, so a consuming assembly definition must explicitly reference the Anchor assemblies it uses.

## Install Anchor

### BovineLabs Package Manager — recommended

Install the standalone [BovineLabs Package Manager](https://gitlab.com/tertle/com.bovinelabs) once per project:

1. Open **Window > Package Management > Package Manager**.
2. Select **Install package from git URL...** from the add menu and enter:

```text
https://gitlab.com/tertle/com.bovinelabs.git
```

3. Open **Window > Package Management > BovineLabs Package Manager**.
4. Select **BovineLabs Anchor** and click **Install**.

The manager installs Anchor and its required BovineLabs dependencies as embedded packages under `Packages/`. Commit the installed package directories
to version control. The manager connects to the BovineLabs registry itself; do not add the registry to Unity's scoped registry settings.

### Git or manifest alternative

To install Anchor directly from Git in a fresh project, install Core first, then Anchor. Open the Unity Package Manager, choose
**Install package from git URL...**, and enter each URL in this order:

```text
https://gitlab.com/tertle/com.bovinelabs.core.git
https://gitlab.com/tertle/com.bovinelabs.anchor.git
```

The equivalent `Packages/manifest.json` entries are:

```json
{
  "dependencies": {
    "com.bovinelabs.core": "https://gitlab.com/tertle/com.bovinelabs.core.git",
    "com.bovinelabs.anchor": "https://gitlab.com/tertle/com.bovinelabs.anchor.git"
  }
}
```

The Git versions may contain unpublished changes.

Add `BovineLabs.Anchor` to the references of the runtime assembly that will contain the app builder. Add `BovineLabs.Anchor.Adapters` as well if that assembly uses Anchor's App UI controls.

## Create a start screen

Create `Home.uxml` in the project:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement style="flex-grow: 1; align-items: center; justify-content: center;">
        <ui:Label text="Anchor is running" />
    </ui:VisualElement>
</ui:UXML>
```

This first screen intentionally uses only standard UI Toolkit elements. View-model data sources and App UI controls can be added after the host is working.

## Create an app builder

Create a component for the application host:

```csharp
namespace MyGame.UI
{
    using BovineLabs.Anchor;

    public sealed class GameAppBuilder : AnchorAppBuilder
    {
    }
}
```

The non-generic `AnchorAppBuilder` creates the standard `AnchorApp`. Subclassing it is optional, but gives the application a clear scene component and a place to register project services later.

## Configure Anchor settings

1. Open **BovineLabs > Settings**.
2. Select **Anchor**. The settings framework creates the `AnchorSettings` asset if it does not exist.
3. Add an entry to **Views** with key `home` and assign `Home.uxml` as its asset.
4. Set **Start Destination** to `home`.

View keys are exact string identifiers. `AnchorNavHost` asks `IUXMLService` to instantiate the asset whose key matches the destination.

Actions, animations, debug style sheets, and audio profiles are optional for this first screen.

## Create the UI host

Anchor uses `PanelRenderer` as its UI Toolkit panel component.

1. Create a GameObject named `UI`.
2. Add a `PanelRenderer`.
3. Assign a valid Panel Settings asset to the renderer.
4. Leave the renderer's Source Asset empty.
5. Add `GameAppBuilder` to the same GameObject.

Use a dedicated host for Anchor. The builder clears the host root before attaching its app root, including after a `PanelRenderer` reload. Any visual content supplied by a renderer Source Asset would therefore be removed.

The builder always uses the `PanelRenderer` on the same GameObject.

## Configure the panel theme

Assign the Anchor theme style sheet through the Panel Settings asset:

- `Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss` imports the App UI theme plus Anchor and toolbar styles.

Custom themes must provide the App UI context and include the Anchor styles used by the application.

## Run the app

Enter Play mode. `GameAppBuilder` will:

1. Build the default service provider.
2. Create and attach an `AnchorPanel`.
3. Initialize `AnchorApp.Current`.
4. Create the navigation host.
5. Navigate to `home` and instantiate `Home.uxml`.

At runtime, the main managed access points are:

```csharp
namespace MyGame.UI
{
    using System;
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.Nav;

    public static class GameUI
    {
        public static AnchorApp App => AnchorApp.Current;

        public static IServiceProvider Services => App.Services;

        public static IAnchorNavHost Navigation => App.NavHost;
    }
}
```

Do not use `AnchorApp.Current` directly inside Burst-compiled code. Anchor exposes dedicated Burst navigation entry points for that use case.

## Common startup problems

**The builder logs that no host is assigned**

Put `PanelRenderer` on the same GameObject as the builder.

**The app starts but no screen appears**

Confirm that **Start Destination** is not blank and the same exact key exists in **Views** with a non-null `VisualTreeAsset`.

**Project code cannot resolve Anchor types**

Add `BovineLabs.Anchor` to the consuming `.asmdef`. Add `BovineLabs.Anchor.Adapters` for adapter elements.

**A second app replaces the first**

Multiple active builders are unsupported. Initializing another app logs an error and disposes the previous `AnchorApp.Current`, but it does not shut down the first builder or its service provider. Remove or disable the duplicate builder instead of relying on this replacement behavior.

## Next steps

- [App hosting and services](app-and-services.md)
- [MVVM and binding](mvvm-and-binding.md)
- [Adapter elements](adapter-elements.md)
- [Navigation](navigation.md)
- [Debug toolbar](debug-toolbar.md)
- [Troubleshooting](troubleshooting.md)
