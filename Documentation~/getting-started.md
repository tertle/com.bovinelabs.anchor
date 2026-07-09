# Getting started

This guide creates the smallest useful Anchor application: one UI Toolkit screen, one Anchor app host, and one configured start destination.

## Requirements

The current package metadata targets Unity 6000.3 or newer and declares `com.bovinelabs.core` 1.6.4 as a dependency.

Unity App UI (`com.unity.dt.app-ui`) is optional. Install it when using `BovineLabs.Anchor.Adapters`, App UI-backed controls, or the optional debug toolbar. Anchor's core app, navigation, services, and UI Toolkit elements do not require it.

The package's assembly definitions are not auto-referenced, so a consuming assembly definition must explicitly reference the Anchor assemblies it uses.

## Install Anchor

In Package Manager, choose **Add package from git URL** and enter:

```text
https://gitlab.com/tertle/com.bovinelabs.anchor.git
```

The equivalent `Packages/manifest.json` entry is:

```json
{
  "dependencies": {
    "com.bovinelabs.anchor": "https://gitlab.com/tertle/com.bovinelabs.anchor.git"
  }
}
```

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
3. Leave **Toolbar Only** disabled.
4. Add an entry to **Views** with key `home` and assign `Home.uxml` as its asset.
5. Set **Start Destination** to `home`.

View keys are exact string identifiers. `AnchorNavHost` asks `IUXMLService` to instantiate the asset whose key matches the destination.

Actions, animations, debug style sheets, and audio profiles are optional for this first screen.

## Create the UI host

Anchor uses the UI Toolkit panel component supplied by the Unity version.

### Unity 6000.5 or newer

1. Create a GameObject named `UI`.
2. Add a `PanelRenderer`.
3. Assign a valid Panel Settings asset to the renderer.
4. Leave the renderer's Source Asset empty.
5. Add `GameAppBuilder` to the same GameObject.

Use a dedicated host for Anchor. The builder clears the host root before attaching its app root, including after a `PanelRenderer` reload. Any visual content supplied by a renderer Source Asset would therefore be removed.

### Unity 6000.3 or 6000.4

1. Create a GameObject named `UI`.
2. Add a `UIDocument`, assign a valid Panel Settings asset, and leave Source Asset empty.
3. Add `GameAppBuilder` to the same GameObject.

Anchor clears `UIDocument.rootVisualElement` and attaches its app root on these versions. Do not share this document with another UI tree.

The builder auto-finds a colocated host component. Its serialized host field can also be assigned explicitly in the Inspector.

## Choose the panel theme

Assign an Anchor theme style sheet through the Panel Settings asset:

- `Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss` imports the App UI theme plus Anchor and toolbar styles.
- `Packages/com.bovinelabs.anchor/PackageResources/Anchor No AppUI.tss` imports only Anchor's core styles.

Use the second option in projects that do not install App UI or that provide their own UI Toolkit theme.

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

Put `PanelRenderer` or `UIDocument` on the same GameObject as the builder, or assign the builder's serialized host field.

**The app starts but no screen appears**

Confirm that **Toolbar Only** is disabled, **Start Destination** is not blank, and the same exact key exists in **Views** with a non-null `VisualTreeAsset`.

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
