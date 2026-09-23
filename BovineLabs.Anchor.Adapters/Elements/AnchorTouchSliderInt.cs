namespace BovineLabs.Anchor.Elements
{
    using System;
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorTouchSliderInt : TouchSliderInt, INotifyBindablePropertyChanged
    {
        private readonly VisualElement _progressElement;
        private event EventHandler<BindablePropertyChangedEventArgs> BindingPropertyChanged;

        event EventHandler<BindablePropertyChangedEventArgs> INotifyBindablePropertyChanged.propertyChanged
        {
            add => BindingPropertyChanged += value;
            remove => BindingPropertyChanged -= value;
        }

        public AnchorTouchSliderInt()
        {
            _progressElement = this.Q<VisualElement>(progressUssClassName);
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
                NotifyBindingPropertyChanged(in AnchorTouchSlider.SizeProperty);
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
                NotifyBindingPropertyChanged(in AnchorTouchSlider.LabelProperty);
            }
        }

        public override void SetValueWithoutNotify(int newValue)
        {
            base.SetValueWithoutNotify(newValue);
            RefreshProgress();
        }

        protected override void SetOrientation(Direction newValue)
        {
            base.SetOrientation(newValue);
            RefreshProgress();
        }

        private void RefreshProgress()
        {
            AnchorTouchSlider.RefreshProgress(this, _progressElement, orientation, m_CurrentDirection);
        }

        private void NotifyBindingPropertyChanged(in BindingId property)
        {
            NotifyPropertyChanged(in property);
            BindingPropertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
    }
}
