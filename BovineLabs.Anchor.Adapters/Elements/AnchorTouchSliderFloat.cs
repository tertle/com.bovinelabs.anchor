// <copyright file="AnchorTouchSliderFloat.cs" company="BovineLabs">
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
    /// Touch slider replacement for floating-point values.
    /// </summary>
    [MovedFrom(true, "BovineLabs.Anchor.Elements", "BovineLabs.Anchor")]
    [UxmlElement]
    public partial class AnchorTouchSliderFloat : AnchorTouchSlider<float>
    {
        private const float DefaultStep = 0.1f;
        private const float DefaultShiftStep = 1f;
        private const string DefaultFormatString = "g7";

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorTouchSliderFloat"/> class.
        /// </summary>
        public AnchorTouchSliderFloat()
        {
            this.formatString = DefaultFormatString;
            this.step = DefaultStep;
            this.shiftStep = DefaultShiftStep;
            this.lowValue = 0f;
            this.highValue = 1f;
            this.value = 0f;
        }

        /// <summary>
        /// Gets or sets the low end of the slider range.
        /// </summary>
        [UxmlAttribute("low-value")]
        public float lowValueOverride
        {
            get => this.lowValue;
            set => this.lowValue = value;
        }

        /// <summary>
        /// Gets or sets the high end of the slider range.
        /// </summary>
        [UxmlAttribute("high-value")]
        public float highValueOverride
        {
            get => this.highValue;
            set => this.highValue = value;
        }

        /// <summary>
        /// Gets or sets the current slider value.
        /// </summary>
        [UxmlAttribute("value")]
        public float valueOverride
        {
            get => this.value;
            set => this.value = value;
        }

        /// <summary>
        /// Gets or sets the keyboard/controller step amount.
        /// </summary>
        [UxmlAttribute("step")]
        public float stepOverride
        {
            get => this.step;
            set => this.step = value;
        }

        /// <summary>
        /// Gets or sets the keyboard/controller shift step amount.
        /// </summary>
        [UxmlAttribute("shift-step")]
        public float shiftStepOverride
        {
            get => this.shiftStep;
            set => this.shiftStep = value;
        }

        /// <inheritdoc />
        protected override int thumbCount => 1;

        /// <inheritdoc />
        protected override bool ParseStringToValue(string strValue, out float value)
        {
            if (string.IsNullOrWhiteSpace(strValue))
            {
                value = this.value;
                return false;
            }

            if (ExpressionEvaluator.Evaluate(strValue, out double result))
            {
                value = ClampToFloat(result);
                return true;
            }

            value = this.value;
            return false;
        }

        /// <inheritdoc />
        protected override string ParseValueToString(float value)
        {
            return this.formatFunction != null
                ? this.formatFunction(value)
                : value.ToString(this.formatString, CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc />
        protected override string ParseSubValueToString(float value)
        {
            return this.ParseValueToString(value);
        }

        /// <inheritdoc />
        protected override string ParseRawValueToString(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture.NumberFormat);
        }

        /// <inheritdoc />
        protected override float SliderLerpUnclamped(float a, float b, float interpolant)
        {
            return Mathf.LerpUnclamped(a, b, interpolant);
        }

        /// <inheritdoc />
        protected override float SliderNormalizeValue(float currentValue, float lowerValue, float higherValue)
        {
            return Mathf.InverseLerp(lowerValue, higherValue, currentValue);
        }

        /// <inheritdoc />
        protected override float Mad(int m, float a, float b)
        {
            return (m * a) + b;
        }

        /// <inheritdoc />
        protected override int GetStepCount(float stepValue)
        {
            return Mathf.FloorToInt((this.highValue - this.lowValue) / stepValue) + 1;
        }

        /// <inheritdoc />
        protected override float ClampThumb(float x, float min, float max)
        {
            return Mathf.Clamp(x, min, max);
        }

        /// <inheritdoc />
        protected override float GetValueFromScalarValues(Span<float> values)
        {
            return values[0];
        }

        /// <inheritdoc />
        protected override void GetScalarValuesFromValue(float value, Span<float> values)
        {
            values[0] = value;
        }

        private static float ClampToFloat(double value)
        {
            if (value < float.MinValue)
            {
                return float.MinValue;
            }

            if (value > float.MaxValue)
            {
                return float.MaxValue;
            }

            return (float)value;
        }
    }
}
