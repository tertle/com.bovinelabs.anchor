# BovineLabs Anchor
BovineLabs Anchor provides a utility layer on top of Unity's [AppUI](https://docs.unity3d.com/Packages/com.unity.dt.app-ui@latest), along with an easy-to-use debug ribbon toolbar and runtime workflow enhancements.

For support and discussions, join the [Discord](https://discord.gg/RTsw6Cxvw3) community.

If you want to support my work or get access to a few private libraries, [Buy Me a Coffee](https://buymeacoffee.com/bovinelabs).

## Table of Contents
- [Installation](#installation)
- [Setup](#setup)
- [MVVM Architecture](#mvvm-architecture)
- [Model Data Binding](#model-data-binding)
- [Runtime UI with Entities](#runtime-ui-with-entities)
- [Debug Ribbon Toolbar](#debug-ribbon-toolbar)

## Installation

To install the package, add it to the Unity Package Manager using the following URL:

`https://gitlab.com/tertle/com.bovinelabs.anchor.git`

Alternatively, you can add it directly to your manifest.json file:

`"com.bovinelabs.anchor": "https://gitlab.com/tertle/com.bovinelabs.anchor.git",`

### Dependencies

This package depends on [AppUI](https://docs.unity3d.com/Packages/com.unity.dt.app-ui@latest).

Beyond this, BovineLabs Anchor has no other dependencies. However, it does offer custom support for [Entities](https://docs.unity3d.com/Packages/com.unity.entities@latest/), including the generation of per-World tabs. It also includes a binding process compatible with [Burst](https://docs.unity3d.com/Packages/com.unity.burst@latest).

Additionally, if you have my [Core](https://gitlab.com/tertle/com.bovinelabs.core) library, Unity [Physics](https://docs.unity3d.com/Packages/com.unity.physics@latest) or [Localization](https://docs.unity3d.com/Packages/com.unity.localization@latest) installed, BovineLabs Anchor will provide extra built-in tabs.

## Setup

Setting up BovineLabs Anchor is similar to a regular AppUI application, with the key difference being that you'll use `AnchorAppBuilder` or your own inherited version instead of `UIToolkitAppBuilder`. If you're unfamiliar with AppUI, please check out its MVVM and Redux sample.

1. Add a `UIDocument` to your scene as usual.
2. Set up `PanelSettings` and apply them to your `UIDocument`.
3. In your `PanelSettings`, change the `Theme Style Sheet` field to point to the `Anchor UI` theme located at `/Packages/com.bovinelabs.anchor/PackageResources/Anchor UI.tss`.

   Alternatively, you can create your own `Theme Style Sheet`, making sure to import the `Anchor UI`.
4. Add a `AnchorAppBuilder` component to your `UIDocument`.

Your inspector should look something like this:

![Inspector](Documentation~/Images/inspector.png)

### Creating a Custom AppBuilder (Optional)

For more control over your application, you can create a custom `AnchorAppBuilder` that inherits from `AnchorAppBuilder<T>` where `T` is your custom `AnchorApp` class:

```csharp
public class MyAppBuilder : AnchorAppBuilder<MyApp>
{
    [SerializeField] 
    private NavGraphViewAsset navigationGraph;

    protected override NavGraphViewAsset NavigationGraph => this.navigationGraph;
    
    // Override any additional properties to customize behavior
    protected override Type ViewModelService => typeof(MyViewModelService);
}

// You often don't need to setup your own app and can just use the default AnchorApp in which case you can implement MyAppBuilder with the generic like 
// public class MyAppBuilder : AnchorAppBuilder
public class MyApp : AnchorApp
{
    public override void Initialize()
    {
        base.Initialize();
        
        // Add custom initialization logic
    }
}
```

This allows you to customize service registration, navigation, and various other aspects of the application.

## MVVM Architecture

BovineLabs Anchor extends the MVVM (Model-View-ViewModel) implementation that AppUI provides to allow for Burst-compatible data flow when used with Entities.

### View

The View is responsible for presenting the UI to the user and handling user interactions. In Anchor, Views:

- Are implemented as `VisualElement` classes that implement `View<T>` base class
- Have a reference to a ViewModel that they can bind to
- Usually use UI Toolkit data binding to stay in sync with the ViewModel
- Are created and destroyed by the system, but are not directly interacted with by the system

```csharp
public class MyView : View<MyViewModel>
{
    public MyView()
        : base(new MyViewModel())
    {
        // Create UI elements and bind to ViewModel
        var counter = new Text();
        counter.dataSource = this.ViewModel;
        counter.SetBindingToUI(nameof(Text.text), nameof(MyViewModel.Counter));
        this.Add(counter);
    }
}
```

#### VisualElement Extensions

Anchor provides a set of helpful extensions for binding to reduce boilerplate. The previous example uses the SetBindingToUI extension.
Typically, this would be implemented with the following:

```csharp
counter.SetBinding(nameof(Text.text), new DataBinding
{ 
    bindingMode = BindingMode.ToTarget,
    dataSourcePath = new PropertyPath(nameof(MyViewModel.Counter))
});
 ```

There are overloads that also support [TypeConverters](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-create-runtime-binding-type-converter.html).

### ViewModel

The ViewModel serves as the intermediary between the View and the Model (entity data):

- When not using Entities, use the AppUI source generation properties. From here out though Entities will be assumed to be used.
- When using Entities, implement `SystemObservableObject<T>` for Burst compatibility
- Properties decorated with `[SystemProperty]` attribute in the data struct get auto-generated binding code
  - Remember to mark your data struct and view model as `partial`
- Provides observable properties that the View can bind to
- Handles data conversion between Model and View-friendly formats

```csharp
public partial class MyViewModel : SystemObservableObject<MyViewModel.Data>
{
    [CreateProperty(ReadOnly = true)]
    public int Counter => this.Value.Counter;
    
    public partial struct Data
    {
        [SystemProperty]
        private int counter;
    }
}
```

## Model Data Binding

BovineLabs Anchor provides a powerful data binding system that connects your data models to the UI in a Burst-compatible way. This enables seamless integration with Unity's ECS framework.

### SystemProperty Attribute

The `SystemProperty` attribute uses source generation to create Burst-compatible property accessors in your ViewModel.
This allows systems to interact with your ViewModel's data while maintaining Burst compatibility.

For a field marked with `[SystemProperty]`, the generator creates:

```csharp
// Generated in a partial struct matching your original struct
public int Counter
{
    get => this.counter;
    set => this.SetProperty(ref counter, value);
}
```

The source generator supports common field naming conventions:
- camelCase (`counter` → `Counter`)
- _underscorePrefixed (`_counter` → `Counter`)

`SetProperty` provides the standard property change notification pattern:
- Compares current value against the new value
- If different, fires `OnPropertyChanging` event
- Updates the field value
- Fires `OnPropertyChanged` event

What makes this special is that fields in the `Data` struct can be accessed from Burst-compiled systems via the Binding reference.

### Special Field Types

The source generator also supports several special field types that provide additional functionality:

```csharp
public partial struct Data
{
    // Standard property
    [SystemProperty]
    private int counter;

    // Changed<T> - Tracks when a value has been changed
    [SystemProperty]
    private Changed<int> changedCounter;
    
    // NativeList<T> - For collections
    [SystemProperty]
    private NativeList<MyData> items;
    
    // ChangedList<T> - For collections with change tracking
    [SystemProperty]
    private ChangedList<MyData> changedItems;
}
```

For `Changed<T>` fields, additional `PropertyNameChanged` helper methods are generated to detect and reset change state:

```csharp
// Generated method for Changed<T> fields
public bool CounterChanged(out int value, bool resetToDefault = false)
{
    value = this.changedCounter.Value;
    if (this.changedCounter.HasChanged)
    {
        if (resetToDefault)
            this.Counter = new Changed<int>(default, false);
        else
            this.changedCounter = new Changed<int>(this.changedCounter.Value, false);
        return true;
    }
    return false;
}
```

### Model Patterns

There are a few common patterns for working with models in Anchor:

1. **Value Types**: Use standard value types for simple properties
2. **Changed Values**: Use `Changed<T>` to track when a value has been modified
3. **Collections**: Use `NativeList<T>` or `ChangedList<T>` for collections
4. **Events/Commands**: Use `Changed<bool>` for one-shot events from UI to system

This model binding system enables clean separation of UI representation and ECS-based logic, while maintaining high performance through Burst compatibility.

## Runtime UI with Entities

Anchor provides a powerful system for integrating UI components with ECS, allowing you to create responsive interfaces that work seamlessly with Burst-compiled systems.

### UIHelper Structure

The `UIHelper` struct is a key component for managing UI-to-ECS binding. It handles:
- Automatic viewmodel lifecycle management
- Safe access to unmanaged data from Burst-compiled systems
- Proper garbage collection and resource management

```csharp
public partial struct MySystem : ISystem, ISystemStartStop
{
    private UIHelper<MyViewModel, MyViewModel.Data> ui;

    public void OnCreate(ref SystemState state)
    {
        // For screens using navigation component
        this.ui = new UIHelper<MyViewModel, MyViewModel.Data>(ref state, "screen-name");
        
        // Alternative approach with ComponentType directly
        this.ui = new UIHelper<MyViewModel, MyViewModel.Data>(ref state, ComponentType.ReadOnly<MyScreenTag>());
    }

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
        // Access viewmodel data safely from Burst
        ref var binding = ref this.ui.Binding;
        
        // Update viewmodel data directly
        binding.Counter++;
        
        // Check and consume events from UI
        if (binding.SubmitButton.TryConsume())
        {
            // Handle button click event
        }
    }
}
```

### Binding Workflow

1. Define a view and viewmodel as described in the MVVM section
2. In your system, create a `UIHelper<TViewModel, TData>` instance
3. Call `Bind()` when the system starts running
4. Access and update the viewmodel data through `Binding` property
5. Call `Unbind()` when the system stops running

This pattern ensures resources are properly managed and provides a clean separation between UI representation and system logic.

### State Management

Anchor supports integration with navigation and state systems:

```csharp
[UpdateInGroup(typeof(UISystemGroup))]
public partial struct MyScreenSystem : ISystem, ISystemStartStop
{
    private UIHelper<MyViewModel, MyViewModel.Data> ui;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        // Register the system with a specific screen state
        StateAPI.Register<GameState, StateMyScreen, UIStates>(ref state, "my-screen");
    }

    public void OnStartRunning(ref SystemState state)
    {
        this.ui.Bind();
        
        // Initialize UI on screen load
        ref var binding = ref this.ui.Binding;
        binding.Title = "Welcome to My Screen";
    }

    public void OnStopRunning(ref SystemState state)
    {
        this.ui.Unbind();
    }
}
```

This allows you to create screen-specific systems that activate only when that screen is visible, providing efficient resource usage.

### UI Events and Commands

The `Changed<T>` type enables a command pattern for UI interactions:

```csharp
public partial struct Data
{
    [SystemProperty]
    private Changed<bool> submitButton;
}

// In the system:
ref var binding = ref this.ui.Binding;
if (binding.SubmitButton.TryConsume())
{
    // Button was clicked, handle the action
    // The TryConsume() method returns true once per click and resets the state
}
```

This pattern enables one-shot events from UI to system without the need for callbacks, making it compatible with Burst.

## Debug Ribbon Toolbar

![RibbonToolbar](Documentation~/Images/ribbon.png)

The Ribbon Toolbar provides easily accessible debugging tools and information within the game. It appears as a collapsible panel at the top of the screen, with tabs for different debugging functions.

### Using the Toolbar

- The toolbar is optionally included in **development builds** with `BL_DEBUG` defined or in the **Unity Editor**
- Click the dropdown button in the top-left to show/hide debugging categories
- Click the arrow button to collapse or expand the toolbar
- Click category tabs to switch between different debugging panels, optionally tied to Worlds
- Can be completely hidden in the configvar window

### Built-in Debug Tabs

Several debugging panels are included by default:

- **Memory**: Displays memory usage statistics
- **FPS**: Shows framerate information
- **Entities**: Displays entity, archetype, and chunk counts (if Entities package is installed)
- **Physics**: Toggle physics debug visualization (if Physics package is installed)
- **Localization**: Change locale settings (if Localization package is installed)
- **Quality**: Adjust quality settings
- **Time**: Control time scale and view elapsed time
- **UI**: Change UI theme and scale

### Creating Custom Toolbar Tabs

There are two mutually exclusive approaches to creating toolbar tabs:

#### Approach 1: Using AutoToolbar (UI-only)

Use this approach for simple debugging panels that don't need access to ECS data:

```csharp
[AutoToolbar("MyTab")]
public class MyToolbarView : View<MyToolbarViewModel>
{
    public MyToolbarView()
        : base(new MyToolbarViewModel())
    {
        // Create UI elements and bind to ViewModel
        var statusText = new Text();
        statusText.dataSource = this.ViewModel;
        statusText.SetBindingToUI(nameof(Text.text), nameof(MyToolbarViewModel.StatusText));
        this.Add(statusText);
        
        // For UI-driven functionality, use regular AppUI binding
        var button = new Button { text = "Refresh" };
        button.clicked += () => this.ViewModel.RefreshData();
        this.Add(button);
    }
}

// Regular ViewModel without Entities integration
[ObservableObject]
public partial class MyToolbarViewModel
{
    [ObservableProperty]
    private string statusText = "Ready";
    
    public void RefreshData()
    {
        this.StatusText = "Refreshed at " + DateTime.Now.ToString("HH:mm:ss");
    }
}
```

The `[AutoToolbar]` attribute automatically registers your view with the toolbar system. The first parameter is the element name displayed on the tab, and the optional second parameter is the tab group (defaults to "Service").

#### Approach 2: Using ToolbarHelper (With ECS Integration)

Use this approach when you need to update the toolbar with data from ECS systems:

```csharp
// Create a regular View without AutoToolbar attribute
[Transient] // Optional: Creates a new instance each time
public class MyEcsToolbarView : View<MyEcsToolbarViewModel>
{
    public MyEcsToolbarView()
        : base(new MyEcsToolbarViewModel())
    {
        // Create UI elements
        var entityCount = new Text();
        entityCount.dataSource = this.ViewModel;
        entityCount.SetBindingToUI(nameof(Text.text), nameof(MyEcsToolbarViewModel.EntityCount));
        this.Add(entityCount);
    }
}

// ViewModel with SystemObservableObject for ECS integration
public partial class MyEcsToolbarViewModel : SystemObservableObject<MyEcsToolbarViewModel.Data>
{
    [CreateProperty(ReadOnly = true)]
    public int EntityCount => this.Value.EntityCount;
    
    public partial struct Data
    {
        [SystemProperty]
        private int entityCount;
    }
}

// System to create and update the toolbar
[UpdateInGroup(typeof(ToolbarSystemGroup))]
public partial struct MyEcsToolbarSystem : ISystem, ISystemStartStop
{
    private ToolbarHelper<MyEcsToolbarView, MyEcsToolbarViewModel, MyEcsToolbarViewModel.Data> toolbar;

    public void OnCreate(ref SystemState state)
    {
        this.toolbar = new ToolbarHelper<MyEcsToolbarView, MyEcsToolbarViewModel, MyEcsToolbarViewModel.Data>(ref state, "MyEcsTab");
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
        // Only update when tab is visible
        if (!this.toolbar.IsVisible())
        {
            return;
        }

        // Update toolbar data from ECS
        ref var data = ref this.toolbar.Binding;
        data.EntityCount = state.EntityManager.UniversalQuery.CalculateEntityCountWithoutFiltering();
    }
}
```

The `ToolbarHelper` approach uses the same model binding system as regular screens but integrates specially with the toolbar UI. The data binding works the same way with `[SystemProperty]` attributes and source generated binding code, allowing you to create rich debugging tools that can display real-time ECS data.