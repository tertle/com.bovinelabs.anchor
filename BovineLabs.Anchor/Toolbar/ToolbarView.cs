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
    using Unity.AppUI.MVVM;
    using Unity.AppUI.UI;
    using Unity.Burst;
    using Unity.Mathematics;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

#if BL_CORE
    [BovineLabs.Core.ConfigVars.Configurable]
#endif
    [Preserve]
    public class ToolbarView : VisualElement, IView
    {
        public const float DefaultUpdateRate = 1 / 4f;

        /// <summary>
        /// The NavigationScreen main styling class.
        /// </summary>
        public const string UssClassName = "bl-toolbar";

        public const string MenuButtonClassName = UssClassName + "__button";

        /// <summary> The Toolbar background styling class. </summary>
        public const string MenuUssClassName = UssClassName + "__menu";

        /// <summary> The Toolbar show button styling class. </summary>
        public const string ShowUssClassName = UssClassName + "__show";

        /// <summary> The Toolbar filter button styling class. </summary>
        public const string FilterUssClassName = UssClassName + "__filter";

        /// <summary> The Toolbar container styling class. </summary>
        public const string MenuContainerUssClassName = UssClassName + "__menu-container";

        public const string MenuButtonUssClassName = UssClassName + "__menu-button";

        public const string ShowIconUssClassName = ShowUssClassName + "__icon";

        public const string ShowHiddenUssClassName = ShowIconUssClassName + "-hidden";

        private const string ShowIconTargetClass = "appui-button__trailingicon";

#if BL_CORE
        [BovineLabs.Core.ConfigVars.ConfigVar("debug.toolbar", true, "Should the toolbar be shown", true)]
        private static readonly SharedStatic<bool> Show = SharedStatic<bool>.GetOrCreate<ToolbarView, EnabledVar>();
#endif

        private readonly Dictionary<string, ToolbarGroup> toolbarTabs = new();
        private readonly Dictionary<int, ToolbarGroup.Tab> toolbarGroups = new();
        private readonly ToolbarViewModel viewModel;
        private readonly VisualElement menuContainer;
        private readonly Dropdown filterButton;
        private readonly Button showButton;

        private ToolbarGroup activeGroup;

        private float uiHeight;
        private bool showRibbon;
        private int key;

        public ToolbarView(ToolbarViewModel viewModel)
        {
            Instance = this;

            this.viewModel = viewModel;

            this.AddToClassList(UssClassName);

            var menu = new VisualElement();
            menu.AddToClassList(MenuUssClassName);

            this.showButton = this.CreateShowButton();
            this.filterButton = this.CreateFilterButton();

            this.menuContainer = new VisualElement();
            this.menuContainer.AddToClassList(MenuContainerUssClassName);

            menu.Add(this.showButton);
            menu.Add(this.filterButton);
            menu.Add(this.menuContainer);

            this.Add(menu);

            this.uiHeight = Screen.height; // TODO

            this.RegisterCallback<GeometryChangedEvent>(evt => this.ResizeViewRect(evt.newRect));
            this.viewModel.PropertyChanged += this.OnPropertyChanged;

            foreach (var t in Core.GetAllWithAttribute<AutoToolbarAttribute>())
            {
                var attr = t.GetCustomAttribute<AutoToolbarAttribute>();
                var tabName = attr.TabName ?? "Service";

                this.AddTab(t, tabName, attr.ElementName, out _, out _);
            }

            this.SetDefaultGroup();

#if BL_CORE
            if (!Show.Data)
            {
                this.style.display = DisplayStyle.None;
            }
#endif

            this.RegisterCallback<GeometryChangedEvent>(this.OnRootContentChanged);

            App.shuttingDown += this.AppOnShuttingDown;
        }

        public static ToolbarView Instance { get; private set; }

        public void AddTab<T>(string tabName, string elementName, out int id, out T view)
            where T : VisualElement, IView
        {
            this.AddTab(typeof(T), tabName, elementName, out id, out var visualElement);
            view = (T)visualElement;
        }

        public void AddTab(Type viewType, string tabName, string elementName, out int id, out VisualElement view)
        {
            if (!typeof(VisualElement).IsAssignableFrom(viewType))
            {
                throw new ArgumentException($"{viewType} is not a {nameof(VisualElement)}", nameof(viewType));
            }

            if (!typeof(IView).IsAssignableFrom(viewType))
            {
                throw new ArgumentException($"{viewType} is not a {nameof(IView)}", nameof(viewType));
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
        }

        public T RemoveTab<T>(int id)
            where T : VisualElement, IView
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

        public VisualElement GetPanel(int id)
        {
            return this.toolbarGroups.TryGetValue(id, out var group) ? group.View : null;
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
                Instance = null;
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

        private void OnRootContentChanged(GeometryChangedEvent evt)
        {
            var height = App.current.rootVisualElement.contentRect.height;

            if (math.abs(this.uiHeight - height) > float.Epsilon)
            {
                this.uiHeight = height;
                this.ResizeViewRect(this.contentRect);
            }
        }

        private Button CreateShowButton()
        {
            var button = new Button(() => this.ShowRibbon(!this.showRibbon))
            {
                trailingIcon = "caret-down",
            };

            button.AddToClassList(MenuButtonClassName);
            button.AddToClassList(ShowUssClassName);
            button.Q<Icon>(ShowIconTargetClass).AddToClassList(ShowIconUssClassName);
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

        private ToolbarGroup CreateTab(string tabName)
        {
            var button = new Button { title = tabName };
            button.AddToClassList(MenuButtonUssClassName);

            var contents = new ToolbarGroupElement();

            var toolbarTab = new ToolbarGroup(tabName, button, contents);

            button.clicked += () => this.SetToolbarActive(toolbarTab);

            this.SetToolbarActive(toolbarTab);

            return toolbarTab;
        }

        private void SetToolbarActive(ToolbarGroup group)
        {
            if (group == this.activeGroup)
            {
                this.ShowRibbon(true);

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

                this.activeGroup = default;
                ToolbarViewData.ActiveTab.Data = string.Empty;
            }

            if (group == null)
            {
                return;
            }

            this.activeGroup = group;
            ToolbarViewData.ActiveTab.Data = group.Name;
            group.Button.variant = ButtonVariant.Accent;

            this.ShowRibbon(true);
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

            this.showRibbon = show;
        }

        private void ResizeViewRect(Rect uiRect)
        {
            if (this.uiHeight == 0)
            {
                return;
            }

            var cameraHeightNormalized = (this.uiHeight - uiRect.height) / this.uiHeight;

            var cam = Camera.main;
            if (cam != null)
            {
                var rect = cam.rect;
                rect.height = cameraHeightNormalized;
                cam.rect = rect;
            }

            if (AnchorApp.current.PopupContainer != null)
            {
                AnchorApp.current.PopupContainer.style.top = uiRect.height;
            }

            if (AnchorApp.current.NotificationContainer != null)
            {
                AnchorApp.current.NotificationContainer.style.top = uiRect.height;
            }

            if (AnchorApp.current.TooltipContainer != null)
            {
                AnchorApp.current.TooltipContainer.style.top = uiRect.height;
            }
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
