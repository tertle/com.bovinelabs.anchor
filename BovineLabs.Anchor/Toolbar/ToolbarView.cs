// <copyright file="ToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if !BL_DISABLE_TOOLBAR && (BL_DEBUG || UNITY_EDITOR)
namespace BovineLabs.Anchor.Toolbar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.UI;
    using Unity.Burst;
    using Unity.Collections;
    using Unity.Mathematics;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;
    using Button = Unity.AppUI.UI.Button;

    /// <summary> Burst data separate to avoid compiling issues with static variables. </summary>
    public static class ToolbarViewrData
    {
        public static readonly SharedStatic<FixedString32Bytes> ActiveTab = SharedStatic<FixedString32Bytes>.GetOrCreate<ActiveTabVar>();

        private struct ActiveTabVar
        {
        }
    }

    [Preserve]
    public class ToolbarView : VisualElement, IViewRoot
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

        // [ConfigVar("debug.toolbar", false, "Should the toolbar be hidden", true)]
        private static readonly SharedStatic<bool> Hide = SharedStatic<bool>.GetOrCreate<ToolbarView, EnabledVar>();

        private readonly Dictionary<string, ToolbarGroup> toolbarTabs = new();
        private readonly Dictionary<int, ToolbarGroup.Tab> toolbarGroups = new();
        private readonly FilterBind filterBind;
        private readonly IServiceProvider serviceProvider;
        private readonly VisualElement menuContainer;
        private readonly Button showButton;

        private Panel appPanel;
        private ToolbarGroup activeGroup;

        private float uiHeight;
        private bool showRibbon;
        private int key;

        public ToolbarView(IServiceProvider serviceProvider, ILocalStorageService storageService)
        {
            Instance = this;

            this.filterBind = new FilterBind(storageService);

            this.serviceProvider = serviceProvider;
            this.AddToClassList(UssClassName);

            var menu = new VisualElement();
            menu.AddToClassList(MenuUssClassName);

            this.showButton = this.CreateShowButton();
            var filterButton = this.CreateFilterButton();

            this.menuContainer = new VisualElement();
            this.menuContainer.AddToClassList(MenuContainerUssClassName);

            menu.Add(this.showButton);
            menu.Add(filterButton);
            menu.Add(this.menuContainer);

            this.Add(menu);

            this.uiHeight = Screen.height; // TODO

            if (Hide.Data)
            {
                this.style.display = DisplayStyle.None;
            }

            this.RegisterCallback<GeometryChangedEvent>(evt => this.ResizeViewRect(evt.newRect));

            foreach (var t in ReflectionUtility.GetAllWithAttribute<AutoToolbarAttribute>())
            {
                var attr = t.GetCustomAttribute<AutoToolbarAttribute>();
                var tabName = attr.TabName ?? "Service";
                this.AddGroup(t, tabName, attr.ElementName, out _, out _);
            }

            this.SetDefaultGroup();
        }

        public static ToolbarView Instance { get; private set; }

        public int Priority => -1000;

        object IView.ViewModel => null;

        /// <inheritdoc/>
        void IViewRoot.AttachedToPanel(Panel value)
        {
            this.appPanel = value;
            this.appPanel.RegisterCallback<GeometryChangedEvent>(this.OnRootContentChanged);
        }

        public void AddGroup<T>(string tabName, string groupName, out int id, out T view)
            where T : VisualElement, IView
        {
            this.AddGroup(typeof(T), tabName, groupName, out id, out var viewElement);
            view = (T)viewElement;
        }

        public void AddGroup(Type viewType, string tabName, string elementName, out int id, out VisualElement view)
        {
            if (!typeof(VisualElement).IsAssignableFrom(viewType))
            {
                throw new ArgumentException($"{viewType} is not a {nameof(VisualElement)}", nameof(viewType));
            }

            id = ++this.key;

            view = (VisualElement)this.serviceProvider.GetService(viewType);

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

            this.filterBind.AddSelection(group.Name);

            if (!this.filterBind.SelectionsHidden.Contains(group.Name))
            {
                this.ShowGroup(group);
            }
        }

        public IBindingObject RemoveGroup(int id)
        {
            if (!this.toolbarGroups.Remove(id, out var group))
            {
                return null;
            }

            this.filterBind.RemoveSelection(group.Name);

            this.HideGroup(group);
            group.Group.Groups.Remove(group);

            return ((IView)group.View).ViewModel as IBindingObject;
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

        private void ShowGroup(ToolbarGroup.Tab tab)
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

        private void HideGroup(ToolbarGroup.Tab tab)
        {
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
            var height = this.appPanel.contentRect.height;

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
                leadingIcon = "caret-down",
            };

            button.AddToClassList(MenuButtonClassName);
            button.AddToClassList(ShowUssClassName);
            button.Q<Icon>("appui-button__leadingicon").AddToClassList(ShowIconUssClassName);

            return button;
        }

        private Dropdown CreateFilterButton()
        {
            var dropdown = new Dropdown
            {
                dataSource = this.filterBind,
                selectionType = PickerSelectionType.Multiple,
                closeOnSelection = false,
                defaultMessage = string.Empty,
                bindTitle = (item, _) => item.labelElement.text = string.Empty,
            };

            dropdown.SetBinding(nameof(Dropdown.sourceItems), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(FilterBind.SourceItems)),
            });
            dropdown.SetBinding(nameof(Dropdown.bindItem), new DataBinding
            {
                bindingMode = BindingMode.ToTarget,
                dataSourcePath = new PropertyPath(nameof(FilterBind.BindItem)),
            });
            dropdown.SetBinding(nameof(Dropdown.value), new DataBinding { dataSourcePath = new PropertyPath(nameof(FilterBind.Value)) });

            dropdown.AddToClassList(MenuButtonClassName);
            dropdown.AddToClassList(FilterUssClassName);

            this.filterBind.ValueChanged += this.FilterBindOnValueChanged;
            this.filterBind.SourceChanged += _ => dropdown.Refresh();

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
                ToolbarViewrData.ActiveTab.Data = string.Empty;
            }

            if (group == null)
            {
                return;
            }

            this.activeGroup = group;
            ToolbarViewrData.ActiveTab.Data = group.Name;
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
                this.showButton.Q<Icon>("appui-button__leadingicon").RemoveFromClassList(ShowHiddenUssClassName);

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
                this.showButton.Q<Icon>("appui-button__leadingicon").AddToClassList(ShowHiddenUssClassName);

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
        }

        private void FilterBindOnValueChanged(FilterBind sender, (IReadOnlyList<int> Added, IReadOnlyList<int> Removed) e)
        {
            foreach (var added in e.Added)
            {
                var s = sender.SourceItems[added];
                foreach (var g in this.toolbarTabs.SelectMany(t => t.Value.Groups.Where(g => g.Name == s)))
                {
                    this.ShowGroup(g);
                }
            }

            foreach (var removed in e.Removed)
            {
                var s = sender.SourceItems[removed];
                foreach (var g in this.toolbarTabs.SelectMany(t => t.Value.Groups.Where(g => g.Name == s)))
                {
                    this.HideGroup(g);
                }
            }
        }

        private struct EnabledVar
        {
        }

        private class FilterBind : INotifyBindablePropertyChanged
        {
            private const string SelectionKey = "bl.toolbarmanager.filter.selections";

            private readonly List<int> value = new();
            private readonly HashSet<int> newValues = new();

            private readonly List<int> added = new();
            private readonly List<int> removed = new();

            private readonly Dictionary<string, int> selectionsCount = new();
            private readonly HashSet<string> selectionsHidden;

            private readonly List<string> sourceItems = new();

            private readonly ILocalStorageService storageService;

            public FilterBind(ILocalStorageService storageService)
            {
                this.storageService = storageService;
                var selectionSaved = storageService.GetValue(SelectionKey, string.Empty);
                var selectionArray = selectionSaved.Split(",");
                this.selectionsHidden = new HashSet<string>(selectionArray);
                this.selectionsHidden.Remove(string.Empty);
            }

            public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;

            public event Action<FilterBind, (IReadOnlyList<int> Added, IReadOnlyList<int> Removed)> ValueChanged;

            public event Action<FilterBind> SourceChanged;

            public IReadOnlyCollection<string> SelectionsHidden => this.selectionsHidden;

            [CreateProperty]
            public List<string> SourceItems => this.sourceItems;

            [CreateProperty]
            public IEnumerable<int> Value
            {
                get => this.value;
                set
                {
                    this.added.Clear();
                    this.removed.Clear();
                    this.newValues.Clear();

                    this.newValues.UnionWith(value);

                    foreach (var oldValue in this.value.Where(oldValue => !this.newValues.Contains(oldValue)))
                    {
                        this.removed.Add(oldValue);
                        this.selectionsHidden.Add(this.sourceItems[oldValue]);
                    }

                    foreach (var newValue in this.newValues.Where(newValue => !this.value.Contains(newValue)))
                    {
                        this.added.Add(newValue);
                        this.selectionsHidden.Remove(this.sourceItems[newValue]);
                    }

                    this.value.Clear();
                    this.value.AddRange(value);

                    var serializedString = string.Join(",", this.selectionsHidden);
                    this.storageService.SetValue(SelectionKey, serializedString);

                    this.ValueChanged?.Invoke(this, (this.added, this.removed));
                    this.propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(this.Value)));
                }
            }

            [CreateProperty]
            public Action<DropdownItem, int> BindItem => (item, i) => item.label = this.sourceItems[i];

            public void AddSelection(string filterName)
            {
                this.selectionsCount.TryGetValue(filterName, out var count);
                if (count == 0)
                {
                    this.sourceItems.Add(filterName);
                    this.sourceItems.Sort();

                    this.UpdateValue();
                    this.UpdateSelections();
                }

                this.selectionsCount[filterName] = count + 1;
            }

            public void RemoveSelection(string filterName)
            {
                if (!this.selectionsCount.TryGetValue(filterName, out var currentValue))
                {
                    return;
                }

                currentValue--;
                if (currentValue == 0)
                {
                    this.selectionsCount.Remove(filterName);
                    this.sourceItems.Remove(filterName);
                    this.UpdateValue();
                    this.UpdateSelections();
                }
                else
                {
                    this.selectionsCount[filterName] = currentValue;
                }
            }

            private void UpdateValue()
            {
                this.value.Clear();

                for (var i = 0; i < this.sourceItems.Count; i++)
                {
                    var selection = this.sourceItems[i];
                    if (!this.SelectionsHidden.Contains(selection))
                    {
                        this.value.Add(i);
                    }
                }

                this.propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(this.Value)));
            }

            private void UpdateSelections()
            {
                this.propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(nameof(this.SourceItems)));
                this.SourceChanged?.Invoke(this);
            }
        }
    }
}
#endif
