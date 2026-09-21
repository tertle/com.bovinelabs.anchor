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

    public class ToolbarView : VisualElement, IDisposable
    {
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
        private const int CameraRefreshMilliseconds = 250;

        private readonly List<Transform> _transformList = new();
        private readonly List<Camera> _cameraList = new();
        private readonly Dictionary<Camera, Rect> _originalCameraRects = new();
        private readonly Dictionary<string, ToolbarGroup> _toolbarTabs = new();
        private readonly Dictionary<int, ToolbarGroup.Tab> _toolbarGroups = new();
        private readonly Toolbar _toolbar;
        private readonly ToolbarViewModel _viewModel;

        private readonly VisualElement _menuContainer;
        private readonly Dropdown _filterButton;
        private readonly Button _showButton;
        private readonly IVisualElementScheduledItem _cameraRefresh;

        private ToolbarGroup _activeGroup;
        private AnchorApp _anchorApp;
        private VisualElement _panelRoot;

        private Vector2 _uiSize;
        private float _cameraHeightNormalized;
        private bool _toolbarHidden;
        private bool _hasCameraHeight;
        private bool _compositionCompleted;
        private bool _disposed;
        private int _restoreClickCount;
        private float _lastRestoreClickTime;

        internal ToolbarView(Toolbar toolbar, ToolbarViewModel viewModel)
        {
            _toolbar = toolbar;
            _viewModel = viewModel;
            dataSource = viewModel;

            AddToClassList(UssClassName);

            var menu = new VisualElement();
            menu.AddToClassList(MenuUssClassName);

            _showButton = CreateShowButton();
            _filterButton = CreateFilterButton();
            var hideButton = CreateHideButton();

            _menuContainer = new VisualElement();
            _menuContainer.AddToClassList(MenuContainerUssClassName);

            menu.Add(_showButton);
            menu.Add(_filterButton);
            menu.Add(_menuContainer);
            menu.Add(hideButton);

            Add(menu);

            DisableKeyboardNavigation(menu);

            RegisterCallback<GeometryChangedEvent, ToolbarView>(OnToolbarGeometryChanged, this);
            _viewModel.PropertyChanged += OnPropertyChanged;

            if (_toolbar.IsToolbarHidden)
            {
                HideToolbar();
            }

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            _cameraRefresh = schedule.Execute(RefreshCameraRects).Every(CameraRefreshMilliseconds);
        }

        public bool ToolbarHidden => _toolbarHidden;

        private bool IsRibbonVisible
        {
            get => _toolbar.IsRibbonVisible;
            set => _toolbar.SetRibbonVisible(value);
        }

        internal void AddRegistration(int id, string tabName, string elementName, VisualElement element)
        {
            if (_disposed)
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

            if (!_toolbarTabs.TryGetValue(tabName, out var tab))
            {
                tab = _toolbarTabs[tabName] = CreateTab(tabName);
            }

            var container = new ToolbarTabElement(elementName);
            container.Add(element);

            var group = new ToolbarGroup.Tab(id, elementName, container, tab, element);
            _toolbarGroups.Add(id, group);

            tab.Groups.Add(group);
            tab.Groups.Sort(static (t1, t2) =>
            {
                var nameComparison = string.Compare(t1.Name, t2.Name, StringComparison.Ordinal);
                return nameComparison != 0 ? nameComparison : t1.ID.CompareTo(t2.ID);
            });

            if (!_viewModel.SelectionsHidden.Contains(group.Name))
            {
                ShowTab(group);
            }

            DisableKeyboardNavigation(element);

            if (_compositionCompleted)
            {
                EnsureActiveGroup();
            }
        }

        internal void RemoveRegistration(int id)
        {
            if (!_toolbarGroups.Remove(id, out var group))
            {
                return;
            }

            HideTab(group);
            group.Group.Groups.Remove(group);
            ReleaseVisualElement(group.View);

            if (_compositionCompleted)
            {
                EnsureActiveGroup();
            }
        }

        internal void CompleteComposition()
        {
            _compositionCompleted = true;
            EnsureActiveGroup();
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _compositionCompleted = false;
            _cameraRefresh.Pause();

            _viewModel.PropertyChanged -= OnPropertyChanged;

            if (!resourcesReleased)
            {
                UnregisterCallback<GeometryChangedEvent, ToolbarView>(OnToolbarGeometryChanged);
                UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
                UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            }

            UnregisterPanelRoot();

            foreach (var group in _toolbarGroups.Values)
            {
                ReleaseVisualElement(group.View);
            }

            _toolbarGroups.Clear();
            _toolbarTabs.Clear();
            _activeGroup = null;

            if (resourcesReleased)
            {
                dataSource = null;
                _filterButton.dataSource = null;
            }
            else
            {
                RemoveFromHierarchy();
                ClearBindingsAndDataSources(this);
                Clear();
            }
        }

        public void ShowRibbon(bool show)
        {
            if (show)
            {
                _showButton.Q<Icon>(ShowIconTargetClassName).RemoveFromClassList(ShowHiddenUssClassName);

                if (_activeGroup != null)
                {
                    if (_activeGroup.Parent.parent == null)
                    {
                        _activeGroup.Parent.AddToTab(this);
                    }
                    else
                    {
                        Assert.IsTrue(this == _activeGroup.Parent.parent);
                    }
                }
            }
            else
            {
                _showButton.Q<Icon>(ShowIconTargetClassName).AddToClassList(ShowHiddenUssClassName);

                if (_activeGroup?.Parent.parent != null)
                {
                    Assert.IsTrue(_activeGroup.Parent.parent == this);
                    _activeGroup.Parent.RemoveFromTab();
                }
            }

            IsRibbonVisible = show;
        }

        public void HideToolbar()
        {
            if (_toolbarHidden)
            {
                return;
            }

            style.display = DisplayStyle.None;
            _toolbarHidden = true;
            _toolbar.SetToolbarHidden(true);
            ResetRestoreClickState();
        }

        public void RestoreToolbar()
        {
            if (!_toolbarHidden)
            {
                return;
            }

            style.display = DisplayStyle.Flex;
            _toolbarHidden = false;
            _toolbar.SetToolbarHidden(false);
            ResetRestoreClickState();
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
            RegisterPanelRoot(evt.destinationPanel?.visualTree);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterPanelRoot();
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
                _menuContainer.Add(group.Button);
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

                if (_activeGroup == group)
                {
                    SetToolbarActive(null);
                }
            }
        }

        private void RegisterPanelRoot(VisualElement root)
        {
            if (ReferenceEquals(_panelRoot, root))
            {
                UpdatePanelSize();
                return;
            }

            UnregisterPanelRoot();
            _panelRoot = root;

            if (_panelRoot == null)
            {
                _anchorApp = null;
                _uiSize = Vector2.zero;
                return;
            }

            _anchorApp = AnchorApp.Current;
            _panelRoot.RegisterCallback<GeometryChangedEvent>(OnPanelRootGeometryChanged);
            _panelRoot.RegisterCallback<PointerDownEvent>(OnRootPointerDown);
            if (_anchorApp != null)
            {
                _anchorApp.ScreenMetricsChanged += OnScreenMetricsChanged;
            }

            UpdatePanelSize();
            ResizeViewRect(contentRect);
        }

        private void UnregisterPanelRoot()
        {
            if (_anchorApp != null)
            {
                _anchorApp.ScreenMetricsChanged -= OnScreenMetricsChanged;
            }

            if (_panelRoot != null && !_panelRoot.resourcesReleased)
            {
                _panelRoot.UnregisterCallback<GeometryChangedEvent>(OnPanelRootGeometryChanged);
                _panelRoot.UnregisterCallback<PointerDownEvent>(OnRootPointerDown);
            }

            ResetCanvasOffsets();
            _hasCameraHeight = false;
            ResetCameraRects();

            if (!resourcesReleased)
            {
                AnchorSafeAreaUtility.ResetPadding(style);
            }

            _anchorApp = null;
            _panelRoot = null;
            _uiSize = Vector2.zero;
        }

        private void OnPanelRootGeometryChanged(GeometryChangedEvent evt)
        {
            UpdatePanelSize();
            ResizeViewRect(contentRect);
        }

        private void OnScreenMetricsChanged(AnchorScreenMetrics metrics)
        {
            UpdatePanelSize();
            ResizeViewRect(contentRect);
        }

        private void UpdatePanelSize()
        {
            _uiSize = GetPanelUiSize();
        }

        private Vector2 GetPanelUiSize()
        {
            var layoutSize = panel.visualTree.layout.size;
            if (float.IsNaN(layoutSize.x) || float.IsNaN(layoutSize.y))
            {
                return Vector2.zero;
            }

            return layoutSize;
        }

        private Button CreateShowButton()
        {
            var button = new Button(() => ShowRibbon(!IsRibbonVisible)) { trailingIcon = "caret-down" };

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
                dataSource = _viewModel,
                sourceItems = _viewModel.FilterItems,
                selectionType = PickerSelectionType.Multiple,
                closeOnSelection = false,
                defaultMessage = string.Empty,
                bindTitle = (item, _) => item.labelElement.text = string.Empty,
                bindItem = (item, i) => item.label = _viewModel.FilterItems[i],
                value = _viewModel.FilterValues,
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
            var button = new Button(HideToolbar) { trailingIcon = "x" };

            button.AddToClassList(MenuButtonClassName);
            button.AddToClassList(ShowUssClassName);
            button.size = Size.S;

            return button;
        }

        private void ResetRestoreClickState()
        {
            _restoreClickCount = 0;
            _lastRestoreClickTime = 0f;
        }

        private void OnRootPointerDown(PointerDownEvent evt)
        {
            // Only track restoration clicks when the toolbar is hidden from view.
            var isHidden = _toolbarHidden || resolvedStyle.display == DisplayStyle.None;
            _toolbarHidden = isHidden;

            if (!isHidden)
            {
                ResetRestoreClickState();
                return;
            }

            var width = _uiSize.x;
            var height = _uiSize.y;

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
                ResetRestoreClickState();
                return;
            }

            if (evt.button != (int)MouseButton.LeftMouse && evt.button != -1)
            {
                return;
            }

            var time = Time.realtimeSinceStartup;

            if (time - _lastRestoreClickTime > RestoreClickResetSeconds)
            {
                _restoreClickCount = 0;
            }

            _lastRestoreClickTime = time;
            _restoreClickCount++;

            if (_restoreClickCount < RestoreClickThreshold)
            {
                return;
            }

            RestoreToolbar();
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
                SetToolbarActive(toolbarTab);

                if (!IsRibbonVisible)
                {
                    ShowRibbon(true);
                }
            };

            return toolbarTab;
        }

        private void SetToolbarActive(ToolbarGroup group, bool updateState = true)
        {
            if (group == _activeGroup)
            {
                if (updateState)
                {
                    _toolbar.SetActiveTab(group?.Name);
                }

                if (IsRibbonVisible)
                {
                    ShowRibbon(true);
                }

                return;
            }

            if (_activeGroup != null)
            {
                _activeGroup.Button.variant = ButtonVariant.Default;

                // something else has already removed it or moved it
                if (_activeGroup.Parent.parent == this)
                {
                    _activeGroup.Parent.RemoveFromTab();
                }

                _activeGroup = null;
            }

            if (group == null)
            {
                if (updateState)
                {
                    _toolbar.SetActiveTab(string.Empty);
                }

                return;
            }

            _activeGroup = group;
            group.Button.variant = ButtonVariant.Accent;

            if (updateState)
            {
                _toolbar.SetActiveTab(group.Name);
            }

            if (IsRibbonVisible)
            {
                ShowRibbon(true);
            }
        }

        private void EnsureActiveGroup()
        {
            if (_activeGroup?.Button.parent != null)
            {
                return;
            }

            if (_toolbarTabs.TryGetValue(_toolbar.ActiveTabName, out var storedGroup) && storedGroup.Button.parent != null)
            {
                SetToolbarActive(storedGroup, false);
                return;
            }

            SetDefaultGroup();
        }

        private void SetDefaultGroup()
        {
            SetToolbarActive(null, false);

            if (_menuContainer.childCount == 0)
            {
                _toolbar.SetActiveTab(string.Empty);
                return;
            }

            var firstButton = (Button)_menuContainer.contentContainer.Children().First();
            var group = _toolbarTabs.First(g => g.Value.Button == firstButton);
            SetToolbarActive(group.Value);
        }

        private void ResizeViewRect(Rect uiRect)
        {
            if (_uiSize.y == 0 || float.IsNaN(uiRect.height))
            {
                return;
            }

            Check.Assume(!float.IsNaN(_uiSize.y));

            AnchorSafeAreaUtility.ApplyPadding(this, this, _panelRoot, AnchorSafeAreaEdges.Top | AnchorSafeAreaEdges.Left | AnchorSafeAreaEdges.Right);

            var cameraHeightNormalized = (_uiSize.y - uiRect.height) / _uiSize.y;
            ResizeCamera(cameraHeightNormalized);
            ResizeCanvas(cameraHeightNormalized);

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
            _cameraHeightNormalized = cameraHeightNormalized;
            _hasCameraHeight = true;

            var cam = Camera.main;
            _cameraList.Clear();

            if (cam == null)
            {
                ResetCameraRects();
                return;
            }

            _cameraList.Add(cam);
            var rect = GetOriginalCameraRect(cam);
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

                    _cameraList.Add(camera);
                    GetOriginalCameraRect(camera);

                    if (camera.rect != rect)
                    {
                        camera.rect = rect;
                    }
                }
            }

            foreach (var camera in _originalCameraRects.Keys.ToArray())
            {
                if (camera == null || !_cameraList.Contains(camera))
                {
                    RestoreCameraRect(camera);
                }
            }
        }

        private void RefreshCameraRects()
        {
            if (_hasCameraHeight)
            {
                ResizeCamera(_cameraHeightNormalized);
            }
        }

        private Rect GetOriginalCameraRect(Camera camera)
        {
            if (!_originalCameraRects.TryGetValue(camera, out var rect))
            {
                rect = camera.rect;
                _originalCameraRects.Add(camera, rect);
            }

            return rect;
        }

        private void ResetCameraRects()
        {
            foreach (var camera in _originalCameraRects.Keys.ToArray())
            {
                RestoreCameraRect(camera);
            }

            _cameraList.Clear();
        }

        private void RestoreCameraRect(Camera camera)
        {
            var rect = _originalCameraRects[camera];
            _originalCameraRects.Remove(camera);

            if (camera != null)
            {
                camera.rect = rect;
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

                var offset = GetOrCreateToolbarOffset(canvas);
                MoveCanvasChildrenToOffset(canvas, offset);

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

            var go = new GameObject("ToolbarOffset", typeof(ToolbarOffset));
            go.transform.SetParent(canvas.transform, false);
            return go.GetComponent<ToolbarOffset>();
        }

        private void MoveCanvasChildrenToOffset(Canvas canvas, ToolbarOffset offset)
        {
            var offsetTransform = offset.transform;

            _transformList.Clear();
            for (var i = 0; i < canvas.transform.childCount; i++)
            {
                var child = canvas.transform.GetChild(i);
                if (child != offsetTransform)
                {
                    _transformList.Add(child);
                }
            }

            foreach (var child in _transformList)
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

                var offset = TryGetToolbarOffset(canvas);
                if (offset == null)
                {
                    continue;
                }

                var offsetTransform = offset.transform;
                _transformList.Clear();
                for (var i = 0; i < offsetTransform.childCount; i++)
                {
                    _transformList.Add(offsetTransform.GetChild(i));
                }

                foreach (var child in _transformList)
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
                _filterButton.value = _viewModel.FilterValues; // Can't rely on binding to have updated in time
                _filterButton.Refresh();
            }
            else if (e.PropertyName == nameof(ToolbarViewModel.FilterValues))
            {
                foreach (var tabGroup in _toolbarTabs.ToArray())
                {
                    foreach (var t in tabGroup.Value.Groups)
                    {
                        if (_viewModel.SelectionsHidden.Contains(t.Name))
                        {
                            HideTab(t);
                        }
                        else
                        {
                            ShowTab(t);
                        }
                    }
                }

                if (_compositionCompleted)
                {
                    EnsureActiveGroup();
                }
            }
        }
    }
}
