# BovineLabs Anchor
BovineLabs Anchor provides a utility layer on top of Unity's [AppUI](https://docs.unity3d.com/Packages/com.unity.dt.app-ui@latest), along with an easy-to-use debug ribbon toolbar.

For support and discussions, join the [Discord](https://discord.gg/RTsw6Cxvw3) community.

If you want to support my work or get access to a few private libraries, [Buy Me a Coffee](https://buymeacoffee.com/bovinelabs).

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

## MVVM

BovineLabs Anchor extends the MVVM (Model-View-ViewModel) implementation that AppUI provides to allow for Burst-compatible data flow when used with Entities.

### View

The View is responsible for presenting the UI to the user and handling user interactions. In Anchor, Views:

- Are implemented as `VisualElement` classes that implement `IView<T>` interface
- Have a reference to a ViewModel that they can bind to
- Usually use UI Toolkit data binding to stay in sync with the ViewModel
- Are created and destroyed by the system, but are not directly interacted with by the system

```csharp
public class MyToolbarView : VisualElement, IView<MyToolbarViewModel>
{
    public MyToolbarView()
    {
        // Create UI elements and bind to ViewModel
        var counter = new Text();
        counter.dataSource = this.ViewModel;
        counter.SetBindingToUI(nameof(Text.text), nameof(MyToolbarViewModel.Counter));
        this.Add(counter);
    }
    
    // ViewModel can be injected if uses services but generally you just create it directly
    public MyToolbarViewModel ViewModel { get; } = new();
}
```

#### VisualElement Extensions

Anchor provides a set of helpful extensions for binding to reduce boilerplate. The previous example uses the SetBindingToUI extension.
Typically, this would be implemented with the following:

```csharp
counter.SetBinding(nameof(Text.text), new DataBinding
{ 
    bindingMode = BindingMode.ToTarget,
    dataSourcePath = new PropertyPath(nameof(MyToolbarViewModel.Counter))
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
public partial class MyToolbarViewModel : SystemObservableObject<MyToolbarViewModel.Data>
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

#### SystemProperty Attribute

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

#### Special Field Types
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

### Model and Entities

In the context of Anchor with Entities:

- **ECS Components** represent the actual data model of your application
- **Systems** contain the business logic that operates on that data
- The `ViewModel.Data` struct serves as a bridge between burst-compiled systems and the ViewModel
- Systems create, manage, and update UI elements through the ViewModel, never interacting directly with Views

An example of it for a Toolbar is shown, but it's the same concept for normal game ui except you use the UIHelper struct instead.

```csharp
[UpdateInGroup(typeof(ToolbarSystemGroup))]
public partial struct MyToolbarSystem : ISystem, ISystemStartStop
{
    private ToolbarHelper<MyToolbarView, MyToolbarViewModel, MyToolbarViewModel.Data> toolbar;
    
    public void OnCreate(ref SystemState state)
    {
        this.toolbar = new ToolbarHelper<MyToolbarView, MyToolbarViewModel, MyToolbarViewModel.Data>(ref state, "MyTab");
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
        data.counter++; // Access ViewModel data directly
    }
}
```

### Data Flow

1. **User Input → View → ViewModel**: When users interact with UI elements, changes are propagated through data binding to the ViewModel
2. **ViewModel → System**: The ViewModel's state can influence system behavior and component data
3. **System → ViewModel → View**: Systems update ViewModel.Data, which automatically refreshes the bound UI elements

This pattern ensures a clean separation of concerns while supporting Burst compatibility when used with Entities.

### TODO THE FOLLOWING IS FROM ORIGINAL MANUAL AND HAS NOT BEEN UPDATED AND WILL BE REVIEWED SOON, it most mostly correct though

## Ribbon Toolbar

![RibbonToolbar](Documentation~/Images/ribbon.png)

The Ribbon Toolbar provides easily accessible debugging tools and information within the game. It can be included in builds by defining `BL_DEBUG` in your Scripting Define Symbols.

To hide elements you don't want to see, use the menu button in the top left. These preferences are saved locally.

## Creating a Tab
There are plenty of built-in tabs you can use as references for creating your own tabs.

To automatically create a tab, use the `[AutoToolbar("NAME")]` attribute, which will cause your new tab to appear under the Service group named as NAME. For example:

```csharp
[AutoToolbar("Test")]
public class TestToolbarView : VisualElement, IView<TestToolbarViewModel>
{
    public AppUIToolbarView(TestToolbarViewModel viewModel)
    {
        this.ViewModel = viewModel;
    }
    
    public TestToolbarViewModel ViewModel { get; }
}
```
If you want to inject the ViewModel as shown above, it needs to inherit from `IViewModel`. This step is optional and is typically done if you need to inject a service into the ViewModel. Otherwise, you can instantiate it directly within the View.

To integrate with Entity Worlds, use `ToolbarHelper<View, ViewModel, ViewModel.UnmanagedBinding>`. For example:

```csharp
[UpdateInGroup(typeof(ToolbarSystemGroup))]
internal partial struct TestToolbarSystem : ISystem, ISystemStartStop
{
    private ToolbarHelper<TestToolbarView, TestToolbarViewModel, TestToolbarViewModel.Data> toolbar;

    public void OnCreate(ref SystemState state)
    {
        this.toolbar = new ToolbarHelper<TestToolbarView, TestToolbarViewModel, TestToolbarViewModel.Data>(ref state, "Test");
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
        /// ...
    }
}
```

### Game UI
To use AppUI by adding another root element for your game, simply inherit from `IViewRoot`. If you need multiple roots, you can order them with `Priority`. Note that the toolbar view uses a priority of -1000 to appear at the top of the screen.

```csharp
[Preserve]
public class GameView : VisualElement, IViewRoot
{
    public GameView()
    {
        this.style.flexGrow = 1;

        var content = new Preloader();
        content.StretchToParentSize();

        this.Add(content);
    }

    public int Priority => 0;

    object IView.ViewModel => null;
}
```
From here, you can build your UI using the regular AppUI flow, including navigation, injection, or whatever approach suits your needs.
