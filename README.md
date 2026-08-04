# BovineLabs Anchor

BovineLabs Anchor is a UI Toolkit application framework for Unity ECS projects. It provides app hosting, service registration, UXML ownership, navigation, MVVM source generation, Burst-to-UI binding, AppUI controls, and an in-game debug toolbar.

For support and discussions, join [Discord](https://discord.gg/RTsw6Cxvw3).

## Requirements

- Unity 6000.7 or newer.
- BovineLabs Core.
- Unity App UI.
- Universal Render Pipeline.

App UI and URP are direct package dependencies. Anchor's assemblies are still not auto-referenced, so consuming asmdefs must explicitly reference the Anchor and App UI assemblies used by their code. See [Getting started](Documentation~/getting-started.md#requirements) for the current assembly constraints.

## Installation

### Latest stable — recommended

Add the BovineLabs scoped registry to your project:

1. Open **Edit > Project Settings > Package Manager**.
2. Under **Scoped Registries**, add:
   - **Name:** `BovineLabs`
   - **URL:** `https://upm.bovinelabs.com`
   - **Scope:** `com.bovinelabs`
3. Open **Window > Package Management > Package Manager**.
4. Select **My Registries**, choose **BovineLabs Anchor**, and click **Install**.

The scoped registry only needs to be added once per project and can provide all BovineLabs packages.

### Latest experimental

To install the latest development version, open the Package Manager, select **Install package from git URL...**, and enter:

```text
https://gitlab.com/tertle/com.bovinelabs.anchor.git
```

The experimental version tracks the latest development branch and may contain unfinished or breaking changes.

Then follow [Getting started](Documentation~/getting-started.md).

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

## Theme entry points

Use `/Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss` for the default App UI theme and Anchor styles.
