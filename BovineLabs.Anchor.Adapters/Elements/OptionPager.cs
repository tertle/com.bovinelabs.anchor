// <copyright file="OptionPager.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System.Collections;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;
    using UnityEngine.UIElements;

    /// <summary>
    /// Alternative to a dropdown that lets users cycle through options with previous/next buttons.
    /// </summary>
    [MovedFrom(true, "BovineLabs.Anchor.Elements", "BovineLabs.Anchor")]
    [UxmlElement]
    public partial class OptionPager : VisualElement
    {
        public const string UssClassName = "bl-option-pager";
        public const string ControlsUssClassName = UssClassName + "__controls";
        public const string PreviousButtonUssClassName = UssClassName + "__previous";
        public const string CenterUssClassName = UssClassName + "__center";
        public const string ValueUssClassName = UssClassName + "__value";
        public const string NextButtonUssClassName = UssClassName + "__next";
        public const string IndicatorUssClassName = UssClassName + "__indicator";

        private static readonly BindingId SourceItemsProperty = nameof(sourceItems);
        private static readonly BindingId OptionsCountProperty = nameof(optionsCount);
        private static readonly BindingId BindItemProperty = nameof(bindItem);
        private static readonly BindingId SelectedIndexProperty = nameof(selectedIndex);
        private static readonly BindingId SelectedTextProperty = nameof(selectedText);
        private static readonly BindingId WrapProperty = nameof(wrap);
        private static readonly BindingId EmptyTextProperty = nameof(emptyText);
        private static readonly BindingId PreviousButtonIconProperty = nameof(previousButtonIcon);
        private static readonly BindingId NextButtonIconProperty = nameof(nextButtonIcon);
        private static readonly BindingId ShowIndicatorProperty = nameof(showIndicator);

        private readonly ActionButton m_previousButton;
        private readonly DropdownItem m_selectedItemElement;
        private readonly ActionButton m_nextButton;
        private readonly PageIndicator m_pageIndicator;

        private Dropdown.BindItemFunc m_bindItem;
        private IList m_sourceItems;
        private int m_selectedIndex = -1;
        private int m_desiredSelectedIndex = -1;
        private string m_selectedText = string.Empty;
        private bool m_wrap = true;
        private string m_emptyText = string.Empty;
        private string m_previousButtonIcon = "caret-left";
        private string m_nextButtonIcon = "caret-right";
        private bool m_showIndicator = true;

        /// <summary>Initializes a new instance of the <see cref="OptionPager"/> class.</summary>
        public OptionPager()
        {
            this.AddToClassList(UssClassName);
            this.style.flexDirection = FlexDirection.Column;
            this.style.alignItems = Align.Stretch;

            var controls = new VisualElement();
            controls.AddToClassList(ControlsUssClassName);
            controls.style.flexDirection = FlexDirection.Row;
            controls.style.alignItems = Align.Center;
            controls.style.justifyContent = Justify.SpaceBetween;
            controls.style.width = Length.Percent(100);

            this.m_previousButton = new ActionButton(this.SelectPrevious)
            {
                icon = this.m_previousButtonIcon,
                label = null,
                quiet = true,
            };
            this.m_previousButton.AddToClassList(PreviousButtonUssClassName);

            this.m_selectedItemElement = new DropdownItem
            {
                label = this.m_emptyText,
                pickingMode = PickingMode.Ignore,
            };
            this.m_selectedItemElement.AddToClassList(ValueUssClassName);
            this.m_selectedItemElement.style.justifyContent = Justify.Center;
            this.m_selectedItemElement.style.flexShrink = 1;
            this.m_selectedItemElement.style.flexGrow = 1;
            this.m_selectedItemElement.style.minWidth = 0;
            this.m_selectedItemElement.style.maxWidth = Length.Percent(100);
            this.m_selectedItemElement.labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;

            this.m_nextButton = new ActionButton(this.SelectNext)
            {
                icon = this.m_nextButtonIcon,
                label = null,
                quiet = true,
            };
            this.m_nextButton.AddToClassList(NextButtonUssClassName);

            var center = new VisualElement();
            center.AddToClassList(CenterUssClassName);
            center.style.flexDirection = FlexDirection.Column;
            center.style.alignItems = Align.Stretch;
            center.style.justifyContent = Justify.Center;
            center.style.flexGrow = 1;
            center.style.flexShrink = 1;
            center.style.minWidth = 0;

            this.m_pageIndicator = new PageIndicator();
            this.m_pageIndicator.AddToClassList(IndicatorUssClassName);
            this.m_pageIndicator.style.alignSelf = Align.Center;
            this.m_pageIndicator.RegisterValueChangedCallback(this.OnPageIndicatorValueChanged);

            center.Add(this.m_selectedItemElement);
            center.Add(this.m_pageIndicator);

            controls.Add(this.m_previousButton);
            controls.Add(center);
            controls.Add(this.m_nextButton);

            this.Add(controls);

            this.ApplySelection(sendChangeEvent: false, notifyBindings: false);
        }

        /// <summary>Gets the previous navigation button.</summary>
        public ActionButton PreviousButton => this.m_previousButton;

        /// <summary>Gets the next navigation button.</summary>
        public ActionButton NextButton => this.m_nextButton;

        /// <summary>Gets the dropdown item element that renders the selected option.</summary>
        public DropdownItem SelectedItemElement => this.m_selectedItemElement;

        /// <summary>Gets the page indicator used to visualize selection position.</summary>
        public PageIndicator Indicator => this.m_pageIndicator;

        /// <summary>
        /// Gets or sets the source items collection.
        /// When <see cref="bindItem"/> is null, each item is rendered using <see cref="object.ToString"/>.
        /// </summary>
        [CreateProperty]
        public IList sourceItems
        {
            get => this.m_sourceItems;
            set
            {
                var changed = !ReferenceEquals(this.m_sourceItems, value);
                var previousCount = this.optionsCount;
                this.m_sourceItems = value;

                this.ApplySelection(sendChangeEvent: true, notifyBindings: true);

                if (changed)
                {
                    this.NotifyPropertyChanged(in SourceItemsProperty);
                }

                if (previousCount != this.optionsCount)
                {
                    this.NotifyPropertyChanged(in OptionsCountProperty);
                }
            }
        }

        /// <summary>Gets the number of available options.</summary>
        [CreateProperty]
        public int optionsCount => this.m_sourceItems?.Count ?? 0;

        /// <summary>
        /// Gets or sets the function used to bind the selected source item to the center text.
        /// </summary>
        [CreateProperty]
        public Dropdown.BindItemFunc bindItem
        {
            get => this.m_bindItem;
            set
            {
                if (this.m_bindItem == value)
                {
                    return;
                }

                this.m_bindItem = value;
                this.ApplySelection(sendChangeEvent: false, notifyBindings: true);
                this.NotifyPropertyChanged(in BindItemProperty);
            }
        }

        /// <summary>
        /// Gets or sets the selected index. Values are clamped into the current options range.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int selectedIndex
        {
            get => this.m_selectedIndex;
            set
            {
                if (this.m_desiredSelectedIndex == value)
                {
                    return;
                }

                this.m_desiredSelectedIndex = value;
                this.ApplySelection(sendChangeEvent: true, notifyBindings: true);
            }
        }

        /// <summary>Gets the display text of the selected option.</summary>
        [CreateProperty]
        public string selectedText => this.m_selectedText;

        /// <summary>
        /// Gets or sets a value indicating whether next/previous wraps around at the list bounds.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool wrap
        {
            get => this.m_wrap;
            set
            {
                if (this.m_wrap == value)
                {
                    return;
                }

                this.m_wrap = value;
                this.RefreshButtonStates();
                this.NotifyPropertyChanged(in WrapProperty);
            }
        }

        /// <summary>
        /// Gets or sets the text shown when there is no available option.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string emptyText
        {
            get => this.m_emptyText;
            set
            {
                value ??= string.Empty;

                if (this.m_emptyText == value)
                {
                    return;
                }

                this.m_emptyText = value;
                this.ApplySelection(sendChangeEvent: false, notifyBindings: true);
                this.NotifyPropertyChanged(in EmptyTextProperty);
            }
        }

        /// <summary>
        /// Gets or sets the previous button icon.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string previousButtonIcon
        {
            get => this.m_previousButtonIcon;
            set
            {
                value ??= string.Empty;

                if (this.m_previousButtonIcon == value)
                {
                    return;
                }

                this.m_previousButtonIcon = value;
                this.m_previousButton.icon = value;
                this.NotifyPropertyChanged(in PreviousButtonIconProperty);
            }
        }

        /// <summary>
        /// Gets or sets the next button icon.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string nextButtonIcon
        {
            get => this.m_nextButtonIcon;
            set
            {
                value ??= string.Empty;

                if (this.m_nextButtonIcon == value)
                {
                    return;
                }

                this.m_nextButtonIcon = value;
                this.m_nextButton.icon = value;
                this.NotifyPropertyChanged(in NextButtonIconProperty);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the page indicator is visible.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public bool showIndicator
        {
            get => this.m_showIndicator;
            set
            {
                if (this.m_showIndicator == value)
                {
                    return;
                }

                this.m_showIndicator = value;
                this.m_pageIndicator.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                this.NotifyPropertyChanged(in ShowIndicatorProperty);
            }
        }

        private void OnPageIndicatorValueChanged(ChangeEvent<int> evt)
        {
            this.selectedIndex = evt.newValue;
        }

        private void SelectPrevious()
        {
            var count = this.optionsCount;
            if (count == 0)
            {
                return;
            }

            if (this.wrap)
            {
                var next = this.m_selectedIndex <= 0 ? count - 1 : this.m_selectedIndex - 1;
                this.selectedIndex = next;
                return;
            }

            if (this.m_selectedIndex > 0)
            {
                this.selectedIndex = this.m_selectedIndex - 1;
            }
        }

        private void SelectNext()
        {
            var count = this.optionsCount;
            if (count == 0)
            {
                return;
            }

            if (this.wrap)
            {
                var next = this.m_selectedIndex >= count - 1 ? 0 : this.m_selectedIndex + 1;
                this.selectedIndex = next;
                return;
            }

            if (this.m_selectedIndex < count - 1)
            {
                this.selectedIndex = this.m_selectedIndex + 1;
            }
        }

        private void ApplySelection(bool sendChangeEvent, bool notifyBindings)
        {
            var previousIndex = this.m_selectedIndex;
            var previousText = this.m_selectedText;

            this.m_selectedIndex = this.ResolveIndex(this.m_desiredSelectedIndex);
            this.BindSelectedItem();

            this.m_pageIndicator.count = this.optionsCount;
            if (this.m_selectedIndex >= 0)
            {
                this.m_pageIndicator.SetValueWithoutNotify(this.m_selectedIndex);
            }

            this.RefreshButtonStates();

            if (sendChangeEvent && previousIndex != this.m_selectedIndex)
            {
                using var evt = ChangeEvent<int>.GetPooled(previousIndex, this.m_selectedIndex);
                evt.target = this;
                this.SendEvent(evt);
            }

            if (!notifyBindings)
            {
                return;
            }

            if (previousIndex != this.m_selectedIndex)
            {
                this.NotifyPropertyChanged(in SelectedIndexProperty);
            }

            if (previousText != this.m_selectedText)
            {
                this.NotifyPropertyChanged(in SelectedTextProperty);
            }
        }

        private void RefreshButtonStates()
        {
            var count = this.optionsCount;
            var canMovePrevious = count > 0 && (this.wrap || this.m_selectedIndex > 0);
            var canMoveNext = count > 0 && (this.wrap || this.m_selectedIndex < count - 1);

            this.m_previousButton.SetEnabled(canMovePrevious);
            this.m_nextButton.SetEnabled(canMoveNext);
        }

        private int ResolveIndex(int desiredIndex)
        {
            var count = this.optionsCount;
            if (count <= 0)
            {
                return -1;
            }

            if (desiredIndex < 0)
            {
                return 0;
            }

            if (desiredIndex >= count)
            {
                return count - 1;
            }

            return desiredIndex;
        }

        private string GetOptionText(int index)
        {
            if (index < 0 || index >= this.optionsCount)
            {
                return this.emptyText;
            }

            return this.m_sourceItems[index]?.ToString() ?? string.Empty;
        }

        private void BindSelectedItem()
        {
            if (this.m_selectedIndex < 0 || this.m_selectedIndex >= this.optionsCount)
            {
                this.m_selectedItemElement.icon = null;
                this.m_selectedItemElement.label = this.emptyText;
                this.m_selectedText = this.m_selectedItemElement.labelElement.text ?? string.Empty;
                return;
            }

            if (this.bindItem != null)
            {
                this.bindItem.Invoke(this.m_selectedItemElement, this.m_selectedIndex);
            }
            else
            {
                this.m_selectedItemElement.icon = null;
                this.m_selectedItemElement.label = this.GetOptionText(this.m_selectedIndex);
            }

            this.m_selectedText = this.m_selectedItemElement.labelElement.text ?? string.Empty;
        }
    }
}
