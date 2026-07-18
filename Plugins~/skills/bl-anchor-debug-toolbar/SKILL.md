---
name: bl-anchor-debug-toolbar
description: "Use for Anchor debug-toolbar panels backed by ECS systems, ToolbarHelper, SystemObservableObject, managed AutoToolbar panels, or debug asmdefs."
---

# Anchor Debug Toolbar

Use this skill for debug panels registered with the durable `BovineLabs.Anchor.Debug.Toolbar.Toolbar` service.
Resolve Anchor package paths against `Packages/com.bovinelabs.anchor` or the matching `Library/PackageCache/com.bovinelabs.anchor@*`, and prefer the current Anchor Debug examples over unfinished feature-specific toolbar code.

## Workflow

1. Inspect the target debug assembly and its `.asmdef` first. It usually needs `BovineLabs.Anchor`, `BovineLabs.Anchor.Debug`, `Unity.AppUI`, `Unity.Entities`, and any package-specific runtime/data assemblies it reads.
2. Use the three-file ECS pattern for world-backed panels: `*ToolbarSystem`, a model that implements `IToolbarElement`, and a plain `VisualElement` projection.
3. Put the system in `ToolbarSystemGroup` and implement `ISystem, ISystemStartStop`.
4. Create a `ToolbarHelper<TModel, TData>` field in `OnCreate`, call `Load()` in `OnStartRunning`, and call `Unload()` in `OnStopRunning`.
5. In `OnUpdate`, return immediately when `!toolbar.IsVisible()`, then write only raw unmanaged state through `ref var data = ref toolbar.Binding`.
6. For generated `NativeList<T>` / `MultiContainer<T>` bindings, build the complete replacement list and assign it through the generated property (`data.Items = nativeList`) so the binding layer handles content diffing and notifications. Do not mutate the backing list and call `Notify` manually.
7. Keep formatting, texture lookup, localization, and other managed UI work in the view model or view. Burst/system code should publish data, not build UI strings through managed APIs.
8. Have `IToolbarElement.CreateElement()` return a fresh view on every call. Pass the model into the view constructor, set the view's `dataSource`, then bind with `SetBindingToUI`, `SetBindingFromUI`, `SetBindingTwoWay`, or explicit `DataBinding`.
9. Use the MVVM/binding and adapter-elements skills for non-toolbar screen binding or AppUI control details.
10. Run the affected Unity test assembly when code changes are made; for documentation-only edits, validate links and examples against the current source.

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
        private ToolbarHelper<ExampleToolbarViewModel, ExampleToolbarViewModel.Data> toolbar;

        public void OnCreate(ref SystemState state)
        {
            this.toolbar = new ToolbarHelper<ExampleToolbarViewModel, ExampleToolbarViewModel.Data>(ref state, "Example");
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
    using BovineLabs.Anchor.Debug.Toolbar;
    using Unity.Properties;
    using UnityEngine.UIElements;

    public partial class ExampleToolbarViewModel : SystemObservableObject<ExampleToolbarViewModel.Data>, IToolbarElement
    {
        [CreateProperty(ReadOnly = true)]
        public int EntityCount => this.Value.EntityCount;

        public partial struct Data
        {
            [SystemProperty]
            private int entityCount;
        }

        public VisualElement CreateElement()
        {
            return new ExampleToolbarView(this);
        }
    }
}
```

Use `[SystemProperty]` on private fields in a `partial struct Data` when bindings need generated setters, property-change notifications, or Burst-written values. For persisted debug settings, mark the view model and data `[Serializable]`, and add `[SerializeField]` to the generated fields; `ToolbarHelper` saves serializable view models through `PlayerPrefs`.

For list-backed bindings, `[SystemProperty] private NativeList<T> items;` generates a `MultiContainer<T>` property. Systems should prepare a full `NativeList<T>` snapshot and assign `data.Items = items`; the generated setter copies, diffs, and notifies. Keep `Data` methods minimal and prefer direct generated property writes for toolbar state.

For UI controls that write back to ECS state, expose a normal `[CreateProperty]` setter that assigns through the generated data property, for example `this.Value.Enabled = value`; that generated setter performs `SetProperty`. For display-only data, expose read-only properties that format raw unmanaged values from `Value`.

## Views

Toolbar views are replaceable projections, not services or state owners. Derive from `VisualElement`, accept the durable model in the constructor, and set the root `dataSource`. The toolbar may call `CreateElement()` again when its root is rebuilt, so never return a cached or previously attached element.

```csharp
namespace BovineLabs.Example.Debug
{
    using BovineLabs.Anchor;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    public class ExampleToolbarView : VisualElement
    {
        public ExampleToolbarView(ExampleToolbarViewModel model)
        {
            this.dataSource = model;

            var count = new Text();
            count.SetBindingToUI(nameof(Text.text), nameof(ExampleToolbarViewModel.EntityCount));
            this.Add(count);
        }
    }
}
```

Prefer AppUI elements for controls: `Text` for compact values, `Toggle` for booleans, `ActionButton` for commands, and small row/column `VisualElement` layouts for grouping. Keep toolbar panels compact; they live in a ribbon, not a full screen.

If a view directly subscribes to managed events, opens popups, or owns other disposable visual resources, implement `IDisposable`. The toolbar disposes every materialized element when that visual generation is replaced or the registration is removed. Keep model lifecycle work out of the view.

## Managed-Only Panels

Put `[IsService]` and `[AutoToolbar("Panel Name")]` on the model when a panel is purely managed and does not need an ECS system. The model must implement `IToolbarElement`; `CreateElement()` returns a fresh plain view. Add `ILoadable` when the model owns subscriptions or resources that should exist for the registration lifetime. `Load()` runs once when the durable toolbar discovers the model, `Unload()` runs once at true toolbar shutdown, and visual recreation does not repeat either call.

Do not add `ToolbarHelper` just to host a static or managed-only panel. Keep `[Preserve]` on attribute-discovered models and constructors that are reached only through reflection in stripped builds.

## Guardrails

- Do not create a new toolbar host, nav path, popup, or custom debug UI framework for panels that fit Anchor Debug.
- Do not put `[AutoToolbar]`, `[IsService]`, or `[Transient]` on the visual projection; the model owns registration identity and state.
- Do not cache or reuse toolbar visual elements. `IToolbarElement.CreateElement()` must create a fresh element for each toolbar root generation.
- Do not use unfinished feature-specific toolbar implementations as templates. Start from Anchor Debug built-ins or stable package debug panels.
- Do not put managed strings, UnityEngine object lookups, localization resolution, or texture polling in Burst paths. Publish unmanaged data from systems and resolve managed presentation in view models/views.
- Do not add `RequireForUpdate` by default. Use it only when the debug system genuinely has an optional runtime dependency.
- Do not add structural changes, `Complete()`, `Run()`, or main-thread ECS loops in debug toolbar systems unless the specific debug feature already requires it and the tradeoff is documented.
- Do not use `Debug.Log*`; use the active project or package logging policy when logging is necessary.
