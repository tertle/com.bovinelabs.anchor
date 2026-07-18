# MVVM and data binding

Anchor builds on Unity UI Toolkit runtime binding. It adds observable view-model bases, source-generated properties and commands, small binding helpers, and a bridge that lets an ECS system publish unmanaged state to a managed view model.

Use the two paths for different kinds of state:

| State owner | View model | Update path |
| --- | --- | --- |
| Managed UI or service code | `ObservableObject` | Set generated or hand-written properties |
| ECS system | `SystemObservableObject<TData>` | Bind with `UIHelper<TViewModel, TData>`, then set generated properties on `TData` |

For app startup, service registration, and UXML asset ownership, see [App and services](app-and-services.md).

## Managed view models

Derive from `ObservableObject`, mark the type `partial`, and register it as an Anchor service when UXML will resolve it by type:

```csharp
namespace Example.UI
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using UnityEngine.Scripting;

    [Preserve]
    [IsService]
    public partial class MenuViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = "Main menu";

        [ObservableProperty]
        private bool hasSave;

        [DependsOn(nameof(HasSave))]
        private bool CanContinue => this.HasSave;

        [ICommand(CanExecuteProperty = nameof(CanContinue))]
        private void Continue()
        {
            AnchorApp.Current.NavHost.Navigate("game");
        }
    }
}
```

The generator produces bindable `Title`, `HasSave`, and `ContinueCommand` properties. Generated observable properties call `ObservableObject.SetProperty`, which raises both .NET property notifications and UI Toolkit `INotifyBindablePropertyChanged` notifications only when the value changes.

### Generator attributes

| Attribute | Apply to | Generated behavior |
| --- | --- | --- |
| `[ObservableProperty]` | Writable instance field | A public `[CreateProperty]` property that calls `SetProperty` |
| `[AlsoNotifyChangeFor(...)]` | `[ObservableProperty]` field | Raises additional property-changed notifications after a real change |
| `[AlsoExecute(...)]` | `[ObservableProperty]` field | Invokes parameterless, non-generic methods after a real change |
| `[DependsOn(...)]` | Computed property | Raises a notification for the computed property when a dependency changes |
| `[ICommand]` | Method | A lazy, read-only `[CreateProperty]` named `<MethodName>Command` |

Dependencies declared with `[DependsOn]` are expanded transitively. A dependency can name a generated property or its backing field; backing-field names are normalized to the generated property name.

All types using these attributes must be `partial`. `[ObservableProperty]` fields must not be static, const, or readonly. The field names `_title`, `m_title`, `mTitle`, and `title` all generate `Title`; a bare `m` prefix is stripped only when the next character is uppercase.

`[ICommand]` methods must return `void` and accept zero or one non-`ref` parameter. Use one of the following optional gates, never both:

```csharp
[ICommand(CanExecuteMethod = nameof(CanDelete))]
private void Delete(Item item) { }

private bool CanDelete(Item item) => item != null;
```

```csharp
[ICommand(CanExecuteProperty = nameof(CanSave))]
private void Save() { }

[DependsOn(nameof(IsDirty))]
private bool CanSave => this.IsDirty;
```

A `CanExecuteMethod` must return `bool` and have the same parameters as the command method. A `CanExecuteProperty` must be a readable `bool`. On an `INotifyPropertyChanged` view model, the generated command observes that property and raises `CanExecuteChanged` when it changes. Use `[DependsOn]` for a computed gate so that invalidation happens automatically.

`RelayCommand` and `RelayCommand<T>` are also available for commands that should be assembled manually. `RelayCommand<T>.Execute(object)` rejects a parameter of the wrong type, and `null` is invalid for a non-nullable value type.

> **Current limitation:** `ObservableObjectAttribute` exists in the runtime assembly, but the current MVVM generator does not consume it. Derive from `ObservableObject`; the attribute is not a substitute for the base class.

## Bind from UXML

`data-source-type` uses the assembly-qualified type name without a version. When the tree is created through `IUXMLService.Instantiate`, Anchor resolves every visual element that declares the attribute from `AnchorApp.Current.Services` and assigns its `dataSource`.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" editor-extension-mode="False">
    <ui:VisualElement
        data-source-type="Example.UI.MenuViewModel, Example.UI"
        class="menu">
        <ui:Label>
            <Bindings>
                <ui:DataBinding
                    property="text"
                    data-source-path="Title"
                    binding-mode="ToTarget" />
            </Bindings>
        </ui:Label>

        <BovineLabs.Anchor.Elements.AnchorActionButton label="Continue">
            <Bindings>
                <ui:DataBinding
                    property="clickable.command"
                    data-source-path="ContinueCommand"
                    binding-mode="ToTarget" />
            </Bindings>
        </BovineLabs.Anchor.Elements.AnchorActionButton>
    </ui:VisualElement>
</ui:UXML>
```

Replace `Example.UI` after the comma with the view model's assembly name. The button in this example is an optional AppUI adapter; see [Adapter elements](adapter-elements.md).

In a stripped player build, use `[Preserve]` for a service type that is reached only through reflection or a UXML type string.

Calling `VisualTreeAsset.Instantiate` directly does not run Anchor's service-resolution pass. Use `IUXMLService` for service-backed trees, or assign `dataSource` yourself. Do not put a service-level `data-source-type` on a grid or accordion item template when the repeating control supplies each row's data source.

## Bind from C#

Plain C# views should accept their model explicitly and assign it as the hierarchical data source. You can set the data source on the view or on each bound element:

```csharp
this.dataSource = viewModel;

var title = new Label();

title.SetBindingToUI(nameof(Label.text), nameof(MenuViewModel.Title));
```

Anchor's `VisualElementExtensions` provide these shortcuts:

- `SetBindingToUI` creates a `BindingMode.ToTarget` binding.
- `SetBindingFromUI` creates a `BindingMode.ToSource` binding.
- `SetBindingTwoWay` creates the normal two-way binding.
- Each shortcut has a converter overload for different source and UI types.
- `TryResolveDataSource<T>` resolves the effective hierarchical data source and data-source path for an element.
- `SetPickingModeRecursive` applies a picking mode to an element and its descendants.

Use an explicit `DataBinding` when you need an update trigger, a named converter group, or several converters:

```csharp
toggle.SetBinding(nameof(toggle.value), new DataBinding
{
    bindingMode = BindingMode.TwoWay,
    dataSourcePath = new PropertyPath(nameof(SettingsViewModel.Enabled)),
    updateTrigger = BindingUpdateTrigger.OnSourceChanged,
});
```

## Converter groups

`BovineLabs.Anchor.Binding.Converters` registers three named groups during subsystem initialization:

| Name | Conversion |
| --- | --- |
| `DisplayStyle` | `true` to `DisplayStyle.Flex`; `false` to `DisplayStyle.None`, with reverse conversion |
| `DisplayStyleInverted` | Inverted display-style conversion |
| `Invert` | Boolean negation |

Use a group by name in UXML:

```xml
<ui:VisualElement>
    <Bindings>
        <ui:DataBinding
            property="style.display"
            data-source-path="Visible"
            binding-mode="ToTarget"
            source-to-ui-converters="DisplayStyle" />
    </Bindings>
</ui:VisualElement>
```

## Toggle a USS class

`ClassBinding` is a custom binding that enables or disables one USS class from a boolean source. It also accepts `IConvertible` values, removes its class when deactivated, and uses `BindingUpdateTrigger.OnSourceChanged`.

```xml
<ui:VisualElement>
    <Bindings>
        <BovineLabs.Anchor.ClassBinding
            property="selected-class"
            class="is-selected"
            data-source-path="Selected" />
    </Bindings>
</ui:VisualElement>
```

For this custom binding, `property` is only the binding registration key; it does not need to name a real target property. The Anchor-specific UXML attributes are:

- `class`: required non-empty USS class name.
- `data-source-path`: optional property path. An empty path converts the data source itself.
- `delay`: schedules the class change for the next UI update, which is useful when a transition must observe the initial state first.

In C#, the equivalent shape is:

```csharp
element.SetBinding("selected-class", new ClassBinding
{
    Class = "is-selected",
    dataSourcePath = new PropertyPath(nameof(RowViewModel.Selected)),
});
```

## Observable collections

`AnchorObservableCollection<T>` extends `ObservableCollection<T>` with bulk operations for repeated controls:

```csharp
private readonly AnchorObservableCollection<RowViewModel> rows = new();

public AnchorObservableCollection<RowViewModel> Rows => this.rows;

public void Refresh(IEnumerable<RowViewModel> values)
{
    this.rows.Replace(values);
}
```

`Replace` and `AddRange` emit one `NotifyCollectionChangedAction.Reset` when their operation changes the collection, rather than one event per item. A `null` sequence is a no-op. These bulk methods do not emit the normal per-item `PropertyChanged` notifications for `Count` or the indexer.

Register an IList property bag before Unity's property system must traverse this concrete collection type:

```csharp
PropertyBag.RegisterIList<AnchorObservableCollection<RowViewModel>, RowViewModel>();
```

For indexed row binding, set the collection as the data source and use `PropertyPath.FromIndex(index)` as the element's `dataSourcePath`.

## ECS-backed view models

`SystemObservableObject<TData>` keeps an unmanaged `TData` value inside the managed view model. `[SystemProperty]` generates properties on a `partial struct` that update the field through Anchor's Burst-to-binding interop.

```csharp
namespace Example.UI
{
    using BovineLabs.Anchor;
    using Unity.Properties;

    public partial class StatusViewModel : SystemObservableObject<StatusViewModel.Data>
    {
        [CreateProperty(ReadOnly = true)]
        public int EntityCount => this.Value.EntityCount;

        public partial struct Data
        {
            [SystemProperty]
            private int entityCount;
        }
    }
}
```

`SystemObservableObject<TData>` is already an inherited Anchor service. Keep an ECS-backed view model singleton so the UXML tree and `UIHelper` resolve the same instance.

The data struct and every field must remain unmanaged. A `[SystemProperty]` field declaration must contain exactly one field and cannot be static, const, or readonly. Its containing struct must be `partial`. Use `camelCase` or `_camelCase` field names; this generator strips only one leading underscore. Generated scalar setters require the field type to implement `IEquatable<T>`; Unity primitives and most Unity value types already do.

### UIHelper lifecycle

Bind the view model while its ECS system is running:

```csharp
namespace Example.UI
{
    using BovineLabs.Anchor;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(UISystemGroup))]
    public partial struct StatusSystem : ISystem, ISystemStartStop
    {
        private UIHelper<StatusViewModel, StatusViewModel.Data> ui;

        public void OnStartRunning(ref SystemState state)
        {
            this.ui.Bind();
        }

        public void OnStopRunning(ref SystemState state)
        {
            this.ui.Unbind();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref var data = ref this.ui.Binding;
            data.EntityCount = state.EntityManager.UniversalQuery.CalculateEntityCountWithoutFiltering();
        }
    }
}
```

`Bind()` loads the singleton through `IViewModelService`, registers its unmanaged data with the notification bridge, calls `ILoadable.Load()` when implemented, and pins the data. `Unbind()` reverses that lifecycle and removes it from `IViewModelService`. Pair the calls exactly once and never access `Binding` outside the bound interval.

`IViewModelService` caches one instance per view-model type but does not reference-count owners. Give one helper or system ownership of each ECS-backed view-model lifecycle; two helpers that independently bind and unbind the same type can invalidate each other.

The default helper does not add an ECS query requirement. These constructors intentionally call `RequireForUpdate`:

- `UIHelper(ref state, ComponentType)` waits for the specified component, including system entities.
- `UIHelper(ref state, FixedString32Bytes)` maps a UI state name through `UISystemTypes.NameToKey` and waits for its component.

Publish data from the system's main-thread update. Do not capture the pinned binding reference in a scheduled job, and do not manipulate visual elements from ECS or Burst code.

### Special generated fields

- `Changed<T>` gets a generated `<PropertyName>Changed(out T value, bool resetToDefault = false)` consumer helper. Assigning a `T` marks the wrapper changed.
- `NativeList<T>` is exposed as `MultiContainer<T>`. The setter compares contents, copies a complete replacement into the owned list, and notifies only when contents differ.
- `ChangedList<T>` supports list state that UI and ECS both mutate and exposes `GetIfChanged` for consumption.
- `UIArray<T>` wraps an array-backed `MultiContainer<T>` as a read-only non-generic `IList` for UI controls. Its write operations throw.

Native list fields must be allocated before the generated setter is used; an uncreated destination list ignores the assignment. Implement `ILoadable` on the view model to allocate persistent lists and dispose them when the helper unbinds. Build list data in a separate system-owned buffer and assign the complete snapshot through the generated property:

```csharp
scratch.Clear();
// Populate scratch.
data.Rows = scratch;
```

Do not mutate `data.Rows` in place and then call `Notify` as a normal update path. Assignment centralizes equality checks, copying, and notifications.

## Troubleshooting

- **A generated property does not exist:** make the declaring type `partial`, use a writable field, and check generator diagnostics `BLMVVM0001` through `BLMVVM0008` or `BLSYS0001` through `BLSYS0003`.
- **UXML shows a null data source:** instantiate through `IUXMLService`, verify the `Namespace.Type, Assembly` string, and ensure the type is an Anchor service.
- **A command stays enabled:** use `CanExecuteProperty` with an observable property, and add `[DependsOn]` when the gate is computed.
- **An ECS value changes but the UI does not:** write the generated `TData` property while `UIHelper` is bound; assigning the backing field directly bypasses notifications.
- **A repeated row resolves the screen view model instead of its item:** remove the service-level `data-source-type` from the row template and let the parent control supply row data.
