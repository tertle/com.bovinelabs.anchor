// <copyright file="ToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Core.ConfigVars;
    using BovineLabs.Core.Utility;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.UI;
    using Unity.Burst;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;
    using Canvas = UnityEngine.Canvas;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Ribbon-style toolbar that surfaces debug and service panels within the Anchor app.
    /// </summary>
    [Configurable]
    [IsService]
    public class ToolbarView : VisualElement
    {
        /// <summary>Default polling interval in seconds for toolbar updates.</summary>
        public const float DefaultUpdateRate = 1 / 4f;

        /// <summary>
        /// The NavigationScreen main styling class.
        /// </summary>
        private const string UssClassName = "bl-toolbar";

        private const string MenuButtonClassName = UssClassName + "__button";

        /// <summary> The Toolbar background styling class. </summary>
        private const string MenuUssClassName = UssClassName + "__menu";

        /// <summary> The Toolbar show button styling class. </summary>
        private const string ShowUssClassName = UssClassName + "__show";

        /// <summary> The Toolbar filter button styling class. </summary>
        private const string FilterUssClassName = UssClassName + "__filter";

        /// <summary> The Toolbar container styling class. </summary>
        private const string MenuContainerUssClassName = UssClassName + "__menu-container";

        private const string MenuButtonUssClassName = UssClassName + "__menu-button";

        private const string ShowIconUssClassName = ShowUssClassName + "__icon";

        private const string ShowHiddenUssClassName = ShowIconUssClassName + "-hidden";

        private const string ShowIconTargetClass = "appui-button__trailingicon";
        private const string ActiveTabKey = "bl.active-tab";
        private const string ShowRibbonKey = "bl.show-ribbon";
        private const float RestoreHotspotPercent = 0.04f;
        private const int RestoreClickThreshold = 5;
        private const float RestoreClickResetSeconds = 1f;

        [ConfigVar("anchor.toolbar", true, "Should the toolbar be shown", true)]
        private static readonly SharedStatic<bool> Show = SharedStatic<bool>.GetOrCreate<ToolbarView, EnabledVar>();

        private readonly List<Transform> transformList = new();
        private readonly Dictionary<string, ToolbarGroup> toolbarTabs = new();
        private readonly Dictionary<int, ToolbarGroup.Tab> toolbarGroups = new();
        private readonly ToolbarViewModel viewModel;
        private readonly ILocalStorageService storageService;

        private readonly VisualElement menuContainer;
        private readonly Dropdown filterButton;
        private readonly Button showButton;

        private ToolbarGroup activeGroup;

        private Vector2 uiSize;
        private int key;
        private bool toolbarHidden;
        private int restoreClickCount;
        private float lastRestoreClickTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarView"/> class.
        /// </summary>
        /// <param name="viewModel">Backing view model used to manage selections.</param>
        /// <param name="storageService">Persistence layer for storing toolbar preferences.</param>
        public ToolbarView(ToolbarViewModel viewModel, ILocalStorageService storageService)
        {
            Instance = this;

            this.viewModel = viewModel;
            this.storageService = storageService;

            this.AddToClassList(UssClassName);

            var menu = new VisualElement();
            menu.AddToClassList(MenuUssClassName);

            this.showButton = this.CreateShowButton();
            this.filterButton = this.CreateFilterButton();
            var hideButton = this.CreateHideButton();

            this.menuContainer = new VisualElement();
            this.menuContainer.AddToClassList(MenuContainerUssClassName);

            menu.Add(this.showButton);
            menu.Add(this.filterButton);
            menu.Add(this.menuContainer);
            menu.Add(hideButton);

            this.Add(menu);

            DisableKeyboardNavigation(menu);

            this.RegisterCallback<GeometryChangedEvent, ToolbarView>((evt, tv) => tv.ResizeViewRect(evt.newRect), this);
            this.viewModel.PropertyChanged += this.OnPropertyChanged;

            var serviceTabName = AnchorApp.current?.ServiceTabName;
            if (string.IsNullOrWhiteSpace(serviceTabName))
            {
                serviceTabName = AnchorApp.DefaultServiceTabName;
            }

            foreach (var t in ReflectionUtility.GetAllWithAttribute<AutoToolbarAttribute>())
            {
                var attr = t.GetCustomAttribute<AutoToolbarAttribute>();
                var tabName = attr.TabName ?? serviceTabName;

                this.AddTab(t, tabName, attr.ElementName, out _, out _);
            }

            this.SetDefaultGroup();

            if (!Show.Data)
            {
                this.HideToolbar();
            }

            App.current.rootVisualElement.RegisterCallback<GeometryChangedEvent, ToolbarView>((evt, tv) => tv.OnRootContentChanged(evt.newRect), this);
            App.current.rootVisualElement.RegisterCallback<PointerDownEvent>(this.OnRootPointerDown);

            App.shuttingDown += this.AppOnShuttingDown;
        }

        /// <summary>Gets the active toolbar view instance, if any.</summary>
        public static ToolbarView Instance { get; private set; }

        private bool IsRibbonVisible
        {
            get => bool.TryParse(this.storageService.GetValue(ShowRibbonKey), out var value) && value;
            set => this.storageService.SetValue(ShowRibbonKey, value.ToString());
        }

        /// <summary>
        /// Adds a VisualElement service as a toolbar tab entry.
        /// </summary>
        /// <typeparam name="T">Type of view to resolve from the service container.</typeparam>
        /// <param name="tabName">Name shown on the toolbar tab.</param>
        /// <param name="elementName">Name of the service element registered in UXML.</param>
        /// <param name="id">Outputs the assigned unique tab identifier.</param>
        /// <param name="view">Outputs the instantiated view.</param>
        public void AddTab<T>(string tabName, string elementName, out int id, out T view)
            where T : VisualElement
        {
            this.AddTab(typeof(T), tabName, elementName, out id, out var visualElement);
            view = (T)visualElement;
        }

        /// <summary>
        /// Adds a VisualElement service as a toolbar tab entry.
        /// </summary>
        /// <param name="viewType">Concrete view type that will be resolved from services.</param>
        /// <param name="tabName">Name shown on the toolbar tab.</param>
        /// <param name="elementName">Name of the service element registered in UXML.</param>
        /// <param name="id">Outputs the assigned unique tab identifier.</param>
        /// <param name="view">Outputs the instantiated view.</param>
        public void AddTab(Type viewType, string tabName, string elementName, out int id, out VisualElement view)
        {
            if (!typeof(VisualElement).IsAssignableFrom(viewType))
            {
                throw new ArgumentException($"{viewType} is not a {nameof(VisualElement)}", nameof(viewType));
            }

            if (!viewType.IsDefined(typeof(IsServiceAttribute)))
            {
                throw new ArgumentException($"{viewType} is not defined as a service", nameof(viewType));
            }

            id = ++this.key;

            view = (VisualElement)App.current.services.GetService(viewType);

            if (!this.toolbarTabs.TryGetValue(tabName, out var tab))
            {
                tab = this.toolbarTabs[tabName] = this.CreateTab(tabName);
            }

            var container = new ToolbarTabElement(elementName);
            container.Add(view);

            var group = new ToolbarGroup.Tab(id, elementName, container, tab, view);
            this.toolbarGroups.Add(id, group);

            tab.Groups.Add(group);
            tab.Groups.Sort((t1, t2) => string.Compare(t1.Name, t2.Name, StringComparison.Ordinal));

            this.viewModel.AddSelection(group.Name);

            if (!this.viewModel.SelectionsHidden.Contains(group.Name))
            {
                this.ShowTab(group);
            }

            DisableKeyboardNavigation(view);
        }

        /// <summary>
        /// Removes a previously registered tab by id.
        /// </summary>
        /// <param name="id">Identifier of the tab that should be removed.</param>
        /// <typeparam name="T">Expected type of the view backing the tab.</typeparam>
        /// <returns>The removed view, or null if the id was not found.</returns>
        public T RemoveTab<T>(int id)
            where T : VisualElement
        {
            if (!this.toolbarGroups.Remove(id, out var group))
            {
                return null;
            }

            this.viewModel.RemoveSelection(group.Name);

            this.HideTab(group);
            group.Group.Groups.Remove(group);

            if (group.View is IDisposable disposable)
            {
                disposable.Dispose();
            }

            return (T)group.View;
        }

        /// <summary>Gets the VisualElement backing a registered tab.</summary>
        /// <param name="id">Identifier of the tab that should be queried.</param>
        /// <returns>The view backing the tab, or null when not found.</returns>
        public VisualElement GetPanel(int id)
        {
            return this.toolbarGroups.TryGetValue(id, out var group) ? group.View : null;
        }

        private static void DisableKeyboardNavigation(VisualElement root)
        {
            root.focusable = false;

            foreach (var child in root.Children())
            {
                DisableKeyboardNavigation(child);
            }
        }

        private static int FindInsertIndex(ToolbarGroup.Tab tab)
        {
            var group = tab.Group;
            var index = group.Groups.FindIndex(t => t == tab);

            // Start index before us
            for (var i = index - 1; i >= 0; i--)
            {
                // tab.Groups is sorted alphabetically so we find the closest active element before this
                if (group.Groups[i].Container.parent != null)
                {
                    // Then find the index in the visual element
                    return group.Parent.IndexOf(group.Groups[i].Container) + 1; // insert after it
                }
            }

            return 0;
        }

        private void AppOnShuttingDown()
        {
            if (Instance == this)
            {
                // Instance = null;
            }

            if (App.current?.rootVisualElement != null)
            {
                App.current.rootVisualElement.UnregisterCallback<PointerDownEvent>(this.OnRootPointerDown);
            }

            App.shuttingDown -= this.AppOnShuttingDown;
        }

        private void ShowTab(ToolbarGroup.Tab tab)
        {
            // Already visible
            if (tab.Container.parent != null)
            {
                return;
            }

            var insert = FindInsertIndex(tab);

            var group = tab.Group;
            group.Parent.Insert(insert, tab.Container);

            // If this tab is hidden, show it
            if (group.Button.parent == null)
            {
                this.menuContainer.Add(group.Button);
            }

            // If the first toolbar group loads after the toolbar we want to set it as default
            if (this.activeGroup == null)
            {
                this.SetToolbarActive(group);
            }
        }

        private void HideTab(ToolbarGroup.Tab tab)
        {
            if (tab.Container.parent == null)
            {
                return;
            }

            var group = tab.Group;
            tab.Container.RemoveFromHierarchy();

            // Removed all groups, hide the tab
            if (group.Parent.childCount == 0)
            {
                group.Button.RemoveFromHierarchy();

                if (this.activeGroup == group)
                {
                    this.SetDefaultGroup();
                }
            }
        }

        private void OnRootContentChanged(Rect newRect)
        {
            // var height = newRect.height;
            var newSize = newRect.size;

            if (!this.uiSize.Equals(newSize))
            {
                this.uiSize = newSize;
                this.ResizeViewRect(this.contentRect);
            }
        }

        private Button CreateShowButton()
        {
            var button = new Button(() => this.ShowRibbon(!this.IsRibbonVisible)) { trailingIcon = "caret-down" };

            button.AddToClassList(MenuButtonClassName);
            button.AddToClassList(ShowUssClassName);

            var icon = button.Q<Icon>(ShowIconTargetClass);
            icon.AddToClassList(ShowIconUssClassName);
            icon.AddToClassList(ShowHiddenUssClassName);
            button.size = Size.S;

            return button;
        }

        private Dropdown CreateFilterButton()
        {
            var dropdown = new Dropdown
            {
                dataSource = this.viewModel,
                sourceItems = this.viewModel.FilterItems,
                selectionType = PickerSelectionType.Multiple,
                closeOnSelection = false,
                defaultMessage = string.Empty,
                bindTitle = (item, _) => item.labelElement.text = string.Empty,
                bindItem = (item, i) => item.label = this.viewModel.FilterItems[i],
                value = this.viewModel.FilterValues,
            };

            dropdown.SetBinding(nameof(Dropdown.value), new DataBinding
            {
                bindingMode = BindingMode.ToSource,
                dataSourcePath = new PropertyPath(nameof(ToolbarViewModel.FilterValues)),
            });

            dropdown.AddToClassList(MenuButtonClassName);
            dropdown.AddToClassList(FilterUssClassName);
            dropdown.size = Size.S;

            return dropdown;
        }

        private Button CreateHideButton()
        {
            var button = new Button(this.HideToolbar) { trailingIcon = "x" };

            button.AddToClassList(MenuButtonClassName);
            button.AddToClassList(ShowUssClassName);
            button.size = Size.S;

            return button;
        }

        private void HideToolbar()
        {
            if (this.toolbarHidden)
            {
                return;
            }

            this.style.display = DisplayStyle.None;
            this.toolbarHidden = true;
            this.ResetRestoreClickState();
        }

        private void RestoreToolbar()
        {
            if (!this.toolbarHidden)
            {
                return;
            }

            this.style.display = DisplayStyle.Flex;
            this.toolbarHidden = false;
            this.ResetRestoreClickState();
        }

        private void ResetRestoreClickState()
        {
            this.restoreClickCount = 0;
            this.lastRestoreClickTime = 0f;
        }

        private void OnRootPointerDown(PointerDownEvent evt)
        {
            // Only track restoration clicks when the toolbar is hidden from view.
            var isHidden = this.toolbarHidden || this.resolvedStyle.display == DisplayStyle.None;
            this.toolbarHidden = isHidden;

            if (!isHidden)
            {
                this.ResetRestoreClickState();
                return;
            }

            var root = App.current.rootVisualElement;
            var rect = root.contentRect;
            var width = rect.width;
            var height = rect.height;

            var hotspotHeight = Screen.height * RestoreHotspotPercent;
            var hotspotWidth = hotspotHeight;

            if (Screen.width > 0f && Screen.height > 0f)
            {
                var safeArea = Screen.safeArea;
                var safeRight = Mathf.Max(0f, Screen.width - (safeArea.x + safeArea.width));
                var safeTop = Mathf.Max(0f, Screen.height - (safeArea.y + safeArea.height));

                hotspotWidth += (safeRight / Screen.width) * width;
                hotspotHeight += (safeTop / Screen.height) * height;
            }

            var position = evt.localPosition;

            if (width <= 0 || position.x < width - hotspotWidth || position.y > hotspotHeight)
            {
                this.ResetRestoreClickState();
                return;
            }

            if (evt.button != (int)MouseButton.LeftMouse && evt.button != -1)
            {
                return;
            }

            var time = Time.realtimeSinceStartup;

            if (time - this.lastRestoreClickTime > RestoreClickResetSeconds)
            {
                this.restoreClickCount = 0;
            }

            this.lastRestoreClickTime = time;
            this.restoreClickCount++;

            if (this.restoreClickCount < RestoreClickThreshold)
            {
                return;
            }

            this.RestoreToolbar();
            evt.StopPropagation();
        }

        private ToolbarGroup CreateTab(string tabName)
        {
            var button = new Button
            {
                title = tabName,
                focusable = false,
            };

            button.AddToClassList(MenuButtonUssClassName);

            var contents = new ToolbarGroupElement();

            var toolbarTab = new ToolbarGroup(tabName, button, contents);

            button.clicked += () =>
            {
                this.storageService.SetValue(ActiveTabKey, tabName);
                this.SetToolbarActive(toolbarTab);

                if (!this.IsRibbonVisible)
                {
                    this.ShowRibbon(true);
                }
            };

            if (this.storageService.GetValue(ActiveTabKey) == tabName)
            {
                this.SetToolbarActive(toolbarTab);
            }

            return toolbarTab;
        }

        private void SetToolbarActive(ToolbarGroup group)
        {
            if (group == this.activeGroup)
            {
                if (this.IsRibbonVisible)
                {
                    this.ShowRibbon(true);
                }

                return;
            }

            if (this.activeGroup != null)
            {
                this.activeGroup.Button.variant = ButtonVariant.Default;

                // something else has already removed it or moved it
                if (this.activeGroup.Parent.parent == this)
                {
                    this.activeGroup.Parent.RemoveFromTab();
                }

                this.activeGroup = null;
                ToolbarViewData.ActiveTab.Data = string.Empty;
            }

            if (group == null)
            {
                return;
            }

            this.activeGroup = group;
            ToolbarViewData.ActiveTab.Data = group.Name;
            group.Button.variant = ButtonVariant.Accent;

            if (this.IsRibbonVisible)
            {
                this.ShowRibbon(true);
            }
        }

        private void SetDefaultGroup()
        {
            this.SetToolbarActive(null);

            // First is always the toggle button
            if (this.menuContainer.childCount == 0)
            {
                return;
            }

            var firstButton = (Button)this.menuContainer.contentContainer.Children().First();
            var group = this.toolbarTabs.First(g => g.Value.Button == firstButton);
            this.SetToolbarActive(group.Value);
        }

        private void ShowRibbon(bool show)
        {
            if (show)
            {
                this.showButton.Q<Icon>(ShowIconTargetClass).RemoveFromClassList(ShowHiddenUssClassName);

                if (this.activeGroup != null)
                {
                    if (this.activeGroup.Parent.parent != null)
                    {
                        Assert.IsTrue(this == this.activeGroup.Parent.parent);
                        return;
                    }

                    this.activeGroup.Parent.AddToTab(this);
                }
            }
            else
            {
                this.showButton.Q<Icon>(ShowIconTargetClass).AddToClassList(ShowHiddenUssClassName);

                if (this.activeGroup != null)
                {
                    Assert.IsTrue(this.activeGroup.Parent.parent == this);
                    this.activeGroup.Parent.RemoveFromTab();
                }
            }

            this.IsRibbonVisible = show;
        }

        private void ResizeViewRect(Rect uiRect)
        {
            if (this.uiSize.Equals(Vector2.zero))
            {
                return;
            }

            this.UpdateSafeArea();

            var cameraHeightNormalized = (this.uiSize.y - uiRect.height) / this.uiSize.y;
            this.ResizeCamera(cameraHeightNormalized);
            this.ResizeCanvas(cameraHeightNormalized);

            // if (AnchorApp.current.PopupContainer != null)
            // {
            //     AnchorApp.current.PopupContainer.style.top = uiRect.height;
            // }
            //
            // if (AnchorApp.current.NotificationContainer != null)
            // {
            //     AnchorApp.current.NotificationContainer.style.top = uiRect.height;
            // }
            //
            // if (AnchorApp.current.TooltipContainer != null)
            // {
            //     AnchorApp.current.TooltipContainer.style.top = uiRect.height;
            // }
        }

        private void ResizeCamera(float cameraHeightNormalized)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var rect = cam.rect;
                rect.height = cameraHeightNormalized;
                cam.rect = rect;

#if UNITY_URP
                var additional = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                if (additional != null)
                {
                    foreach (var camera in additional.cameraStack)
                    {
                        if (camera == null)
                        {
                            continue;
                        }

                        if (camera.rect != rect)
                        {
                            camera.rect = rect;
                        }
                    }
                }
#endif
            }
        }

        private void ResizeCanvas(float cameraHeightNormalized)
        {
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    continue;
                }

                if (!canvas.isRootCanvas)
                {
                    continue;
                }

                ToolbarOffset offset = null;

                // Only look in direct children
                for (var i = 0; i < canvas.transform.childCount; i++)
                {
                    offset = canvas.transform.GetChild(i).GetComponent<ToolbarOffset>();
                    if (offset != null)
                    {
                        break;
                    }
                }

                if (offset == null)
                {
                    // Get children to move
                    this.transformList.Clear();
                    for (var i = 0; i < canvas.transform.childCount; i++)
                    {
                        this.transformList.Add(canvas.transform.GetChild(i));
                    }

                    // Create new
                    var go = new GameObject("ToolbarOffset");
                    go.transform.SetParent(canvas.transform);

                    offset = go.AddComponent<ToolbarOffset>();
                    var offsetTr = offset.transform;

                    foreach (var child in this.transformList)
                    {
                        child.SetParent(offsetTr, true);
                    }
                }

                ((RectTransform)offset.transform).offsetMax = new Vector2(0, (cameraHeightNormalized-1) * Screen.height);
            }
        }

        private void UpdateSafeArea()
        {
            if (this.uiSize.Equals(Vector2.zero))
            {
                return;
            }

            var safeSpace = AnchorApp.SafeArea;
            var screenWidth = (float)Screen.width;
            var screenHeight = (float)Screen.height;
            var uiWidth = this.uiSize.x;
            var uiHeight = this.uiSize.y;

            if (screenWidth <= 0 || screenHeight <= 0 || uiWidth <= 0 || uiHeight <= 0)
            {
                return;
            }

            var leftInset = safeSpace.x;
            var rightInset = screenWidth - safeSpace.width - safeSpace.x;
            var topInset = screenHeight - safeSpace.height - safeSpace.y;

            var leftPercent = (leftInset / screenWidth) * 100f;
            var rightPercent = (rightInset / screenWidth) * 100f;

            var topInsetUi = (topInset / screenHeight) * uiHeight;
            var topPercent = (topInsetUi / uiWidth) * 100f;

            this.style.paddingLeft = new StyleLength(Length.Percent(leftPercent));
            this.style.paddingRight = new StyleLength(Length.Percent(rightPercent));
            this.style.paddingTop = new StyleLength(Length.Percent(topPercent));
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ToolbarViewModel.FilterItems))
            {
                this.filterButton.value = this.viewModel.FilterValues; // Can't rely on binding to have updated in time
                this.filterButton.Refresh();
            }
            else if (e.PropertyName == nameof(ToolbarViewModel.FilterValues))
            {
                foreach (var tabGroup in this.toolbarTabs.ToArray())
                {
                    foreach (var t in tabGroup.Value.Groups)
                    {
                        if (this.viewModel.SelectionsHidden.Contains(t.Name))
                        {
                            this.HideTab(t);
                        }
                        else
                        {
                            this.ShowTab(t);
                        }
                    }
                }
            }
        }

        private struct EnabledVar
        {
        }
    }
}
#endif
