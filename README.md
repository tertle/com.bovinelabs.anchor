# BovineLabs Anchor

BovineLabs Anchor is a UI Toolkit application framework for Unity ECS projects. It provides app hosting, service registration, UXML ownership, navigation, MVVM source generation, Burst-to-UI binding, optional AppUI controls, and an in-game debug toolbar.

For support and discussions, join [Discord](https://discord.gg/RTsw6Cxvw3).

## Documentation

| Guide | Covers |
|---|---|
| [Overview](Documentation~/index.md) | Requirements, assemblies, themes, and the recommended workflow |
| [Getting started](Documentation~/getting-started.md) | Install Anchor, create a host, configure settings, and open the first screen |
| [Application and services](Documentation~/app-and-services.md) | App lifecycle, dependency injection, UXML ownership, storage, and UI audio |
| [Navigation](Documentation~/navigation.md) | Destinations, actions, popups, arguments, callbacks, saved state, animations, and Burst entry points |
| [MVVM and binding](Documentation~/mvvm-and-binding.md) | Observable objects, generated properties and commands, ECS-facing data, and `UIHelper` |
| [Adapter elements](Documentation~/adapter-elements.md) | AppUI controls, repeated-item templates, command payloads, safe areas, and timers |
| [Debug toolbar](Documentation~/debug-toolbar.md) | Auto toolbar views, ECS-backed tabs, build symbols, and lifecycle |
| [Troubleshooting](Documentation~/troubleshooting.md) | Assembly references, host startup, UXML binding, navigation, and toolbar diagnostics |

## Package layout

| Assembly | Purpose |
|---|---|
| `BovineLabs.Anchor` | App hosting, services, navigation, MVVM, binding, core elements, audio, and utilities |
| `BovineLabs.Anchor.Adapters` | AppUI-backed controls such as `AnchorActionButton`, `AnchorGridView`, and `OptionPager` |
| `BovineLabs.Anchor.Debug` | AppUI-based runtime debug toolbar and built-in panels |
| `BovineLabs.Anchor.Editor` | Anchor settings and navigation asset inspectors |
| `BovineLabs.Anchor.Tests` | Package EditMode tests |

The runtime, adapter, debug, and editor assemblies have `autoReferenced` disabled. Add the assemblies you use to your own `.asmdef`; installing the package alone does not make their types visible.

## Requirements

- Unity 6000.7 or newer.
- `com.bovinelabs.core` 2.0.0 or a compatible newer version.
- A compatible Unity App UI package only when using `BovineLabs.Anchor.Adapters`, `BovineLabs.Anchor.Debug`, or other AppUI-specific APIs such as `GroupedMenuBuilder`.

Optional package integrations are enabled through asmdef `versionDefines`, including `UNITY_APPUI` and `UNITY_URP`. An optional assembly reference does not make that package a dependency. AppUI is not declared in `package.json`; install it explicitly only before referencing AppUI-backed Anchor APIs. See [Getting started](Documentation~/getting-started.md#requirements) for the current assembly constraints.

## Installation

Add the Git URL through Package Manager, or add Anchor to the project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.bovinelabs.anchor": "https://gitlab.com/tertle/com.bovinelabs.anchor.git"
  }
}
```

Then follow [Getting started](Documentation~/getting-started.md).

## Theme entry points

- Use `/Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss` with AppUI. It imports the AppUI theme and Anchor styles.
- Use `/Packages/com.bovinelabs.anchor/PackageResources/Anchor No AppUI.tss` for the plain UI Toolkit panel.
