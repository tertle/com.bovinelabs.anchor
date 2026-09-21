namespace BovineLabs.Anchor.Elements
{
    using System.Collections;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.UIElements;

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

        private readonly ActionButton _previousButton;
        private readonly DropdownItem _selectedItemElement;
        private readonly ActionButton _nextButton;
        private readonly PageIndicator _pageIndicator;

        private Dropdown.BindItemFunc _bindItem;
        private IList _sourceItems;
        private int _selectedIndex = -1;
        private int _desiredSelectedIndex = -1;
        private string _selectedText = string.Empty;
        private bool _wrap = true;
        private string _emptyText = string.Empty;
        private string _previousButtonIcon = "caret-left";
        private string _nextButtonIcon = "caret-right";
        private bool _showIndicator = true;

        public OptionPager()
        {
            AddToClassList(UssClassName);
            style.flexDirection = FlexDirection.Column;
            style.alignItems = Align.Stretch;

            var controls = new VisualElement();
            controls.AddToClassList(ControlsUssClassName);
            controls.style.flexDirection = FlexDirection.Row;
            controls.style.alignItems = Align.Center;
            controls.style.justifyContent = Justify.SpaceBetween;
            controls.style.width = Length.Percent(100);

            _previousButton = new ActionButton(SelectPrevious)
            {
                icon = _previousButtonIcon,
                label = null,
                quiet = true,
            };
            _previousButton.AddToClassList(PreviousButtonUssClassName);

            _selectedItemElement = new DropdownItem
            {
                label = _emptyText,
                pickingMode = PickingMode.Ignore,
            };
            _selectedItemElement.AddToClassList(ValueUssClassName);
            _selectedItemElement.style.justifyContent = Justify.Center;
            _selectedItemElement.style.flexShrink = 1;
            _selectedItemElement.style.flexGrow = 1;
            _selectedItemElement.style.minWidth = 0;
            _selectedItemElement.style.maxWidth = Length.Percent(100);
            _selectedItemElement.labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;

            _nextButton = new ActionButton(SelectNext)
            {
                icon = _nextButtonIcon,
                label = null,
                quiet = true,
            };
            _nextButton.AddToClassList(NextButtonUssClassName);

            var center = new VisualElement();
            center.AddToClassList(CenterUssClassName);
            center.style.flexDirection = FlexDirection.Column;
            center.style.alignItems = Align.Stretch;
            center.style.justifyContent = Justify.Center;
            center.style.flexGrow = 1;
            center.style.flexShrink = 1;
            center.style.minWidth = 0;

            _pageIndicator = new PageIndicator();
            _pageIndicator.AddToClassList(IndicatorUssClassName);
            _pageIndicator.style.alignSelf = Align.Center;
            _pageIndicator.RegisterValueChangedCallback(OnPageIndicatorValueChanged);

            center.Add(_selectedItemElement);
            center.Add(_pageIndicator);

            controls.Add(_previousButton);
            controls.Add(center);
            controls.Add(_nextButton);

            Add(controls);

            ApplySelection(sendChangeEvent: false, notifyBindings: false);
        }

        public ActionButton PreviousButton => _previousButton;

        public ActionButton NextButton => _nextButton;

        public DropdownItem SelectedItemElement => _selectedItemElement;

        public PageIndicator Indicator => _pageIndicator;

        /// <summary>
        /// Without a bindItem callback, items render using object.ToString.
        /// </summary>
        [CreateProperty]
        public IList sourceItems
        {
            get => _sourceItems;
            set
            {
                var changed = !ReferenceEquals(_sourceItems, value);
                var previousCount = optionsCount;
                _sourceItems = value;

                ApplySelection(sendChangeEvent: true, notifyBindings: true);

                if (changed)
                {
                    NotifyPropertyChanged(in SourceItemsProperty);
                }

                if (previousCount != optionsCount)
                {
                    NotifyPropertyChanged(in OptionsCountProperty);
                }
            }
        }

        [CreateProperty]
        public int optionsCount => _sourceItems?.Count ?? 0;

        [CreateProperty]
        public Dropdown.BindItemFunc bindItem
        {
            get => _bindItem;
            set
            {
                if (_bindItem == value)
                {
                    return;
                }

                _bindItem = value;
                ApplySelection(sendChangeEvent: false, notifyBindings: true);
                NotifyPropertyChanged(in BindItemProperty);
            }
        }

        /// <summary>
        /// Clamped to the current options range.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public int selectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_desiredSelectedIndex == value)
                {
                    return;
                }

                _desiredSelectedIndex = value;
                ApplySelection(sendChangeEvent: true, notifyBindings: true);
            }
        }

        [CreateProperty]
        public string selectedText => _selectedText;

        [CreateProperty]
        [UxmlAttribute]
        public bool wrap
        {
            get => _wrap;
            set
            {
                if (_wrap == value)
                {
                    return;
                }

                _wrap = value;
                RefreshButtonStates();
                NotifyPropertyChanged(in WrapProperty);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public string emptyText
        {
            get => _emptyText;
            set
            {
                value ??= string.Empty;

                if (_emptyText == value)
                {
                    return;
                }

                _emptyText = value;
                ApplySelection(sendChangeEvent: false, notifyBindings: true);
                NotifyPropertyChanged(in EmptyTextProperty);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public string previousButtonIcon
        {
            get => _previousButtonIcon;
            set
            {
                value ??= string.Empty;

                if (_previousButtonIcon == value)
                {
                    return;
                }

                _previousButtonIcon = value;
                _previousButton.icon = value;
                NotifyPropertyChanged(in PreviousButtonIconProperty);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public string nextButtonIcon
        {
            get => _nextButtonIcon;
            set
            {
                value ??= string.Empty;

                if (_nextButtonIcon == value)
                {
                    return;
                }

                _nextButtonIcon = value;
                _nextButton.icon = value;
                NotifyPropertyChanged(in NextButtonIconProperty);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public bool showIndicator
        {
            get => _showIndicator;
            set
            {
                if (_showIndicator == value)
                {
                    return;
                }

                _showIndicator = value;
                _pageIndicator.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
                NotifyPropertyChanged(in ShowIndicatorProperty);
            }
        }

        private void OnPageIndicatorValueChanged(ChangeEvent<int> evt)
        {
            selectedIndex = evt.newValue;
        }

        private void SelectPrevious()
        {
            var count = optionsCount;
            if (count == 0)
            {
                return;
            }

            if (wrap)
            {
                var next = _selectedIndex <= 0 ? count - 1 : _selectedIndex - 1;
                selectedIndex = next;
                return;
            }

            if (_selectedIndex > 0)
            {
                selectedIndex = _selectedIndex - 1;
            }
        }

        private void SelectNext()
        {
            var count = optionsCount;
            if (count == 0)
            {
                return;
            }

            if (wrap)
            {
                var next = _selectedIndex >= count - 1 ? 0 : _selectedIndex + 1;
                selectedIndex = next;
                return;
            }

            if (_selectedIndex < count - 1)
            {
                selectedIndex = _selectedIndex + 1;
            }
        }

        private void ApplySelection(bool sendChangeEvent, bool notifyBindings)
        {
            var previousIndex = _selectedIndex;
            var previousText = _selectedText;

            _selectedIndex = ResolveIndex(_desiredSelectedIndex);
            BindSelectedItem();

            _pageIndicator.count = optionsCount;
            if (_selectedIndex >= 0)
            {
                _pageIndicator.SetValueWithoutNotify(_selectedIndex);
            }

            RefreshButtonStates();

            if (sendChangeEvent && previousIndex != _selectedIndex)
            {
                using var evt = ChangeEvent<int>.GetPooled(previousIndex, _selectedIndex);
                evt.target = this;
                SendEvent(evt);
            }

            if (!notifyBindings)
            {
                return;
            }

            if (previousIndex != _selectedIndex)
            {
                NotifyPropertyChanged(in SelectedIndexProperty);
            }

            if (previousText != _selectedText)
            {
                NotifyPropertyChanged(in SelectedTextProperty);
            }
        }

        private void RefreshButtonStates()
        {
            var count = optionsCount;
            var canMovePrevious = count > 0 && (wrap || _selectedIndex > 0);
            var canMoveNext = count > 0 && (wrap || _selectedIndex < count - 1);

            _previousButton.SetEnabled(canMovePrevious);
            _nextButton.SetEnabled(canMoveNext);
        }

        private int ResolveIndex(int desiredIndex)
        {
            var count = optionsCount;
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
            if (index < 0 || index >= optionsCount)
            {
                return emptyText;
            }

            return _sourceItems[index]?.ToString() ?? string.Empty;
        }

        private void BindSelectedItem()
        {
            if (_selectedIndex < 0 || _selectedIndex >= optionsCount)
            {
                _selectedItemElement.icon = null;
                _selectedItemElement.label = emptyText;
                _selectedText = _selectedItemElement.labelElement.text ?? string.Empty;
                return;
            }

            if (bindItem != null)
            {
                bindItem.Invoke(_selectedItemElement, _selectedIndex);
            }
            else
            {
                _selectedItemElement.icon = null;
                _selectedItemElement.label = GetOptionText(_selectedIndex);
            }

            _selectedText = _selectedItemElement.labelElement.text ?? string.Empty;
        }
    }
}
