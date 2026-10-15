namespace BovineLabs.Anchor.Elements
{
    using System;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorNumberText : AnchorNumericText
    {
        private double _number;

        public AnchorNumberText()
        {
            RefreshText();
        }

        [CreateProperty]
        [UxmlAttribute]
        public double number
        {
            get => _number;
            set
            {
                if (_number.Equals(value))
                {
                    return;
                }

                _number = value;
                RefreshText();
                NotifyPropertyChanged(in NumberProperty);
            }
        }

        protected override bool TryFormatValue(Span<char> destination, out int charsWritten, ReadOnlySpan<char> numericFormat, IFormatProvider provider)
        {
            return _number.TryFormat(destination, out charsWritten, numericFormat, provider);
        }
    }
}
