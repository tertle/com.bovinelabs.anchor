# Debug toolbar

`BovineLabs.Anchor.Debug` provides a ribbon-style host for compact diagnostics and controls. It supports managed panels discovered by attribute and ECS-backed panels whose unmanaged state is bridged through `ToolbarHelper`.

![Anchor debug toolbar ribbon](Images/ribbon.png)

The toolbar is an AppUI feature. Its package asmdef requires:

- `UNITY_APPUI`.
- `UNITY_EDITOR || BL_DEBUG`, so player builds must explicitly define `BL_DEBUG`.
- Runtime AppUI support when building a player, rather than `APP_UI_EDITOR_ONLY`.

`BovineLabs.Anchor.Debug` is not auto-referenced. Put custom panels in a debug asmdef that explicitly references `BovineLabs.Anchor`, `BovineLabs.Anchor.Debug`, `Unity.AppUI`, `Unity.Entities` for ECS panels, and any feature assemblies whose data the panel reads. Mirror the `UNITY_EDITOR || BL_DEBUG` constraint so debug code does not leak into normal player builds.

Anchor's default app builder discovers the non-transient `ToolbarView` service as `IAnchorToolbarHost` and calls `AnchorApp.InitializeToolbar()`. Use the `Anchor UI.tss` theme entry point so the AppUI and toolbar style sheets are present. See [Getting started](getting-started.md) for host and theme setup.

## Choose a panel pattern

| Panel state | Pattern |
| --- | --- |
| Managed Unity API, service state, or static commands | A `View<TViewModel>` marked `[AutoToolbar]` |
| ECS world data or controls consumed by an ECS system | `ToolbarHelper<TView, TViewModel, TData>` in an `ISystem, ISystemStartStop` |

Do not introduce an ECS system solely to host a managed panel. Conversely, do not poll ECS state from a visual element when a toolbar system can publish an unmanaged snapshot.

## Managed panels with AutoToolbar

`[AutoToolbar(elementName, tabName)]` is discovered when `ToolbarView` is constructed. `elementName` is the heading and filter name. When `tabName` is omitted, the panel uses `AnchorApp.ServiceTabName`, which defaults to `Service`.

```csharp
namespace Example.Debug
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.MVVM;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.Scripting;

    public sealed class BuildInfoViewModel : ObservableObject
    {
        private string version;

        [CreateProperty(ReadOnly = true)]
        public string Version
        {
            get => this.version;
            set => this.SetProperty(ref this.version, value);
        }
    }

    [Preserve]
    [AutoToolbar("Build", "Diagnostics")]
    public sealed class BuildInfoView : View<BuildInfoViewModel>
    {
        [Preserve]
        public BuildInfoView()
            : base(new BuildInfoViewModel())
        {
            var version = new Text { dataSource = this.ViewModel };
            version.SetBindingToUI(nameof(Text.text), nameof(BuildInfoViewModel.Version));
            this.Add(version);

            this.schedule.Execute(() => this.ViewModel.Version = UnityEngine.Application.version).Every(250);
        }
    }
}
```

`View<T>` inherits `[IsService]`, which satisfies `ToolbarView.AddTab`. Add `[IsService]` yourself if an auto panel derives directly from `VisualElement`. Auto panels are normally resolved once with the toolbar, so `[Transient]` is unnecessary unless another caller also creates them and needs independent instances. Keep `[Preserve]` on attribute-discovered panels and their reflected constructors for stripped player builds.

Managed polling should be modest. `ToolbarView.UpdateRateSeconds` is `0.25` seconds and is a useful default for counters that do not need per-frame refresh.

## ECS-backed panels

Use three types with separate responsibilities:

- `*ToolbarSystem`: reads or writes ECS data and publishes raw unmanaged values.
- `*ToolbarViewModel`: exposes bindable managed presentation over `SystemObservableObject<TData>`.
- `*ToolbarView`: constructs compact AppUI controls and bindings.

### 1. System

```csharp
namespace Example.Debug
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    public partial struct EntityCountToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<
            EntityCountToolbarView,
            EntityCountToolbarViewModel,
            EntityCountToolbarViewModel.Data> toolbar;

        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<
                EntityCountToolbarView,
                EntityCountToolbarViewModel,
                EntityCountToolbarViewModel.Data>(ref state, "Entities");
        }

        public void OnStartRunning(ref SystemState state)
        {
            this.toolbar.Load();
        }

        public void OnStopRunning(ref SystemState state)
        {
            this.toolbar.Unload();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (!this.toolbar.IsVisible())
            {
                return;
            }

            ref var data = ref this.toolbar.Binding;
            data.EntityCount = state.EntityManager.UniversalQuery.CalculateEntityCountWithoutFiltering();
        }
    }
}
```

`ToolbarSystemGroup` runs inside Core's `DebugSystemGroup` for default and service worlds. It skips child updates until both `ToolbarView.Instance` and `AnchorApp.Current` exist.

The `ref state` constructor uses the system world's name as the toolbar tab and the second argument as the group heading and filter name. A world name ending in `World` has that suffix removed. Use the `(FixedString32Bytes tabName, FixedString32Bytes groupName)` constructor when the tab should not be world-scoped.

`IsVisible()` compares only the active toolbar tab with the helper's tab. It does not test whether the ribbon is expanded. Use it before expensive queries, but apply a separate throttle when collapsed-ribbon cost matters. Panels in different groups on the same active tab all report visible, because the ribbon presents the tab's visible groups together.

### 2. View model

```csharp
namespace Example.Debug
{
    using BovineLabs.Anchor;
    using Unity.Properties;

    public partial class EntityCountToolbarViewModel :
        SystemObservableObject<EntityCountToolbarViewModel.Data>
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

Keep `Data` unmanaged and use `[SystemProperty]` instead of public fields when bindings need notifications. Presentation stays managed: format strings, resolve textures, and localize labels in the view model or view, not in the Burst system.

For a control that writes back into `Data`, expose a normal bindable setter through the generated data property:

```csharp
[CreateProperty]
public bool Enabled
{
    get => this.Value.Enabled;
    set => this.Value.Enabled = value;
}
```

The ECS system can read the updated value on its next update.

### 3. View

```csharp
namespace Example.Debug
{
    using BovineLabs.Anchor;
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;

    [Preserve]
    [Transient]
    public sealed class EntityCountToolbarView : View<EntityCountToolbarViewModel>
    {
        [Preserve]
        public EntityCountToolbarView()
            : base(new EntityCountToolbarViewModel())
        {
            var count = new Text { dataSource = this.ViewModel };
            count.SetBindingToUI(nameof(Text.text), nameof(EntityCountToolbarViewModel.EntityCount));
            this.Add(count);
        }
    }
}
```

Mark ECS-backed views `[Transient]`. Each world creates its own helper and must receive an independent view and view model; a singleton visual element cannot belong to multiple toolbar containers.

Keep panels compact. Prefer AppUI `Text` for values, `Toggle` for booleans, `ActionButton` for commands, and short row or column layouts. `KeyValueGroup.Create` is useful for a few aligned readouts; see [Adapter elements](adapter-elements.md).

## ToolbarHelper lifecycle

`Load()` performs these operations:

1. Adds the view service to `ToolbarView` and records the returned tab id.
2. Registers the view model's unmanaged value with the Burst notification bridge.
3. Calls `ILoadable.Load()` on the view model when implemented.
4. Pins the unmanaged value for direct access through `Binding`.
5. Restores serializable panel state when enabled.

`Unload()` removes the tab, persists state, calls `ILoadable.Unload()`, unregisters the binding bridge, and releases the pin. Pair `Load()` and `Unload()` in `OnStartRunning` and `OnStopRunning`; never use `Binding` before load or after unload.

Removing a tab also calls `Dispose()` when the view implements `IDisposable`. Use that to unsubscribe view-owned managed events.

## Persist panel settings

`ToolbarHelper` enables persistence when the view-model class has `[Serializable]`. Unity's `JsonUtility` writes the panel to PlayerPrefs under `bl.toolbar.<tab>.<group>`.

```csharp
[Serializable]
public partial class PhysicsToolbarViewModel :
    SystemObservableObject<PhysicsToolbarViewModel.Data>
{
    [CreateProperty]
    public bool DrawColliders
    {
        get => this.Value.DrawColliders;
        set => this.Value.DrawColliders = value;
    }

    [Serializable]
    public partial struct Data
    {
        [SerializeField]
        [SystemProperty]
        private bool drawColliders;
    }
}
```

Mark both the view model and nested data struct `[Serializable]`, and put `[SerializeField]` on private generated fields that must persist. Persistence is for small settings, not sampled counters or native collections.

## Native collection data

`[SystemProperty] NativeList<T>` generates a `MultiContainer<T>` property. The destination list belongs to the view model, so it must be allocated and disposed through `ILoadable`:

```csharp
using System;
using BovineLabs.Anchor;
using Unity.Collections;
using Unity.Properties;

public partial class RowsToolbarViewModel :
    SystemObservableObject<RowsToolbarViewModel.Data>, ILoadable
{
    [CreateProperty(ReadOnly = true)]
    public UIArray<Data.Row> Rows => this.Value.Rows;

    public void Load() => this.Value.Initialize();

    public void Unload() => this.Value.Dispose();

    public partial struct Data
    {
        [SystemProperty]
        private NativeList<Row> rows;

        internal void Initialize()
        {
            this.rows = new NativeList<Row>(Allocator.Persistent);
        }

        internal void Dispose()
        {
            if (this.rows.IsCreated)
            {
                this.rows.Dispose();
            }
        }

        public struct Row : IEquatable<Row>
        {
            public int Id;

            public bool Equals(Row other) => this.Id == other.Id;
        }
    }
}
```

Build a complete snapshot in a separate, system-owned `NativeList<T>` and assign it through the generated property:

```csharp
this.scratch.Clear();
// Populate scratch.
data.Rows = this.scratch;
```

The setter compares contents, copies into the view model's owned list, and notifies only on a real change. An uncreated destination list ignores assignment. Do not mutate the owned list through its getter and manually call `Notify` as the normal update path.

See [MVVM and data binding](mvvm-and-binding.md) for `Changed<T>`, `ChangedList<T>`, `UIArray<T>`, generator constraints, and the underlying binding lifecycle.

## Filtering and saved UI state

Toolbar groups are sorted alphabetically within a tab. The filter menu tracks group names with reference counts, so several worlds can publish a group with the same name without duplicating the filter entry. Hidden group names are stored through `ILocalStorageService` under `bl.toolbarmanager.filter.selections`.

The toolbar also persists its active tab and ribbon visibility. The `anchor.toolbar` ConfigVar controls whether the toolbar starts enabled. These host preferences are independent of per-panel `ToolbarHelper` settings.

## Guardrails

- Keep runtime data collection in the system and managed presentation in the view model or view.
- Do not call managed string formatting, localization, asset lookup, or visual-element APIs from a Burst path.
- Do not capture `ToolbarHelper.Binding` in scheduled jobs. The pointer is tied to the panel lifecycle and binding notifications are managed UI work.
- Avoid structural changes, `Run()`, `Complete()`, and broad main-thread entity loops just to populate diagnostics.
- Add an ECS query requirement only when the panel genuinely cannot operate without that optional feature.
- Do not use unfinished, commented-out toolbar code as a template. The built-in Entities and Physics panels demonstrate the supported lifecycle.
- Use the project's logging policy rather than `Debug.Log*` when a diagnostic panel itself needs logging.

## Troubleshooting

- **The toolbar is absent:** verify AppUI, the debug compile constraints, the `BovineLabs.Anchor.Debug` asmdef reference, Anchor app initialization, and the Anchor UI theme.
- **`AddTab` says the view is not a service:** derive from `View<T>` or add `[IsService]` to a direct `VisualElement` subclass.
- **Two worlds show the same view:** mark the ECS-backed view `[Transient]`.
- **The system runs expensive queries while hidden:** guard the work with `toolbar.IsVisible()` before collecting data.
- **A restored setting is missing:** mark the class and nested data `[Serializable]`, and serialize the private field with `[SerializeField]`.
- **A native list never updates:** allocate the destination list in `ILoadable.Load()` and assign a separate snapshot through the generated property.
