// <copyright file="AnchorTouchSlider.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System;
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;
    using UnityEngine.UIElements;

    /// <summary>
    /// Touch-first slider base used by Anchor replacements for AppUI touch sliders.
    /// </summary>
    /// <typeparam name="TValue">A comparable value type.</typeparam>
    [MovedFrom(true, "BovineLabs.Anchor.Elements", "BovineLabs.Anchor")]
    [UxmlElement]
    public abstract partial class AnchorTouchSlider<TValue> : BaseSlider<TValue, TValue>
        where TValue : unmanaged, IComparable, IEquatable<TValue>
    {
        /// <summary>
        /// Main USS class.
        /// </summary>
        public const string UssClassName = "appui-touchslider";

        /// <summary>
        /// Progress USS class.
        /// </summary>
        public const string ProgressUssClassName = UssClassName + "__progress";

        /// <summary>
        /// Label container USS class.
        /// </summary>
        public const string LabelContainerUssClassName = UssClassName + "__label-container";

        /// <summary>
        /// Label USS class.
        /// </summary>
        public const string LabelUssClassName = UssClassName + "__label";

        /// <summary>
        /// Value container USS class.
        /// </summary>
        public const string ValueContainerUssClassName = UssClassName + "__valuelabel-container";

        /// <summary>
        /// Value USS class.
        /// </summary>
        public const string ValueUssClassName = UssClassName + "__valuelabel";

        /// <summary>
        /// Size USS class prefix.
        /// </summary>
        public const string SizeUssClassName = UssClassName + "--size-";

        /// <summary>
        /// Orientation USS class prefix.
        /// </summary>
        public const string VariantUssClassName = UssClassName + "--";

        /// <summary>
        /// Additional class used to scope the overflow workaround override.
        /// </summary>
        public const string WorkaroundUssClassName = "bl-touchslider-workaround";

        private static readonly BindingId SizeProperty = nameof(size);
        private static readonly BindingId LabelProperty = nameof(label);
        private static readonly BindingId OrientationProperty = nameof(orientation);

        private readonly VisualElement progressElement;
        private readonly Label labelElement;
        private readonly Label valueLabelElement;
        private readonly UnityEngine.UIElements.TextField inputField;

        private bool isEditingTextField;
        private Size sizeValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorTouchSlider{TValue}"/> class.
        /// </summary>
        protected AnchorTouchSlider()
        {
            this.AddToClassList(UssClassName);
            this.AddToClassList(WorkaroundUssClassName);

            this.focusable = true;
            this.pickingMode = PickingMode.Position;
            this.passMask = Passes.Clear;
            this.tabIndex = 0;

            this.progressElement = new VisualElement
            {
                name = ProgressUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            this.progressElement.AddToClassList(ProgressUssClassName);
            this.hierarchy.Add(this.progressElement);

            var labelContainer = new VisualElement
            {
                name = LabelContainerUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            labelContainer.AddToClassList(LabelContainerUssClassName);
            this.hierarchy.Add(labelContainer);

            this.labelElement = new Label { name = LabelUssClassName, pickingMode = PickingMode.Ignore };
            this.labelElement.AddToClassList(LabelUssClassName);
            labelContainer.Add(this.labelElement);

            var valueContainer = new VisualElement
            {
                name = ValueContainerUssClassName,
                pickingMode = PickingMode.Ignore,
            };
            valueContainer.AddToClassList(ValueContainerUssClassName);
            this.hierarchy.Add(valueContainer);

            this.valueLabelElement = new Label { name = ValueUssClassName, pickingMode = PickingMode.Ignore };
            this.valueLabelElement.AddToClassList(ValueUssClassName);
            valueContainer.Add(this.valueLabelElement);

            this.inputField = new UnityEngine.UIElements.TextField { name = ValueUssClassName, pickingMode = PickingMode.Position };
            this.inputField.AddToClassList(ValueUssClassName);
            this.inputField.RegisterCallback<FocusEvent>(this.OnInputFocusedIn);
            this.inputField.RegisterCallback<FocusOutEvent>(this.OnInputFocusedOut);
            this.inputField.RegisterValueChangedCallback(this.OnInputValueChanged);
            valueContainer.Add(this.inputField);

            this.m_DraggerManipulator = new Draggable(this.OnTrackClicked, this.OnTrackDragged, this.OnTrackUp, this.OnTrackDown)
            {
                dragDirection = Draggable.DragDirection.Horizontal,
            };
            this.AddManipulator(this.m_DraggerManipulator);
            this.AddManipulator(new KeyboardFocusController(this.OnKeyboardFocusIn, this.OnPointerFocusIn));

            this.HideInputField();
            this.size = Size.M;

            this.RegisterCallback<KeyDownEvent>(this.OnKeyDown);
        }

        /// <summary>
        /// Gets or sets slider size.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public Size size
        {
            get => this.sizeValue;
            set
            {
                if (this.sizeValue == value)
                {
                    return;
                }

                this.RemoveFromClassList(GetSizeUssClassName(this.sizeValue));
                this.sizeValue = value;
                this.AddToClassList(GetSizeUssClassName(this.sizeValue));
                this.NotifyPropertyChanged(in SizeProperty);
            }
        }

        /// <summary>
        /// Gets or sets the optional label shown on the slider.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string label
        {
            get => this.labelElement.text;
            set
            {
                if (this.labelElement.text == value)
                {
                    return;
                }

                this.labelElement.text = value;
                this.NotifyPropertyChanged(in LabelProperty);
            }
        }

        /// <inheritdoc />
        public override VisualElement contentContainer => null;

        /// <inheritdoc />
        public override void SetValueWithoutNotify(TValue newValue)
        {
            if (this.isEditingTextField)
            {
                return;
            }

            newValue = this.GetClampedValue(newValue);
            var stringValue = this.ParseValueToString(newValue);

            this.m_Value = newValue;
            this.inputField.SetValueWithoutNotify(stringValue);
            this.valueLabelElement.text = stringValue;
            if (this.validateValue != null)
            {
                this.invalid = !this.validateValue(this.m_Value);
            }

            this.RefreshUI();
        }

        /// <inheritdoc />
        protected override void SetOrientation(Direction newValue)
        {
            this.RemoveFromClassList(GetOrientationClassName(this.m_Orientation));
            this.m_Orientation = newValue;
            this.AddToClassList(GetOrientationClassName(this.m_Orientation));
            if (this.m_DraggerManipulator != null)
            {
                this.m_DraggerManipulator.dragDirection = this.m_Orientation == Direction.Horizontal
                    ? Draggable.DragDirection.Horizontal
                    : Draggable.DragDirection.Vertical;
            }

            this.RefreshUI();
            this.NotifyPropertyChanged(in OrientationProperty);
        }

        /// <inheritdoc />
        protected override TValue Clamp(TValue v, TValue lowBound, TValue highBound)
        {
            var result = v;
            if (lowBound.CompareTo(v) > 0)
            {
                result = lowBound;
            }

            if (highBound.CompareTo(v) < 0)
            {
                result = highBound;
            }

            return result;
        }

        /// <inheritdoc />
        protected override void OnTrackClicked()
        {
            if (!this.m_DraggerManipulator.hasMoved)
            {
                this.ShowInputField();
            }
        }

        private static string GetSizeUssClassName(Size size)
        {
            var suffix = size switch
            {
                Size.S => "s",
                Size.M => "m",
                Size.L => "l",
                _ => "m",
            };

            return SizeUssClassName + suffix;
        }

        private static string GetOrientationClassName(Direction direction)
        {
            var suffix = direction == Direction.Vertical ? "vertical" : "horizontal";
            return VariantUssClassName + suffix;
        }

        private void OnInputValueChanged(ChangeEvent<string> evt)
        {
            evt.StopPropagation();
        }

        private void OnInputFocusedIn(FocusEvent evt)
        {
            this.isEditingTextField = true;
            this.AddToClassList(Styles.focusedUssClassName);
        }

        private void OnInputFocusedOut(FocusOutEvent evt)
        {
            this.isEditingTextField = false;
            this.RemoveFromClassList(Styles.focusedUssClassName);
            this.HideInputField();

            var currentValueString = this.ParseValueToString(this.value);
            if (this.inputField.value != currentValueString && this.ParseStringToValue(this.inputField.value, out var newValue))
            {
                this.value = newValue;
                this.SetValueWithoutNotify(newValue);
            }
            else
            {
                this.inputField.SetValueWithoutNotify(currentValueString);
            }
        }

        private void OnPointerFocusIn(FocusInEvent evt)
        {
            this.passMask = 0;
        }

        private void OnKeyboardFocusIn(FocusInEvent evt)
        {
            this.passMask = Passes.Clear | Passes.Outline;
        }

        private void ShowInputField()
        {
            this.valueLabelElement.style.display = DisplayStyle.None;
            this.inputField.style.display = DisplayStyle.Flex;
            this.inputField.schedule.Execute(this.OnInputFieldShown);
        }

        private void OnInputFieldShown()
        {
            this.inputField.SetValueWithoutNotify(this.ParseRawValueToString(this.value));
            this.inputField.Focus();
            this.inputField.SelectAll();
        }

        private void HideInputField()
        {
            this.valueLabelElement.style.display = DisplayStyle.Flex;
            this.inputField.style.display = DisplayStyle.None;
        }

        private void RefreshUI()
        {
            if (this.panel == null || !this.layout.IsValid())
            {
                return;
            }

            var norm = this.SliderNormalizeValue(this.m_Value, this.lowValue, this.highValue);
            var clampedNorm = Mathf.Clamp01(norm);

            var leftInset = this.resolvedStyle.borderLeftWidth;
            var rightInset = this.resolvedStyle.borderRightWidth;
            var topInset = this.resolvedStyle.borderTopWidth;
            var bottomInset = this.resolvedStyle.borderBottomWidth;

            if (this.m_Orientation == Direction.Horizontal)
            {
                var trackWidth = Mathf.Max(0, this.layout.width - leftInset - rightInset);
                var width = trackWidth * clampedNorm;
                var left = this.m_CurrentDirection == Dir.Ltr ? leftInset : this.layout.width - rightInset - width;

                this.progressElement.style.left = left;
                this.progressElement.style.right = StyleKeyword.Null;
                this.progressElement.style.width = width;
                this.progressElement.style.top = topInset;
                this.progressElement.style.bottom = bottomInset;
                this.progressElement.style.height = StyleKeyword.Null;
            }
            else
            {
                var trackHeight = Mathf.Max(0, this.layout.height - topInset - bottomInset);
                var height = trackHeight * clampedNorm;
                var top = this.layout.height - bottomInset - height;

                this.progressElement.style.top = top;
                this.progressElement.style.height = height;
                this.progressElement.style.left = leftInset;
                this.progressElement.style.right = rightInset;
                this.progressElement.style.bottom = StyleKeyword.Null;
                this.progressElement.style.width = StyleKeyword.Null;
            }
        }
    }
}
