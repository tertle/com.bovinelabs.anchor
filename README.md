# BovineLabs Anchor

BovineLabs Anchor is a UI Toolkit application framework for Unity ECS projects. It provides app hosting, service registration, UXML ownership, navigation, MVVM source generation, Burst-to-UI binding, AppUI controls, and an in-game debug toolbar.

For support and discussions, join [Discord](https://discord.gg/RTsw6Cxvw3).

## Requirements

- Unity 6000.7 or newer.
- BovineLabs Core 2.0.0-pre.3 or newer.
- Unity App UI 3.0.0-pre.1 or newer.
- Universal Render Pipeline 17.7.0 or newer.

App UI and URP are direct package dependencies. Anchor's assemblies are still not auto-referenced, so consuming asmdefs must explicitly reference the Anchor and App UI assemblies used by their code. See [Getting started](Documentation~/getting-started.md#requirements) for the current assembly constraints.

## Installation

### BovineLabs Package Manager — recommended

Install the standalone [BovineLabs Package Manager](https://gitlab.com/tertle/com.bovinelabs) once per project:

1. Open **Window > Package Management > Package Manager**.
2. Select **Install package from git URL...** from the add menu and enter:

```text
https://gitlab.com/tertle/com.bovinelabs.git
```

3. Open **Window > Package Management > BovineLabs Package Manager**.
4. Select **BovineLabs Anchor** and click **Install**.

The BovineLabs Package Manager installs this package and its required BovineLabs dependencies as embedded packages under `Packages/`. Commit the
installed package directories to version control. The manager connects to the BovineLabs registry itself; do not add the registry to Unity's scoped
registry settings.

### Git URLs

To install Anchor directly from Git in a fresh project, install Core first, then Anchor. Open the Package Manager, select
**Install package from git URL...**, and enter each URL in this order:

```text
https://gitlab.com/tertle/com.bovinelabs.core.git
https://gitlab.com/tertle/com.bovinelabs.anchor.git
```

The Git versions may contain unpublished changes.

Then follow [Getting started](Documentation~/getting-started.md).

## Sample

Import **Basic UI** from the Package Manager, open `Scenes/Basic UI`, and enter Play mode. Its scene script navigates to the sample screen without changing
project settings.

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

## License

BovineLabs Anchor is licensed under the [MIT License](LICENSE.md).
The bundled Inter font remains under the SIL Open Font License 1.1; see [Third Party Notices](Third%20Party%20Notices.md).
