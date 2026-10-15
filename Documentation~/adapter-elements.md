# Adapter elements and UI utilities

Use stock App UI controls with composable behavior from `BovineLabs.Anchor.Components`. Anchor retains custom elements for numeric text, progress rendering, touch sliders, and other controls that need their own visual implementation.

Components and `GroupedMenuBuilder` live in the core `BovineLabs.Anchor` assembly. Stock App UI controls with Anchor components require `BovineLabs.Anchor` and `Unity.AppUI`; add `BovineLabs.Anchor.Adapters` for the custom elements below. These assemblies are not auto-referenced.

Unity converts C# camel-case `[UxmlAttribute]` names to kebab case. For example, `itemTemplate` is `item-template` and `showIndicator` is `show-indicator` in UXML.

## Directional, textured, and masked progress

The custom progress renderer uses `AnchorSettings.LinearProgressShader`. New settings default to the shader in `BovineLabs.Anchor.Adapters/Shaders`.
When upgrading from Resources-based loading, assign `AnchorLinearProgress.shader` to this field on existing Anchor settings assets.

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

### Grid item template

Attach `GridItemTemplate` to a stock App UI `GridView`. Its `item-template` attribute supplies the `VisualTreeAsset` cloned by `makeItem`; its binder assigns `itemsSource[index]` to the cloned row's `dataSource`. Scrolling, recycling and selection remain owned by App UI.

```xml
<Unity.AppUI.UI.GridView
    data-source-type="Example.UI.CatalogViewModel, Example.UI"
    selection-type="None"
    column-count="4"
    item-height="84">
    <BovineLabs.Anchor.Components.GridItemTemplate
        item-template="project://database/Assets/UI/CatalogItem.uxml#CatalogItem" />
    <Bindings>
        <ui:DataBinding property="itemsSource" data-source-path="Items" binding-mode="ToTarget" />
    </Bindings>
</Unity.AppUI.UI.GridView>
```

Bind the item template directly to row properties. Its root should not declare the screen view model as `data-source-type`, because the grid supplies the row source:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <Unity.AppUI.UI.ActionButton>
        <BovineLabs.Anchor.Components.AudioFeedback />
        <Bindings>
            <ui:DataBinding property="label" data-source-path="Label" binding-mode="ToTarget" />
            <ui:DataBinding property="clickable.command" data-source-path="SelectCommand" binding-mode="ToTarget" />
        </Bindings>
    </Unity.AppUI.UI.ActionButton>
</ui:UXML>
```

When `item-template` is missing, the component creates a `Label` containing `Template Not Found` for each item. Removing the component restores the grid callbacks it replaced, unless another owner has since replaced them.

### Accordion items

Attach `AccordionItems` to a stock App UI `Accordion`. `ItemTemplate` supplies the `item-template` asset; bind the source list to the component's `ItemsSource` field:

```xml
<Unity.AppUI.UI.Accordion data-source-type="Example.UI.ModsViewModel, Example.UI">
    <BovineLabs.Anchor.Components.AccordionItems
        item-template="project://database/Assets/UI/ModItem.uxml#ModItem" />
    <Bindings>
        <ui:DataBinding
            property="${component:BovineLabs.Anchor.Components.AccordionItems}.ItemsSource"
            data-source-path="Mods"
            binding-mode="ToTarget" />
    </Bindings>
</Unity.AppUI.UI.Accordion>
```

The component clones its template directly into the accordion for each source item, preserving App UI's exclusive-open behavior. Use one `AccordionItem` per template, with no service-level `data-source-type` on its row root. Replacing the template/source or removing the component detaches the rows it owns while preserving unrelated children. Detached rows keep their normal garbage-collected lifetime so App UI can finish any active callbacks.

## Buttons and commands

Use stock App UI `Button` and `ActionButton`. Bind `clickable.command` for an ordinary command; App UI supplies the element's `userData` parameter and updates enabled state from `CanExecute`:

```xml
<Unity.AppUI.UI.ActionButton label="Play">
    <BovineLabs.Anchor.Components.AudioFeedback />
    <Bindings>
        <ui:DataBinding property="clickable.command" data-source-path="PlayCommand" binding-mode="ToTarget" />
    </Bindings>
</Unity.AppUI.UI.ActionButton>
```

Attach `EventCommand` when the command needs the activation event:

```csharp
[ICommand]
private void InspectEvent(EventBase evt)
{
    // Inspect pointer, keyboard, or action event details.
}
```

```xml
<Unity.AppUI.UI.Button title="Inspect">
    <BovineLabs.Anchor.Components.EventCommand />
    <BovineLabs.Anchor.Components.AudioFeedback />
    <Bindings>
        <ui:DataBinding
            property="${component:BovineLabs.Anchor.Components.EventCommand}.Command"
            data-source-path="InspectEventCommand"
            binding-mode="ToTarget" />
    </Bindings>
</Unity.AppUI.UI.Button>
```

`EventCommand.Command` receives the original `EventBase` from a Button's `clickedWithEventInfo`, or the targeted `ActionTriggeredEvent` from an ActionButton. Bubbled child actions are ignored. Programmatic Button activation can supply `null`. The component calls `ICommand.Execute` without taking ownership of enabled state or `CanExecuteChanged`; Anchor's relay commands check their own eligibility.

Attach `EventCommand` before `AudioFeedback` to retain command-before-audio ordering for ActionButton. Button follows App UI's fixed order: ordinary command, click/audio callbacks, then event-aware command. Removing the component stops its owned subscription; permanent element release also cleans it up.

### Audio feedback component

`AudioFeedback` adds hover and activation audio to a stock App UI `Button` or `ActionButton`. Attach it explicitly; its profile defaults to `default`.

```xml
<Unity.AppUI.UI.Button title="Play">
    <BovineLabs.Anchor.Components.AudioFeedback audio-profile="menu" hover-audio-mode="Disabled" />
</Unity.AppUI.UI.Button>
```

| UXML attribute | Default | Purpose |
| --- | --- | --- |
| `audio-profile` | `default` | Key in `AnchorAudioSettings` |
| `hover-audio-mode` | `Inherit` | `Inherit`, `Disabled`, or `Custom` |
| `hover-audio-clip` | none | Clip used in `Custom` mode |
| `activate-audio-mode` | `Inherit` | `Inherit`, `Disabled`, or `Custom` |
| `activate-audio-clip` | none | Clip used in `Custom` mode |

Regular buttons play activation audio through their normal click callback. Action buttons accept only an `ActionTriggeredEvent` targeted at the owner. Hover audio is limited to mouse and tracked-pointer entry and does not play while the control is disabled. Removal and permanent release clean up owned subscriptions.

### Pulse component

`Pulse` animates an existing element's opacity, including sealed App UI text controls. It is disabled by default.

```xml
<Unity.AppUI.UI.Text text="Waiting" class="waiting-text">
    <BovineLabs.Anchor.Components.Pulse enabled="true" interval="1200" minimum-opacity="0.35" />
</Unity.AppUI.UI.Text>
```

`interval` is the duration of a whole cycle in milliseconds; a value at or below zero disables animation. `minimum-opacity` and `maximum-opacity` define the range. The corresponding USS properties are `--bl-anchor-pulse-interval`, `--bl-anchor-pulse-minimum-opacity`, and `--bl-anchor-pulse-maximum-opacity`. Disabling or explicitly removing the component restores the element's prior inline opacity.

### Component authoring and bindings

Anchor automatically enables **Project Settings > UI Toolkit > Enable UI Components** and restores it if disabled. Components are authored directly inside their owner and do not create additional visual elements. This experimental flag exposes authoring/debugging UI; component compilation, runtime attachment, and bindings do not depend on it.

Bindings target a component field, for example `${component:BovineLabs.Anchor.Components.Pulse}.Enabled`. Custom bindable components must explicitly declare `: IVisualElementComponent` in their source so Unity's Properties generator can produce the property bag; generators cannot inspect one another's generated declarations.

For C# changes, use the generated setters where available. Direct field writes require a binding notification, and behavior driven by `OnComponentChanged` also needs `MarkComponentDirty<T>`:

```csharp
ref var safeArea = ref element.GetComponent<BovineLabs.Anchor.Components.SafeArea>();
safeArea.Edges = AnchorSafeAreaEdges.Top;
element.NotifyComponentChanged(new BindingId("${component:BovineLabs.Anchor.Components.SafeArea}.Edges"));
element.MarkComponentDirty<BovineLabs.Anchor.Components.SafeArea>();
```

Bindings and generated setters mark changes automatically. Configure Pulse timing and opacity through UXML/USS, or use its generated inline-style setters when overriding USS from C#.

## Numeric text

`AnchorIntegerText` binds an exact `long` value; `AnchorNumberText` binds a `double`. Both derive from AppUI `BaseTextElement` and use its text styling and contexts without passing numeric output through localization token resolution. Keep translated labels in ordinary AppUI text elements.

```xml
<BovineLabs.Anchor.Elements.AnchorNumberText format="0.0" suffix=" ms">
    <Bindings>
        <ui:DataBinding property="number" data-source-path="FrameTime" binding-mode="ToTarget" />
    </Bindings>
</BovineLabs.Anchor.Elements.AnchorNumberText>
```

`number` is the typed numeric binding target. `format` uses standard .NET numeric formatting. `suffix` is appended verbatim. An empty `culture` follows the AppUI language context, falling back to the current culture; set a culture name for a fixed format.

Attached elements format into a reused character buffer and call `TextElement.SetText`; detached elements allocate a string so their `text` remains immediately readable. Initial buffer growth can allocate. Reading `text`, saving text view data, or registering string change listeners can also materialize strings, so measure the complete attached binding path before claiming zero allocations.

`KeyValueGroup.CreateNumeric` builds a numeric key/value layout from typed text elements and raw value paths. It returns a `VisualElement`; the existing string-based `KeyValueGroup.Elements` and `KeyValueElement.ValueLabel` contracts are unchanged.

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

`AnchorTouchSliderFloat` and `AnchorTouchSliderInt` derive directly from App UI's concrete touch sliders. They inherit App UI's localized labels, blinking
text cursor, runtime context menu, inline numeric editor, and numeric parsing. Anchor adds border-aware overflow correction, selects all text when editing
starts, and publishes binding notifications when `size` or `label` changes. A click without a drag opens the editor.

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

The concrete sliders inherit these App UI UXML attributes:

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

`GroupedMenuBuilder.AddGroupedActions` keeps large `MenuBuilder` menus navigable. It lives in `BovineLabs.Anchor`, not `BovineLabs.Anchor.Adapters`, and its API requires `Unity.AppUI.UI`. Without a custom group it sorts normalized labels case-insensitively; with one it sorts by primary group and then label. A menu stays flat while its item count is at or below the configured cap. Larger menus are grouped by initial letter, then recursively by longer normalized prefixes. Blank or non-letter initial labels use the `#` group.

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

## Safe-area padding

Attach `SafeArea` to the existing container whose bounds should be measured. It converts `AnchorApp.SafeArea` from screen coordinates to panel coordinates and applies only the unsafe-edge overlap covered by the owner.

```xml
<ui:VisualElement class="screen-safe-area">
    <BovineLabs.Anchor.Components.SafeArea edges="Top,Left,Right" />
    <!-- Screen content -->
</ui:VisualElement>
```

`Edges` accepts `None`, `Top`, `Bottom`, `Left`, `Right`, or `All` and defaults to `All`. The component reacts to app screen metrics, panel-root geometry and its owner's geometry. It owns all four inline padding values and resets them to zero for invalid geometry, detachment or explicit removal. Put design padding on an inner child. Removal and permanent release unsubscribe owned app/panel listeners.

## Timer component

Attach `Timer` to an existing element to execute a bound command through its scheduler:

```xml
<ui:VisualElement>
    <BovineLabs.Anchor.Components.Timer interval="250" />
    <Bindings>
        <ui:DataBinding
            property="${component:BovineLabs.Anchor.Components.Timer}.Command"
            data-source-path="RefreshCommand"
            binding-mode="ToTarget" />
    </Bindings>
</ui:VisualElement>
```

`Interval` is milliseconds. A value at or below zero pauses and clears the schedule; changing a positive interval replaces the old scheduled item. `Command` is bindable and receives `null`; it is not a literal UXML attribute. Detachment suspends scheduling, and removal/permanent release cancel the owned scheduled item.

## Related binding helpers

- Use [MVVM and data binding](mvvm-and-binding.md) for `ClassBinding`, converter groups, commands, and data-source ownership.
- Use `AnchorObservableCollection<T>` when a repeated control needs bulk collection updates with one reset event.
- Prefer row data supplied by `GridItemTemplate` or `AccordionItems` over adding service-level data sources to their templates.
