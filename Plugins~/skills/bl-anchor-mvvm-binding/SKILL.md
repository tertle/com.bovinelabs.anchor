---
name: bl-anchor-mvvm-binding
description: "Use when authoring, wiring, refactoring, or debugging com.bovinelabs.anchor MVVM and binding code, including View models, View elements, ObservableObject source generation, RelayCommand and generated ICommand properties, UXML data-source-type binding, SystemObservableObject data, UIHelper, SystemProperty fields, and Burst-to-UI change notifications."
---

# Anchor MVVM Binding

Use this skill for Anchor view models, generated binding members, UXML data-source wiring, and ECS-to-UI data flow. Resolve Anchor source from the installed package root; in source checkouts this is commonly `Packages/com.bovinelabs.anchor`.

## Workflow

1. Decide whether the UI state is managed-only or ECS-backed.
2. For managed-only screens, use `ObservableObject`, `[ObservableProperty]`, `[ICommand]`, `RelayCommand`, and normal UI Toolkit/AppUI bindings.
3. For ECS-backed screens, use `SystemObservableObject<TData>` and bind/unbind it with `UIHelper<TViewModel, TData>` or `ToolbarHelper` in an `ISystemStartStop` lifecycle.
4. Put formatting, localization, UnityEngine object lookups, and string presentation in the view model or view, not in Burst/system code.
5. For UXML screens, declare `data-source-type` only on elements that should resolve their own service-backed view model.
6. Use the adapter-elements skill for `AnchorGridView`, `AnchorAccordion`, `OptionPager`, `ClassBinding`, and other AppUI-specific controls.

## Managed View Models

```csharp
namespace Example.UI
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;

    [IsService]
    public partial class MenuViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ICommand]
        private void Play()
        {
            AnchorApp.Current.NavHost.Navigate("game");
        }
    }
}
```

- Types using `[ObservableProperty]` or `[ICommand]` must be `partial`.
- `[ObservableProperty]` fields must be non-static, non-const, and non-readonly.
- Generated property names strip `_`, `m_`, and `m` prefixes before Pascal-casing.
- `[AlsoNotifyChangeFor]` raises additional property notifications.
- `[AlsoExecute]` calls a non-generic parameterless method after the generated property changes.
- `[ICommand]` methods must return `void` and have at most one non-ref parameter.
- `[ICommand(CanExecuteMethod = "...")]` must point at a `bool` method with matching parameters.
- `[ICommand(CanExecuteProperty = "...")]` must point at a `bool` property with a getter; use either `CanExecuteMethod` or `CanExecuteProperty`, not both.

## Views And UXML

- `View<TViewModel>` is a `VisualElement` with a strongly typed `ViewModel`.
- Mark service-backed views or view models with `[IsService]`; add `[Transient]` when every resolution should create a new instance.
- `UXMLService.Instantiate` sets `dataSource` to `AnchorApp.Current.Services.GetService(dataSourceType)` for elements with `data-source-type`.
- In C# views, set `dataSource = this.ViewModel` and bind with UI Toolkit `DataBinding`, `SetBindingToUI`, `SetBindingFromUI`, or `SetBindingTwoWay`.
- Use `Converters.RegisterConverters` groups by name when binding bools to `DisplayStyle`, inverted `DisplayStyle`, or inverted booleans.

## ECS-Backed Binding

```csharp
namespace Example.UI
{
    using BovineLabs.Anchor;
    using Unity.Properties;

    [IsService]
    public partial class StatusViewModel : SystemObservableObject<StatusViewModel.Data>
    {
        [CreateProperty(ReadOnly = true)]
        public int Count => this.Value.Count;

        public partial struct Data
        {
            [SystemProperty]
            private int count;
        }
    }
}
```

- `SystemObservableObject<TData>` stores unmanaged `TData` and implements binding notifications for Burst-updated data.
- `TData` must be unmanaged.
- `[SystemProperty]` fields must be inside a `partial struct`, must declare exactly one field, and must not be static, const, or readonly.
- Generated system properties call `SetProperty`, so UI notifications stay centralized.
- `Changed<T>` fields get a generated `PropertyChanged(out value, resetToDefault)` helper.
- `NativeList<T>` fields are exposed as generated `MultiContainer<T>` properties for UI binding. Build a complete replacement `NativeList<T>` and assign the generated property (`data.Items = items`) so the binding layer performs content diffing, copies into the owned list, and emits change notifications.
- Use `UIArray<T>` when a read-only native array needs to be exposed as an `IList`.

## UIHelper Lifecycle

```csharp
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

    public void OnUpdate(ref SystemState state)
    {
        ref var data = ref this.ui.Binding;
        data.Count = 42;
    }
}
```

- `Bind()` loads the view model through `IViewModelService`, calls `Load()`, calls `ILoadable.Load()` when implemented, pins the data, and registers change notification interop.
- `Unbind()` unloads in reverse and frees the pin.
- The `UIHelper(ref state, ComponentType)` and `UIHelper(ref state, FixedString32Bytes)` constructors intentionally add a system update requirement; use them only when that requirement is wanted.

## Guardrails

- Do not put managed string formatting or Unity object work in Burst paths.
- Do not mutate visual elements from ECS jobs; publish unmanaged state and let bindings/view models present it.
- Do not add `data-source-type` to item templates whose parent control sets row data.
- Do not bypass `SetProperty` when changing observable fields that the UI binds to.
