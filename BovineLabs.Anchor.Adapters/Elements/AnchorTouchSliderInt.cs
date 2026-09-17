namespace BovineLabs.Anchor.Elements
{
    using System;
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorTouchSliderInt : TouchSliderInt, INotifyBindablePropertyChanged
    {
        private readonly VisualElement progressElement;
        private event EventHandler<BindablePropertyChangedEventArgs> BindingPropertyChanged;

        event EventHandler<BindablePropertyChangedEventArgs> INotifyBindablePropertyChanged.propertyChanged
        {
            add => this.BindingPropertyChanged += value;
            remove => this.BindingPropertyChanged -= value;
        }

        public AnchorTouchSliderInt()
        {
            this.progressElement = this.Q<VisualElement>(TouchSlider<int>.progressUssClassName);
            AnchorTouchSlider.Initialize(this);
        }

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

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            this.RefreshProgress();
        }

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
