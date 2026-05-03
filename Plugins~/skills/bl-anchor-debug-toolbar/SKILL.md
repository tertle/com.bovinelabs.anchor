---
name: bl-anchor-debug-toolbar
description: "Use when creating, extending, or reviewing Anchor debug toolbar elements in projects or packages that use Anchor, including ToolbarHelper systems, SystemObservableObject view models, View-based UI panels, AutoToolbar managed panels, and BovineLabs.Anchor.Debug asmdef wiring."
---

# Anchor Debug Toolbar

Use this skill for debug panels hosted by `BovineLabs.Anchor.Debug.Toolbar.ToolbarView`.
Resolve Anchor package paths against `Packages/com.bovinelabs.anchor` or the matching `Library/PackageCache/com.bovinelabs.anchor@*`, and prefer the current Anchor Debug examples over unfinished feature-specific toolbar code.

## Workflow

1. Inspect the target debug assembly and its `.asmdef` first. It usually needs `BovineLabs.Anchor`, `BovineLabs.Anchor.Debug`, `Unity.AppUI`, `Unity.Entities`, and any package-specific runtime/data assemblies it reads.
2. Use the three-file ECS pattern for world-backed panels: `*ToolbarSystem`, `*ToolbarViewModel`, and `*ToolbarView`.
3. Put the system in `ToolbarSystemGroup` and implement `ISystem, ISystemStartStop`.
4. Create a `ToolbarHelper<TView, TViewModel, TData>` field in `OnCreate`, call `Load()` in `OnStartRunning`, and call `Unload()` in `OnStopRunning`.
5. In `OnUpdate`, return immediately when `!toolbar.IsVisible()`, then write only raw unmanaged state through `ref var data = ref toolbar.Binding`.
6. Keep formatting, texture lookup, localization, and other managed UI work in the view model or view. Burst/system code should publish data, not build UI strings through managed APIs.
7. Use AppUI/UI Toolkit bindings in the view. Set `dataSource = this.ViewModel`, then bind with `SetBindingToUI`, `SetBindingFromUI`, `SetBindingTwoWay`, or explicit `DataBinding` when a converter is needed.
8. Use the MVVM/binding and adapter-elements skills for non-toolbar screen binding or AppUI control details.
9. Run the affected Unity test assembly when code changes are made; for documentation-only skill edits, validation with `quick_validate.py` is enough.

## ECS Pattern

System shape:

```csharp
namespace BovineLabs.Example.Debug
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using Unity.Burst;
    using Unity.Entities;

    [UpdateInGroup(typeof(ToolbarSystemGroup))]
    public partial struct ExampleToolbarSystem : ISystem, ISystemStartStop
    {
        private ToolbarHelper<ExampleToolbarView, ExampleToolbarViewModel, ExampleToolbarViewModel.Data> toolbar;

        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<ExampleToolbarView, ExampleToolbarViewModel, ExampleToolbarViewModel.Data>(ref state, "Example");
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

The `ref state` constructor uses the world name as the toolbar tab and the string argument as the group/filter name. Use the `(tabName, groupName)` constructor only when the tab should not be world-scoped.

## View Models

Use `SystemObservableObject<TData>` when an ECS system needs pinned unmanaged binding data.

```csharp
namespace BovineLabs.Example.Debug
{
    using BovineLabs.Anchor;
    using Unity.Properties;

    public partial class ExampleToolbarViewModel : SystemObservableObject<ExampleToolbarViewModel.Data>
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

Use `[SystemProperty]` on private fields in a `partial struct Data` when bindings need generated setters, property-change notifications, or Burst-written values. For persisted debug settings, mark the view model and data `[Serializable]`, and add `[SerializeField]` to the generated fields; `ToolbarHelper` saves serializable view models through `PlayerPrefs`.

For UI controls that write back to ECS state, expose a normal `[CreateProperty]` setter that updates `Value` with `SetProperty`. For display-only data, expose read-only properties that format raw unmanaged values from `Value`.

## Views

Derive from `View<TViewModel>` and construct the view model inline. `View<T>` is already an Anchor service; add `[Transient]` when the panel should be resolved as a fresh transient instance like the built-in debug views.

```csharp
namespace BovineLabs.Example.Debug
{
    using BovineLabs.Anchor;
    using Unity.AppUI.UI;

    public class ExampleToolbarView : View<ExampleToolbarViewModel>
    {
        public ExampleToolbarView()
            : base(new ExampleToolbarViewModel())
        {
            var count = new Text { dataSource = this.ViewModel };
            count.SetBindingToUI(nameof(Text.text), nameof(ExampleToolbarViewModel.EntityCount));
            this.Add(count);
        }
    }
}
```

Prefer AppUI elements for controls: `Text` for compact values, `Toggle` for booleans, `ActionButton` for commands, and small row/column `VisualElement` layouts for grouping. Keep toolbar panels compact; they live in a ribbon, not a full screen.

## Managed-Only Panels

Use `[AutoToolbar("Panel Name")]` on a `View<TViewModel>` when the panel is purely managed and does not need an ECS system. This is the pattern for panels that poll Unity managed APIs through `schedule.Execute(...)` or show service/editor state. Do not add `ToolbarHelper` just to host a static or managed-only panel.

## Guardrails

- Do not create a new toolbar host, nav path, popup, or custom debug UI framework for panels that fit Anchor Debug.
- Do not use unfinished feature-specific toolbar implementations as templates. Start from Anchor Debug built-ins or stable package debug panels.
- Do not put managed strings, UnityEngine object lookups, localization resolution, or texture polling in Burst paths. Publish unmanaged data from systems and resolve managed presentation in view models/views.
- Do not add `RequireForUpdate` by default. Use it only when the debug system genuinely has an optional runtime dependency.
- Do not add structural changes, `Complete()`, `Run()`, or main-thread ECS loops in debug toolbar systems unless the specific debug feature already requires it and the tradeoff is documented.
- Do not use `Debug.Log*`; use the active project or package logging policy when logging is necessary.
