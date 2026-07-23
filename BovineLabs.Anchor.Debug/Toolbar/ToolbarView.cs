// <copyright file="ToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using BovineLabs.Core.Assertions;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;
    using Canvas = UnityEngine.Canvas;
    using Object = UnityEngine.Object;

    /// <summary>
    /// Ribbon-style toolbar that surfaces debug and service panels within the Anchor app.
    /// </summary>
    public class ToolbarView : VisualElement, IDisposable
    {
        /// <summary>
        /// The NavigationScreen main styling class.
        /// </summary>
        private const string UssClassName = "bl-toolbar";
        private const string MenuButtonClassName = UssClassName + "__button";
        private const string MenuUssClassName = UssClassName + "__menu";
        private const string ShowUssClassName = UssClassName + "__show";
        private const string FilterUssClassName = UssClassName + "__filter";
        private const string MenuContainerUssClassName = UssClassName + "__menu-container";
        private const string MenuButtonUssClassName = UssClassName + "__menu-button";
        private const string ShowIconUssClassName = ShowUssClassName + "__icon";
        private const string ShowHiddenUssClassName = ShowIconUssClassName + "-hidden";
        private const string ShowIconTargetClassName = "appui-button__trailingicon";

        private const float RestoreHotspotPercent = 0.04f;
        private const int RestoreClickThreshold = 5;
        private const float RestoreClickResetSeconds = 1f;

        private readonly List<Transform> transformList = new();
        private readonly Dictionary<string, ToolbarGroup> toolbarTabs = new();
        private readonly Dictionary<int, ToolbarGroup.Tab> toolbarGroups = new();
        private readonly Toolbar toolbar;
        private readonly ToolbarViewModel viewModel;

        private readonly VisualElement menuContainer;
        private readonly Dropdown filterButton;
        private readonly Button showButton;

        private ToolbarGroup activeGroup;
        private AnchorApp anchorApp;
        private VisualElement panelRoot;

        private Vector2 uiSize;
        private bool toolbarHidden;
        private bool compositionCompleted;
        private bool disposed;
        private int restoreClickCount;
        private float lastRestoreClickTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolbarView"/> class.
        /// </summary>
        /// <param name="toolbar">Durable toolbar service that owns state and registrations.</param>
        /// <param name="viewModel">Backing view model used to manage selections.</param>
        internal ToolbarView(Toolbar toolbar, ToolbarViewModel viewModel)
        {
            this.toolbar = toolbar;
            this.viewModel = viewModel;
            this.dataSource = viewModel;

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

            this.RegisterCallback<GeometryChangedEvent, ToolbarView>(OnToolbarGeometryChanged, this);
            this.viewModel.PropertyChanged += this.OnPropertyChanged;

            if (this.toolbar.IsToolbarHidden)
            {
                this.HideToolbar();
            }

            this.RegisterCallback<AttachToPanelEvent>(this.OnAttachToPanel);
            this.RegisterCallback<DetachFromPanelEvent>(this.OnDetachFromPanel);
        }

        public bool ToolbarHidden => this.toolbarHidden;

        private bool IsRibbonVisible
        {
            get => this.toolbar.IsRibbonVisible;
            set => this.toolbar.SetRibbonVisible(value);
        }

        internal void AddRegistration(int id, string tabName, string elementName, VisualElement element)
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(ToolbarView));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (element.parent != null)
            {
                throw new ArgumentException("Toolbar elements must be unattached when registered.", nameof(element));
            }

            if (!this.toolbarTabs.TryGetValue(tabName, out var tab))
            {
                tab = this.toolbarTabs[tabName] = this.CreateTab(tabName);
            }

            var container = new ToolbarTabElement(elementName);
            container.Add(element);

            var group = new ToolbarGroup.Tab(id, elementName, container, tab, element);
            this.toolbarGroups.Add(id, group);

            tab.Groups.Add(group);
            tab.Groups.Sort(static (t1, t2) =>
            {
                var nameComparison = string.Compare(t1.Name, t2.Name, StringComparison.Ordinal);
                return nameComparison != 0 ? nameComparison : t1.ID.CompareTo(t2.ID);
            });

            if (!this.viewModel.SelectionsHidden.Contains(group.Name))
            {
                this.ShowTab(group);
            }

            DisableKeyboardNavigation(element);

            if (this.compositionCompleted)
            {
                this.EnsureActiveGroup();
            }
        }

        internal void RemoveRegistration(int id)
        {
            if (!this.toolbarGroups.Remove(id, out var group))
            {
                return;
            }

            this.HideTab(group);
            group.Group.Groups.Remove(group);
            ReleaseVisualElement(group.View);

            if (this.compositionCompleted)
            {
                this.EnsureActiveGroup();
            }
        }

        internal void CompleteComposition()
        {
            this.compositionCompleted = true;
            this.EnsureActiveGroup();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.compositionCompleted = false;

            this.viewModel.PropertyChanged -= this.OnPropertyChanged;

            if (!this.resourcesReleased)
            {
                this.UnregisterCallback<GeometryChangedEvent, ToolbarView>(OnToolbarGeometryChanged);
                this.UnregisterCallback<AttachToPanelEvent>(this.OnAttachToPanel);
                this.UnregisterCallback<DetachFromPanelEvent>(this.OnDetachFromPanel);
            }

            this.UnregisterPanelRoot();

            foreach (var group in this.toolbarGroups.Values)
            {
                ReleaseVisualElement(group.View);
            }

            this.toolbarGroups.Clear();
            this.toolbarTabs.Clear();
            this.activeGroup = null;

            if (this.resourcesReleased)
            {
                this.dataSource = null;
                this.filterButton.dataSource = null;
            }
            else
            {
                this.RemoveFromHierarchy();
                ClearBindingsAndDataSources(this);
                this.Clear();
            }
        }

        /// <summary>Shows or hides the active ribbon group.</summary>
        /// <param name="show">Whether the active ribbon group should be shown.</param>
        public void ShowRibbon(bool show)
        {
            if (show)
            {
                this.showButton.Q<Icon>(ShowIconTargetClassName).RemoveFromClassList(ShowHiddenUssClassName);

                if (this.activeGroup != null)
                {
                    if (this.activeGroup.Parent.parent == null)
                    {
                        this.activeGroup.Parent.AddToTab(this);
                    }
                    else
                    {
                        Assert.IsTrue(this == this.activeGroup.Parent.parent);
                    }
                }
            }
            else
            {
                this.showButton.Q<Icon>(ShowIconTargetClassName).AddToClassList(ShowHiddenUssClassName);

                if (this.activeGroup?.Parent.parent != null)
                {
                    Assert.IsTrue(this.activeGroup.Parent.parent == this);
                    this.activeGroup.Parent.RemoveFromTab();
                }
            }

            this.IsRibbonVisible = show;
        }

        public void HideToolbar()
        {
            if (this.toolbarHidden)
            {
                return;
            }

            this.style.display = DisplayStyle.None;
            this.toolbarHidden = true;
            this.toolbar.SetToolbarHidden(true);
            this.ResetRestoreClickState();
        }

        public void RestoreToolbar()
        {
            if (!this.toolbarHidden)
            {
                return;
            }

            this.style.display = DisplayStyle.Flex;
            this.toolbarHidden = false;
            this.toolbar.SetToolbarHidden(false);
            this.ResetRestoreClickState();
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

        private static void OnToolbarGeometryChanged(GeometryChangedEvent evt, ToolbarView toolbarView)
        {
            toolbarView.ResizeViewRect(evt.newRect);
        }

        private static void ReleaseVisualElement(VisualElement element)
        {
            var resourcesReleased = element.resourcesReleased;

            try
            {
                if (element is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            finally
            {
                if (resourcesReleased)
                {
                    element.dataSource = null;
                }
                else
                {
                    element.RemoveFromHierarchy();
                    ClearBindingsAndDataSources(element);
                }
            }
        }

        private static void ClearBindingsAndDataSources(VisualElement element)
        {
            for (var i = 0; i < element.hierarchy.childCount; i++)
            {
                ClearBindingsAndDataSources(element.hierarchy[i]);
            }

            element.ClearBindings();
            element.dataSource = null;
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            this.RegisterPanelRoot(evt.destinationPanel?.visualTree);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            this.UnregisterPanelRoot();
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
                    this.SetToolbarActive(null);
                }
            }
        }

        private void RegisterPanelRoot(VisualElement root)
        {
            if (ReferenceEquals(this.panelRoot, root))
            {
                this.UpdatePanelSize();
                return;
            }

            this.UnregisterPanelRoot();
            this.panelRoot = root;

            if (this.panelRoot == null)
            {
                this.anchorApp = null;
                this.uiSize = Vector2.zero;
                return;
            }

            this.anchorApp = AnchorApp.Current;
            this.panelRoot.RegisterCallback<GeometryChangedEvent>(this.OnPanelRootGeometryChanged);
            this.panelRoot.RegisterCallback<PointerDownEvent>(this.OnRootPointerDown);
            if (this.anchorApp != null)
            {
                this.anchorApp.ScreenMetricsChanged += this.OnScreenMetricsChanged;
            }

            this.UpdatePanelSize();
            this.ResizeViewRect(this.contentRect);
        }

        private void UnregisterPanelRoot()
        {
            if (this.anchorApp != null)
            {
                this.anchorApp.ScreenMetricsChanged -= this.OnScreenMetricsChanged;
            }

            if (this.panelRoot != null && !this.panelRoot.resourcesReleased)
            {
                this.panelRoot.UnregisterCallback<GeometryChangedEvent>(this.OnPanelRootGeometryChanged);
                this.panelRoot.UnregisterCallback<PointerDownEvent>(this.OnRootPointerDown);
            }

            this.ResetCanvasOffsets();

            if (!this.resourcesReleased)
            {
                AnchorSafeAreaUtility.ResetPadding(this.style);
            }

            this.anchorApp = null;
            this.panelRoot = null;
            this.uiSize = Vector2.zero;
        }

        private void OnPanelRootGeometryChanged(GeometryChangedEvent evt)
        {
            this.UpdatePanelSize();
            this.ResizeViewRect(this.contentRect);
        }

        private void OnScreenMetricsChanged(AnchorScreenMetrics metrics)
        {
            this.UpdatePanelSize();
            this.ResizeViewRect(this.contentRect);
        }

        private void UpdatePanelSize()
        {
            this.uiSize = this.GetPanelUiSize();
        }

        private Vector2 GetPanelUiSize()
        {
            var layoutSize = this.panel.visualTree.layout.size;
            if (float.IsNaN(layoutSize.x) || float.IsNaN(layoutSize.y))
            {
                return Vector2.zero;
            }

            return layoutSize;
        }

        private Button CreateShowButton()
        {
            var button = new Button(() => this.ShowRibbon(!this.IsRibbonVisible)) { trailingIcon = "caret-down" };

            button.AddToClassList(MenuButtonClassName);
            button.AddToClassList(ShowUssClassName);
            button.size = Size.S;

            var icon = button.Q<Icon>(ShowIconTargetClassName);
            icon.AddToClassList(ShowIconUssClassName);
            icon.AddToClassList(ShowHiddenUssClassName);

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

            var width = this.uiSize.x;
            var height = this.uiSize.y;

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
                this.SetToolbarActive(toolbarTab);

                if (!this.IsRibbonVisible)
                {
                    this.ShowRibbon(true);
                }
            };

            return toolbarTab;
        }

        private void SetToolbarActive(ToolbarGroup group, bool updateState = true)
        {
            if (group == this.activeGroup)
            {
                if (updateState)
                {
                    this.toolbar.SetActiveTab(group?.Name);
                }

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
            }

            if (group == null)
            {
                if (updateState)
                {
                    this.toolbar.SetActiveTab(string.Empty);
                }

                return;
            }

            this.activeGroup = group;
            group.Button.variant = ButtonVariant.Accent;

            if (updateState)
            {
                this.toolbar.SetActiveTab(group.Name);
            }

            if (this.IsRibbonVisible)
            {
                this.ShowRibbon(true);
            }
        }

        private void EnsureActiveGroup()
        {
            if (this.activeGroup?.Button.parent != null)
            {
                return;
            }

            if (this.toolbarTabs.TryGetValue(this.toolbar.ActiveTabName, out var storedGroup) && storedGroup.Button.parent != null)
            {
                this.SetToolbarActive(storedGroup, false);
                return;
            }

            this.SetDefaultGroup();
        }

        private void SetDefaultGroup()
        {
            this.SetToolbarActive(null, false);

            if (this.menuContainer.childCount == 0)
            {
                this.toolbar.SetActiveTab(string.Empty);
                return;
            }

            var firstButton = (Button)this.menuContainer.contentContainer.Children().First();
            var group = this.toolbarTabs.First(g => g.Value.Button == firstButton);
            this.SetToolbarActive(group.Value);
        }

        private void ResizeViewRect(Rect uiRect)
        {
            if (this.uiSize.y == 0 || float.IsNaN(uiRect.height))
            {
                return;
            }

            Check.Assume(!float.IsNaN(this.uiSize.y));

            AnchorSafeAreaUtility.ApplyPadding(this, this, this.panelRoot, AnchorSafeAreaEdges.Top | AnchorSafeAreaEdges.Left | AnchorSafeAreaEdges.Right);

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

                var additional = cam.GetComponent<UniversalAdditionalCameraData>();
                if (additional != null && additional.scriptableRenderer.SupportsCameraStackingType(CameraRenderType.Base))
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
            }
        }

        private void ResizeCanvas(float cameraHeightNormalized)
        {
            var canvases = Object.FindObjectsByType<Canvas>();
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

                var offset = this.GetOrCreateToolbarOffset(canvas);
                this.MoveCanvasChildrenToOffset(canvas, offset);

                var scaleFactor = canvas.scaleFactor;
                if (scaleFactor <= 0f)
                {
                    scaleFactor = 1f;
                }

                // Convert screen-pixel offset into canvas units (handles CanvasScaler).
                var offsetHeight = (cameraHeightNormalized - 1f) * (Screen.height / scaleFactor);
                ((RectTransform)offset.transform).offsetMax = new Vector2(0f, offsetHeight);
            }
        }

        private ToolbarOffset GetOrCreateToolbarOffset(Canvas canvas)
        {
            for (var i = 0; i < canvas.transform.childCount; i++)
            {
                var offset = canvas.transform.GetChild(i).GetComponent<ToolbarOffset>();
                if (offset != null)
                {
                    return offset;
                }
            }

            var go = new GameObject("ToolbarOffset");
            go.transform.SetParent(canvas.transform, false);
            return go.AddComponent<ToolbarOffset>();
        }

        private void MoveCanvasChildrenToOffset(Canvas canvas, ToolbarOffset offset)
        {
            var offsetTransform = offset.transform;

            this.transformList.Clear();
            for (var i = 0; i < canvas.transform.childCount; i++)
            {
                var child = canvas.transform.GetChild(i);
                if (child != offsetTransform)
                {
                    this.transformList.Add(child);
                }
            }

            foreach (var child in this.transformList)
            {
                child.SetParent(offsetTransform, true);
            }
        }

        private void ResetCanvasOffsets()
        {
            var canvases = Object.FindObjectsByType<Canvas>();
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

                var offset = this.TryGetToolbarOffset(canvas);
                if (offset == null)
                {
                    continue;
                }

                var offsetTransform = offset.transform;
                this.transformList.Clear();
                for (var i = 0; i < offsetTransform.childCount; i++)
                {
                    this.transformList.Add(offsetTransform.GetChild(i));
                }

                foreach (var child in this.transformList)
                {
                    child.SetParent(canvas.transform, true);
                }

                Object.Destroy(offset.gameObject);
            }
        }

        private ToolbarOffset TryGetToolbarOffset(Canvas canvas)
        {
            for (var i = 0; i < canvas.transform.childCount; i++)
            {
                var offset = canvas.transform.GetChild(i).GetComponent<ToolbarOffset>();
                if (offset != null)
                {
                    return offset;
                }
            }

            return null;
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

                if (this.compositionCompleted)
                {
                    this.EnsureActiveGroup();
                }
            }
        }
    }
}
