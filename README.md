# BovineLabs Anchor

BovineLabs Anchor is a UI Toolkit application framework for Unity ECS projects. It provides app hosting, service registration, UXML ownership, navigation, MVVM source generation, Burst-to-UI binding, AppUI controls, and an in-game debug toolbar.

For support and discussions, join [Discord](https://discord.gg/RTsw6Cxvw3).

## Requirements

- Unity 7000.0.0a7 or newer.
- BovineLabs Core 2.0.0-pre.6 or newer.
- Unity App UI 2.2.4 or newer.
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

Import **Anchor Showcase** from the Package Manager, then choose **BovineLabs > Samples > Anchor Play**. One generated scene uses a persistent button bar
to switch between bindings, collections, particles, animations and Burst ECS examples. **Anchor Generate** rebuilds `Generated/Scenes/Main.unity` and its
assets. See the [Showcase README](Samples~/Showcase/README.md) for entry points and implementation details.

## Documentation

| Guide | Covers |
|---|---|
| [Overview](Documentation~/index.md) | Requirements, assemblies, themes, and the recommended workflow |
| [Getting started](Documentation~/getting-started.md) | Install Anchor, create a host, configure settings, and open the first screen |
| [Application and services](Documentation~/app-and-services.md) | App lifecycle, dependency injection, UXML ownership, storage, and UI audio |
| [Navigation](Documentation~/navigation.md) | Destinations, actions, popups, arguments, callbacks, saved state, animations, and Burst entry points |
| [MVVM and binding](Documentation~/mvvm-and-binding.md) | Observable objects, generated properties and commands, ECS-facing data, and `UIHelper` |
| [Components and adapter elements](Documentation~/adapter-elements.md) | Stock App UI composition, repeated-item templates, commands, safe areas, timers, and custom controls |
| [Debug toolbar](Documentation~/debug-toolbar.md) | Auto toolbar views, ECS-backed tabs, build symbols, and lifecycle |
| [Troubleshooting](Documentation~/troubleshooting.md) | Assembly references, host startup, UXML binding, navigation, and toolbar diagnostics |

## Package layout

| Assembly | Purpose |
|---|---|
| `BovineLabs.Anchor` | App hosting, services, navigation, MVVM, binding, UI components, core elements, audio, and utilities |
| `BovineLabs.Anchor.Adapters` | Custom controls such as numeric text, `AnchorLinearProgress`, touch sliders, and `OptionPager` |
| `BovineLabs.Anchor.Debug` | AppUI-based runtime debug toolbar and built-in panels |
| `BovineLabs.Anchor.Editor` | Anchor settings and navigation asset inspectors |
| `BovineLabs.Anchor.Tests` | Package EditMode tests |

The runtime, adapter, debug, and editor assemblies have `autoReferenced` disabled. Add the assemblies you use to your own `.asmdef`; installing the package alone does not make their types visible.

## UI component migration

Use stock App UI controls with behavior from `BovineLabs.Anchor.Components`. The components live in `BovineLabs.Anchor`; reference `Unity.AppUI` for the controls. Enable **Project Settings > UI Toolkit > Enable UI Components** to expose Unity's experimental component authoring interface.

This release removes six former elements:

| Removed element | Replacement |
| --- | --- |
| `AnchorButton` | App UI `Button` plus `AudioFeedback`; add `EventCommand` for event-aware commands |
| `AnchorActionButton` | App UI `ActionButton` plus the same components |
| `AnchorGridView` | App UI `GridView` plus `GridItemTemplate` |
| `AnchorAccordion` | App UI `Accordion` plus `AccordionItems` |
| `AnchorSafeArea` | Existing container plus `SafeArea` |
| `BovineLabs.Anchor.Elements.Timer` | Existing element plus `BovineLabs.Anchor.Components.Timer` |

Move element-specific attributes onto the component. Bind former `commandWithEventInfo` targets to `${component:BovineLabs.Anchor.Components.EventCommand}.Command`; bind accordion sources to `${component:BovineLabs.Anchor.Components.AccordionItems}.ItemsSource`. Add `EventCommand` before `AudioFeedback` to preserve ActionButton command/audio ordering. See [Components and adapter elements](Documentation~/adapter-elements.md) for examples.

## Theme entry points

Use `/Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss` for the default App UI theme and Anchor styles.

## License

BovineLabs Anchor is licensed under the [MIT License](LICENSE.md).
The bundled Inter font remains under the SIL Open Font License 1.1; see [Third Party Notices](Third%20Party%20Notices.md).
