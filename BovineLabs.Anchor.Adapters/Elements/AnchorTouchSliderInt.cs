// <copyright file="AnchorTouchSliderInt.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System;
    using System.Globalization;
    using UnityEngine;
    using UnityEngine.Scripting.APIUpdating;
    using UnityEngine.UIElements;

    /// <summary>
    /// Touch slider replacement for integer values.
    /// </summary>
    [MovedFrom(true, "BovineLabs.Anchor.Elements", "BovineLabs.Anchor")]
    [UxmlElement]
    public partial class AnchorTouchSliderInt : AnchorTouchSlider<int>
    {
        private const int DefaultStep = 1;
        private const int DefaultShiftStep = 10;
        private const string DefaultFormatString = "#######0";

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorTouchSliderInt"/> class.
        /// </summary>
        public AnchorTouchSliderInt()
        {
            this.formatString = DefaultFormatString;
            this.step = DefaultStep;
            this.shiftStep = DefaultShiftStep;
            this.lowValue = 0;
            this.highValue = 1;
            this.value = 0;
        }

        /// <summary>
        /// Gets or sets the low end of the slider range.
        /// </summary>
        [UxmlAttribute("low-value")]
        public int lowValueOverride
        {
            get => this.lowValue;
            set => this.lowValue = value;
        }

        /// <summary>
        /// Gets or sets the high end of the slider range.
        /// </summary>
        [UxmlAttribute("high-value")]
        public int highValueOverride
        {
            get => this.highValue;
            set => this.highValue = value;
        }

        /// <summary>
        /// Gets or sets the current slider value.
        /// </summary>
        [UxmlAttribute("value")]
        public int valueOverride
        {
            get => this.value;
            set => this.value = value;
        }

        /// <summary>
        /// Gets or sets the keyboard/controller step amount.
        /// </summary>
        [UxmlAttribute("step")]
        public int stepOverride
        {
            get => this.step;
            set => this.step = value;
        }

        /// <summary>
        /// Gets or sets the keyboard/controller shift step amount.
        /// </summary>
        [UxmlAttribute("shift-step")]
        public int shiftStepOverride
        {
            get => this.shiftStep;
            set => this.shiftStep = value;
        }

        /// <inheritdoc />
        protected override int thumbCount => 1;

        /// <inheritdoc />
        protected override bool ParseStringToValue(string strValue, out int value)
        {
            if (string.IsNullOrWhiteSpace(strValue))
            {
                value = this.value;
                return false;
            }

            if (ExpressionEvaluator.Evaluate(strValue, out long result))
            {
                value = ClampToInt(result);
                return true;
            }

            value = this.value;
            return false;
        }

        /// <inheritdoc />
        protected override string ParseValueToString(int value)
        {
            return this.formatFunction != null
                ? this.formatFunction(value)
                : value.ToString(this.formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc />
        protected override string ParseSubValueToString(int value)
        {
            return this.ParseValueToString(value);
        }

        /// <inheritdoc />
        protected override string ParseRawValueToString(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc />
        protected override int SliderLerpUnclamped(int a, int b, float interpolant)
        {
            return Mathf.RoundToInt(Mathf.LerpUnclamped(a, b, interpolant));
        }

        /// <inheritdoc />
        protected override float SliderNormalizeValue(int currentValue, int lowerValue, int higherValue)
        {
            return Mathf.InverseLerp(lowerValue, higherValue, currentValue);
        }

        /// <inheritdoc />
        protected override int Mad(int m, int a, int b)
        {
            return (m * a) + b;
        }

        /// <inheritdoc />
        protected override int GetStepCount(int stepValue)
        {
            return ((this.highValue - this.lowValue) / stepValue) + 1;
        }

        /// <inheritdoc />
        protected override int ClampThumb(int x, int min, int max)
        {
            return Mathf.Clamp(x, min, max);
        }

        /// <inheritdoc />
        protected override int GetValueFromScalarValues(Span<int> values)
        {
            return values[0];
        }

        /// <inheritdoc />
        protected override void GetScalarValuesFromValue(int value, Span<int> values)
        {
            values[0] = value;
        }

        private static int ClampToInt(long value)
        {
            if (value < int.MinValue)
            {
                return int.MinValue;
            }

            if (value > int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)value;
        }
    }
}
