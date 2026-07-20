# BovineLabs Anchor

BovineLabs Anchor is a UI Toolkit application framework for Unity ECS projects. It provides app hosting, service registration, UXML ownership, navigation, MVVM source generation, Burst-to-UI binding, AppUI controls, and an in-game debug toolbar.

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
- Unity App UI 2.2.0 or a compatible newer version.
- Universal Render Pipeline 17.7.0 or newer.

App UI and URP are direct package dependencies. Anchor's assemblies are still not auto-referenced, so consuming asmdefs must explicitly reference the Anchor and App UI assemblies used by their code. See [Getting started](Documentation~/getting-started.md#requirements) for the current assembly constraints.

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

Use `/Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss` for the default App UI theme and Anchor styles.
