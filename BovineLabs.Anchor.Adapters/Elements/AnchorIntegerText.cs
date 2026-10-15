namespace BovineLabs.Anchor.Elements
{
    using System;
    using Unity.Properties;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorIntegerText : AnchorNumericText
    {
        private long _number;

        public AnchorIntegerText()
        {
            RefreshText();
        }

        [CreateProperty]
        [UxmlAttribute]
        public long number
        {
            get => _number;
            set
            {
                if (_number == value)
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
