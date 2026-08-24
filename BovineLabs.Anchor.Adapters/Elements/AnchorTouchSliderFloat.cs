// <copyright file="AnchorTouchSliderFloat.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
    using System;
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    /// <summary>
    /// App UI floating-point touch slider with Anchor's overflow and focused-editing fixes.
    /// </summary>
    [UxmlElement]
    public partial class AnchorTouchSliderFloat : TouchSliderFloat, INotifyBindablePropertyChanged
    {
        private readonly VisualElement progressElement;
        private event EventHandler<BindablePropertyChangedEventArgs> BindingPropertyChanged;

        /// <inheritdoc />
        event EventHandler<BindablePropertyChangedEventArgs> INotifyBindablePropertyChanged.propertyChanged
        {
            add => this.BindingPropertyChanged += value;
            remove => this.BindingPropertyChanged -= value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorTouchSliderFloat"/> class.
        /// </summary>
        public AnchorTouchSliderFloat()
        {
            this.progressElement = this.Q<VisualElement>(TouchSlider<float>.progressUssClassName);
            AnchorTouchSlider.Initialize(this);
        }

        /// <summary>
        /// Gets or sets the slider size and publishes binding notifications missing from App UI's implementation.
        /// </summary>
        public new Size size
        {
            get => base.size;
            set
            {
                if (base.size == value)
                {
                    return;
                }

                base.size = value;
                this.NotifyBindingPropertyChanged(in AnchorTouchSlider.SizeProperty);
            }
        }

        /// <summary>
        /// Gets or sets the optional label and publishes binding notifications missing from App UI's implementation.
        /// </summary>
        public new string label
        {
            get => base.label;
            set
            {
                if (base.label == value)
                {
                    return;
                }

                base.label = value;
                this.NotifyBindingPropertyChanged(in AnchorTouchSlider.LabelProperty);
            }
        }

        /// <inheritdoc />
        public override void SetValueWithoutNotify(float newValue)
        {
            base.SetValueWithoutNotify(newValue);
            this.RefreshProgress();
        }

        /// <inheritdoc />
        protected override void SetOrientation(Direction newValue)
        {
            base.SetOrientation(newValue);
            this.RefreshProgress();
        }

        private void RefreshProgress()
        {
            AnchorTouchSlider.RefreshProgress(this, this.progressElement, this.orientation, this.m_CurrentDirection);
        }

        private void NotifyBindingPropertyChanged(in BindingId property)
        {
            this.NotifyPropertyChanged(in property);
            this.BindingPropertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
    }
}
