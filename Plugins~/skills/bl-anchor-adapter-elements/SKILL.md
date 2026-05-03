---
name: bl-anchor-adapter-elements
description: "Use when creating, wiring, styling, or debugging com.bovinelabs.anchor AppUI adapter elements and UXML controls, including AnchorGridView, AnchorAccordion, AnchorActionButton, AnchorButton, OptionPager, KeyValueGroup, ClassBinding, AnchorSafeArea, Timer, and item-template data-source behavior."
---

# Anchor Adapter Elements

Use this skill for Anchor-specific UI Toolkit/AppUI controls. Resolve Anchor source from the installed package root; in source checkouts this is commonly `Packages/com.bovinelabs.anchor`.

## Workflow

1. Check whether the task is about Anchor adapter behavior or generic Unity App UI. Use the App UI skill for generic AppUI element discovery, theming, or context rules.
2. Confirm the target assembly references `BovineLabs.Anchor.Adapters` and `Unity.AppUI` when using AppUI-backed controls.
3. For repeated item controls, identify who owns each item's `dataSource` before changing UXML.
4. Match UXML attribute names to the lower-camel names exposed by the element source.
5. Keep USS within Unity UI Toolkit-supported properties and verify existing package USS before adding new styling patterns.

## Repeated Item Controls

- `AnchorGridView` extends AppUI `GridView`, exposes `itemTemplate`, and sets each cloned row element's `dataSource` to `itemsSource[index]`.
- `AnchorAccordion` clones `itemTemplate` once per item and sets each `AccordionItem.dataSource` from `itemsSource`.
- Item templates supplied to `AnchorGridView` or `AnchorAccordion` should bind to row properties directly. Do not put a root `data-source-type` on those item templates unless the row really should ignore the supplied item data.
- If a row binding cannot find data, check parent control ownership before changing the view model.

## Buttons And Commands

- `AnchorButton` and `AnchorActionButton` expose `commandWithEventInfo` so a bound `ICommand` can receive the click event payload.
- Use normal AppUI `command`/`clickable.command` when no event payload is needed.
- Bind generated Anchor commands by their generated property name, for example `PlayCommand`.

## OptionPager

- Use `OptionPager` when users should cycle through a compact option list instead of opening a dropdown.
- Bind or set `sourceItems`, `selectedIndex`, `wrap`, `emptyText`, `previousButtonIcon`, `nextButtonIcon`, and `showIndicator`.
- `bindItem` is a C# delegate for rendering the selected item; use code when the default `ToString()` text is not enough.
- The element clamps selection to the current source item range and raises binding notifications for count/selection changes.

## Key Values And Class Binding

- `KeyValueGroup.Create(...)` is the C# helper for compact two-column readouts. It creates rows and binds value labels to view-model paths.
- `ClassBinding` toggles a USS class from a boolean source. In UXML, set `class` and `data-source-path`; use `delay` only when a one-frame-delayed transition trigger is required.
- `ClassBinding` removes the class when the binding deactivates.

## Core Elements

- `AnchorSafeArea` applies padding for configured unsafe edges and updates from `AnchorApp.ScreenMetricsChanged`, panel geometry, and its own geometry.
- `Timer` invokes a bound command repeatedly on `schedule.Execute(...).Every(interval)`; `interval = 0` disables scheduling.

## Guardrails

- Do not use adapter elements as a reason to duplicate generic AppUI controls.
- Do not put service-level `data-source-type` on repeated item templates when the parent element supplies row data.
- Do not add AppUI adapter usage to assemblies that are not gated or referenced for AppUI.
- Do not work around binding warnings by assigning arbitrary root data sources; trace the source owner first.
