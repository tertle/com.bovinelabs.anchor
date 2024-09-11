# BovineLabs Anchor
BovineLabs Anchor provides a small utility layer on top of Unity's [AppUI](https://docs.unity3d.com/Packages/com.unity.dt.app-ui@latest), along with an easy-to-use debug ribbon toolbar.

For support and discussions, join our [Discord](https://discord.gg/RTsw6Cxvw3) community.

## Installation

To install the package, add it to the Unity Package Manager using the following URL:

`https://gitlab.com/tertle/com.bovinelabs.anchor.git` 

Alternatively, you can add it directly to your manifest.json file: 

`"com.bovinelabs.anchor": "https://gitlab.com/tertle/com.bovinelabs.anchor.git",`

### Dependencies

This package depends on [AppUI](https://docs.unity3d.com/Packages/com.unity.dt.app-ui@latest).

However, due to bugs in the official AppUI package, this package currently requires a forked version that I have patched. You can install the forked version from the following git URL:

`https://github.com/tertle/com.unity.dt.app-ui.git`

These bugs have been reported and should be fixed in the pre.10 release.

Beyond this, BovineLabs Anchor has no other dependencies. However, it does offer custom support for [Entities](https://docs.unity3d.com/Packages/com.unity.entities@latest/), including the generation of per-World tabs. It also includes a binding process compatible with [Burst](https://docs.unity3d.com/Packages/com.unity.burst@latest).

Additionally, if you have my [Core](https://gitlab.com/tertle/com.bovinelabs.core) library, Unity [Physics](https://docs.unity3d.com/Packages/com.unity.physics@latest) or [Localization](https://docs.unity3d.com/Packages/com.unity.localization@latest) installed, BovineLabs Anchor will provide extra built-in tabs.

Note, if you're using Core, you currently need to use the 1.3.2-net branch.

### Setup
Setting up BovineLabs Anchor is similar to a regular AppUI application, with the key difference being that you'll use `BLUIAppBuilder` or your own inherited version instead of `UIToolkitAppBuilder`. If you're unfamiliar with AppUI, please check out its MVVM and Redux sample.

1. Add a `UIDocument` to your scene as usual.
2. Set up `PanelSettings` and apply them to your `UIDocument`.
3. In your `PanelSettings`, change the `Theme Style Sheet` field to point to the `Anchor UI` theme located at `/Packages/com.bovinelabs.anchor/BovineLabs.Anchor/Assets/Anchor UI.tss`.

   Alternatively, you can create your own `Theme Style Sheet`, making sure to import the `Anchor UI`.  
4. Add a `BLUIAppBuilder` component to your `UIDocument`.

Your inspector should look something like this:

![Inspector](Documentation~/Images/inspector.png)

## Ribbon Toolbar

![RibbonToolbar](Documentation~/Images/ribbon.png)

The Ribbon Toolbar provides easily accessible debugging tools and information within the game. It can be included in builds by defining `BL_DEBUG` in your Scripting Define Symbols.

To hide elements you don't want to see, use the menu button in the top left. These preferences are saved locally.

## Creating a Tab
There are plenty of built-in tabs you can use as references for creating your own tabs.

To automatically create a tab, use the `[AutoToolbar("NAME")]` attribute, which will cause your new tab to appear under the Service group named as NAME. For example:

```csharp
[AutoToolbar("Test")]
public class TestToolbarView : VisualElement, IView
{
    private readonly TestToolbarViewModel viewModel;

    public AppUIToolbarView(TestToolbarViewModel viewModel)
    {
        this.viewModel = viewModel;
    }

    object IView.ViewModel => this.viewModel;
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
