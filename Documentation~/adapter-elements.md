# Adapter elements and UI utilities

Anchor supplies a small set of AppUI adapters plus a few UI Toolkit-only elements. Use them where they add Anchor behavior; use the original AppUI control when no adapter behavior is needed.

Most AppUI-backed element types compile in `BovineLabs.Anchor.Adapters`. A consuming asmdef must reference both `BovineLabs.Anchor.Adapters` and `Unity.AppUI`. The adapter assembly is enabled only when `UNITY_APPUI` is available and is not auto-referenced. `GroupedMenuBuilder` is the layout exception: it lives in the core `BovineLabs.Anchor` assembly, but is also compiled only when `UNITY_APPUI` is available.

Unity converts C# camel-case `[UxmlAttribute]` names to kebab case. For example, `itemTemplate` is `item-template` and `showIndicator` is `show-indicator` in UXML.

## Directional, textured, and masked progress

`AnchorLinearProgress` preserves AppUI progress properties such as `value`, `buffer-value`, `variant`, `size`, `color-override`, and
`rounded-progress-corners`, then adds a selectable progress axis, an optional fill texture, and an optional alpha mask. It intentionally does not use AppUI's
`appui-linear-progress` root class, so the theme does not impose bar dimensions; set both width and height through caller USS or inline UXML styles.

The generated progress mesh renders over `.appui-progress__image`, so a USS `background-image` on that internal element is not value-clipped. Use
`fill-texture` for imagery that should be revealed by the progress value; keep track/background imagery on the root or a sibling element.

The same texture can be supplied from USS. An explicit `fill-texture` UXML attribute or C# `fillTexture` assignment takes precedence; clear it to fall back
to the resolved USS value:

```css
.health-progress {
    --bl-anchor-linear-progress-fill-texture: url("project://database/Assets/UI/Textures/HealthFill.png");
}
```

| UXML attribute | Default | Purpose |
| --- | --- | --- |
| `direction` | `Horizontal` | `Horizontal` uses normal AppUI directionality; `Vertical` fills bottom-to-top in LTR and top-to-bottom in RTL |
| `fill-texture` | none | Stretches a `Texture2D` over the element and reveals it through the progress and buffer values |
| `mask-texture` | none | Stretches a `Texture2D` over the element and multiplies the complete progress output by its alpha |

```xml
<BovineLabs.Anchor.Elements.AnchorLinearProgress
    direction="Vertical"
    variant="Determinate"
    value="0.75"
    buffer-value="0.75"
    fill-texture="project://database/Assets/UI/Textures/HealthFill.png"
    rounded-progress-corners="false"
    mask-texture="project://database/Assets/UI/Masks/GlobeMask.png"
    style="width: 128px; height: 128px;" />
```

The fill texture keeps its alpha and its RGB is tinted by `color-override` or `--progress-color`; use white to preserve the source RGB. Its UVs span the
complete element, so changing `value` reveals more of a stationary image instead of rescaling it. Without a fill texture, progress uses the normal AppUI
color behavior.

The mask's RGB channels are ignored, and neither texture needs CPU read/write access. Texture UVs stretch to the element bounds, so preserve the desired
silhouette through the element's layout. A missing mask behaves like a normal unmasked progress element.

The active AppUI direction context controls the fill origin. Horizontal progress follows AppUI's normal LTR/RTL mirroring. Vertical progress rises in LTR
and descends in RTL; changing the context at runtime repaints the element.

For a health or mana globe, use `AnchorLinearProgress` as the colored fill layer and place decorative frame, glass, and sheen elements after it as sibling
overlays. The control provides linear fill through the silhouette; it does not provide radial sweep, liquid imagery, or wave animation.

## Repeated item controls

### AnchorGridView

`AnchorGridView` derives from AppUI `GridView` and adds one UXML attribute:

| UXML attribute | Type | Purpose |
| --- | --- | --- |
| `item-template` | `VisualTreeAsset` | Template cloned by `makeItem` for every grid slot |

The adapter's `bindItem` assigns `itemsSource[index]` to the cloned row's `dataSource`:

```xml
<BovineLabs.Anchor.Elements.AnchorGridView
    data-source-type="Example.UI.CatalogViewModel, Example.UI"
    item-template="project://database/Assets/UI/CatalogItem.uxml#CatalogItem"
    selection-type="None"
    column-count="4"
    item-height="84">
    <Bindings>
        <ui:DataBinding
            property="itemsSource"
            data-source-path="Items"
            binding-mode="ToTarget" />
    </Bindings>
</BovineLabs.Anchor.Elements.AnchorGridView>
```

Bind the item template directly to row properties. Its root should not declare the screen view model as `data-source-type`, because that would replace the row source:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <BovineLabs.Anchor.Elements.AnchorActionButton>
        <Bindings>
            <ui:DataBinding property="label" data-source-path="Label" binding-mode="ToTarget" />
            <ui:DataBinding property="clickable.command" data-source-path="SelectCommand" binding-mode="ToTarget" />
        </Bindings>
    </BovineLabs.Anchor.Elements.AnchorActionButton>
</ui:UXML>
```

When `item-template` is missing, the grid creates a `Label` containing `Template Not Found` for each item.

### AnchorAccordion

`AnchorAccordion` derives from AppUI `Accordion`. It clones its template once per source item, then assigns each discovered `AccordionItem.dataSource` in source order.

| UXML attribute | Type | Purpose |
| --- | --- | --- |
| `item-template` | `VisualTreeAsset` | Template cloned for each source item |

`itemsSource` is bindable but is not a literal UXML attribute:

```xml
<BovineLabs.Anchor.Elements.AnchorAccordion
    data-source-type="Example.UI.ModsViewModel, Example.UI"
    item-template="project://database/Assets/UI/ModItem.uxml#ModItem">
    <Bindings>
        <ui:DataBinding property="itemsSource" data-source-path="Mods" binding-mode="ToTarget" />
    </Bindings>
</BovineLabs.Anchor.Elements.AnchorAccordion>
```

The template must produce one `AccordionItem` per clone. As with the grid, leave the row root's service-level `data-source-type` unset.

## Buttons and commands

`AnchorButton` derives from AppUI `Button`; `AnchorActionButton` derives from AppUI `ActionButton`. Both add:

- `commandWithEventInfo`, a bindable `ICommand` that receives the click or action event.
- Named-profile hover and activation audio with optional per-control overrides.

Use the normal AppUI command when the handler needs no event payload:

```xml
<BovineLabs.Anchor.Elements.AnchorActionButton label="Play">
    <Bindings>
        <ui:DataBinding property="clickable.command" data-source-path="PlayCommand" binding-mode="ToTarget" />
    </Bindings>
</BovineLabs.Anchor.Elements.AnchorActionButton>
```

Bind `commandWithEventInfo` when a generated command accepts the event:

```csharp
[ICommand]
private void InspectEvent(EventBase evt)
{
    // Inspect pointer, keyboard, or action event details.
}
```

```xml
<BovineLabs.Anchor.Elements.AnchorButton label="Inspect">
    <Bindings>
        <ui:DataBinding
            property="commandWithEventInfo"
            data-source-path="InspectEventCommand"
            binding-mode="ToTarget" />
    </Bindings>
</BovineLabs.Anchor.Elements.AnchorButton>
```

The Anchor-specific audio attributes are the same on both controls:

| UXML attribute | Default | Purpose |
| --- | --- | --- |
| `audio-profile` | `default` | Key in `AnchorAudioSettings` |
| `hover-audio-mode` | `Inherit` | `Inherit`, `Disabled`, or `Custom` |
| `hover-audio-clip` | none | Clip used in `Custom` mode |
| `activate-audio-mode` | `Inherit` | `Inherit`, `Disabled`, or `Custom` |
| `activate-audio-clip` | none | Clip used in `Custom` mode |

`AnchorButton` plays activate audio through its normal click callback. `AnchorActionButton` handles only an `ActionTriggeredEvent` whose target is the button itself, which avoids reacting to a bubbled child action. Hover audio is limited to mouse and tracked-pointer entry and does not play while the control is disabled.

## OptionPager

`OptionPager` is a compact alternative to a dropdown. It renders previous and next `ActionButton`s, the selected item, and an optional `PageIndicator`.

```xml
<BovineLabs.Anchor.Elements.OptionPager
    selected-index="0"
    wrap="true"
    empty-text="No options"
    previous-button-icon="caret-left"
    next-button-icon="caret-right"
    show-indicator="true">
    <Bindings>
        <ui:DataBinding property="sourceItems" data-source-path="Options" binding-mode="ToTarget" />
        <ui:DataBinding property="selectedIndex" data-source-path="SelectedIndex" binding-mode="TwoWay" />
    </Bindings>
</BovineLabs.Anchor.Elements.OptionPager>
```

| UXML attribute | Default | Notes |
| --- | --- | --- |
| `selected-index` | `-1` before items arrive | Clamped to `0..Count - 1`; becomes `-1` while empty |
| `wrap` | `true` | Wrap previous and next at the bounds |
| `empty-text` | empty | Text shown with no items |
| `previous-button-icon` | `caret-left` | AppUI icon name |
| `next-button-icon` | `caret-right` | AppUI icon name |
| `show-indicator` | `true` | Shows the page indicator |

`sourceItems`, `optionsCount`, `selectedText`, and `bindItem` are bindable C# properties, not literal UXML attributes. Without `bindItem`, the selected item uses `ToString()`. For richer labels or icons, assign AppUI's `Dropdown.BindItemFunc` in C#:

```csharp
pager.bindItem = (item, index) =>
{
    var option = (QualityOption)pager.sourceItems[index];
    item.label = option.Label;
    item.icon = option.Icon;
};
```

Selection changes send `ChangeEvent<int>` from the pager. With wrapping disabled, buttons become disabled at their corresponding bounds. Replacing the source re-applies the desired index, updates the indicator, and notifies count, selection, and selected text as needed.

The element exposes stable USS hooks:

- `bl-option-pager`
- `bl-option-pager__controls`
- `bl-option-pager__previous`
- `bl-option-pager__center`
- `bl-option-pager__value`
- `bl-option-pager__next`
- `bl-option-pager__indicator`

## Touch sliders

`AnchorTouchSliderFloat` and `AnchorTouchSliderInt` preserve AppUI touch-slider styling while fixing overflow behavior and providing an inline numeric editor. A click without a drag opens the editor; accepted text is parsed with Unity's `ExpressionEvaluator`.

```xml
<BovineLabs.Anchor.Elements.AnchorTouchSliderFloat
    label="Volume"
    size="M"
    low-value="0"
    high-value="1"
    value="0.5"
    step="0.05"
    shift-step="0.25">
    <Bindings>
        <ui:DataBinding property="value" data-source-path="Volume" binding-mode="TwoWay" />
    </Bindings>
</BovineLabs.Anchor.Elements.AnchorTouchSliderFloat>
```

Anchor declares these UXML attributes on the concrete sliders:

| Attribute | Float default | Int default |
| --- | --- | --- |
| `label` | empty | empty |
| `size` | `M` | `M` |
| `low-value` | `0` | `0` |
| `high-value` | `1` | `1` |
| `value` | `0` | `0` |
| `step` | `0.1` | `1` |
| `shift-step` | `1` | `10` |

Inherited AppUI base-slider attributes and bindable properties, including orientation, remain available. The sliders do not expose a content container, so do not add child elements to them. Style the control through the AppUI `appui-touchslider*` classes or the Anchor scope class `bl-touchslider-workaround` rather than replacing its internal hierarchy.

## Key-value readouts

`KeyValueGroup` builds two aligned columns of AppUI `Text` elements. It is primarily a C# helper for compact status and debug panels:

```csharp
this.Add(KeyValueGroup.Create(viewModel,
    new[]
    {
        ("Entities", nameof(StatusViewModel.EntityCount)),
        ("Chunks", nameof(StatusViewModel.ChunkCount)),
    }));
```

The converter overload lets each row customize its `DataBinding`:

```csharp
TypeConverter<int, string> countConverter = static (ref int value) => value.ToString("N0");

this.Add(KeyValueGroup.Create(viewModel,
    new (string, string, Action<DataBinding>)[]
    {
        ("Entities", nameof(StatusViewModel.EntityCount),
            binding => binding.sourceToUiConverters.AddConverter(countConverter)),
    }));
```

Both overloads default to `BindingUpdateTrigger.OnSourceChanged`. `KeyValueGroup.Elements` exposes the created `KeyValueElement` pairs for further styling.

## Large AppUI menus

`GroupedMenuBuilder.AddGroupedActions` keeps large `MenuBuilder` menus navigable. It lives in `BovineLabs.Anchor`, not `BovineLabs.Anchor.Adapters`, but is guarded by `UNITY_APPUI` and its API requires `Unity.AppUI.UI`. Without a custom group it sorts normalized labels case-insensitively; with one it sorts by primary group and then label. A menu stays flat while its item count is at or below the configured cap. Larger menus are grouped by initial letter, then recursively by longer normalized prefixes. Blank or non-letter initial labels use the `#` group.

```csharp
var menu = new Menu();
var builder = MenuBuilder.Build(anchorElement, menu);

builder.AddGroupedActions(
    items,
    item => item.Label,
    item => item.Category,
    item => Select(item),
    new GroupedMenuBuilderOptions
    {
        MaxItemsPerMenu = 10,
        MaxPrefixLength = 6,
        FlattenSingleItemGroups = true,
    });
```

The primary group selector is optional. Another overload accepts an `IComparer<string>` to impose a domain-specific group order. Defaults are 12 entries per menu level, a maximum normalized prefix length of 8, and flattened one-item groups. Values below 2 for `MaxItemsPerMenu` are clamped to 2; prefix lengths below 1 are clamped to 1. Duplicate labels remain separate actions and invoke the callback with the correct source item.

## Safe-area container

`AnchorSafeArea` is in the core `BovineLabs.Anchor` assembly and does not require an adapter reference. It converts `AnchorApp.SafeArea` from screen coordinates into panel coordinates and applies only the unsafe-edge overlap covered by the element's own bounds.

```xml
<BovineLabs.Anchor.Elements.AnchorSafeArea
    edges="Top,Left,Right"
    class="screen-safe-area">
    <!-- Screen content -->
</BovineLabs.Anchor.Elements.AnchorSafeArea>
```

The `edges` flag attribute accepts `None`, `Top`, `Bottom`, `Left`, `Right`, or `All` and defaults to `All`. The element reacts to app screen-metric changes, panel-root geometry, and its own geometry.

`AnchorSafeArea` owns all four inline padding values and resets them to zero when geometry is invalid or the element detaches. Put design padding on an inner child rather than on the safe-area element itself.

## Timer element

`Timer` is also in the core assembly. It repeatedly executes a bound `ICommand` through the UI Toolkit scheduler:

```xml
<BovineLabs.Anchor.Elements.Timer interval="250">
    <Bindings>
        <ui:DataBinding property="command" data-source-path="RefreshCommand" binding-mode="ToTarget" />
    </Bindings>
</BovineLabs.Anchor.Elements.Timer>
```

`interval` is milliseconds. A value less than or equal to zero pauses and clears the schedule. Changing a positive interval pauses the old scheduled item before creating the replacement. `command` is bindable but is not a literal UXML attribute.

## Related binding helpers

- Use [MVVM and data binding](mvvm-and-binding.md) for `ClassBinding`, converter groups, commands, and data-source ownership.
- Use `AnchorObservableCollection<T>` when a repeated control needs bulk collection updates with one reset event.
- Prefer row data supplied by `AnchorGridView` or `AnchorAccordion` over adding service-level data sources to their templates.
