namespace BovineLabs.Anchor.Elements
{
    using System;
    using System.Globalization;
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine.UIElements;

    [UxmlElement]
    public abstract partial class AnchorNumericText : BaseTextElement
    {
        public const string UssClassName = "bl-anchor-numeric-text";

        [NoAutoStaticsCleanup]
        internal static readonly BindingId NumberProperty = "number";

        [NoAutoStaticsCleanup]
        private static readonly BindingId FormatProperty = nameof(format);
        [NoAutoStaticsCleanup]
        private static readonly BindingId SuffixProperty = nameof(suffix);
        [NoAutoStaticsCleanup]
        private static readonly BindingId CultureProperty = nameof(culture);

        private char[] _buffer = new char[64];
        private string _format = string.Empty;
        private string _suffix = string.Empty;
        private string _culture = string.Empty;
        private CultureInfo _cultureInfo;

        protected AnchorNumericText()
        {
            AddToClassList(UssClassName);
            AddToClassList(Text.ussClassName);
            AddToClassList(Text.primaryUssClassName);
            AddToClassList(Text.GetSizeUssClassName(TextSize.M));
            pickingMode = PickingMode.Position;

            RegisterCallback<AttachToPanelEvent>(OnAttached);
            this.RegisterContextChangedCallback<LangContext>(OnLanguageChanged);
        }

        [CreateProperty]
        [UxmlAttribute]
        public string format
        {
            get => _format;
            set
            {
                value ??= string.Empty;
                if (_format == value)
                {
                    return;
                }

                _format = value;
                RefreshText();
                NotifyPropertyChanged(in FormatProperty);
            }
        }

        [CreateProperty]
        [UxmlAttribute]
        public string suffix
        {
            get => _suffix;
            set
            {
                value ??= string.Empty;
                if (_suffix == value)
                {
                    return;
                }

                _suffix = value;
                RefreshText();
                NotifyPropertyChanged(in SuffixProperty);
            }
        }

        /// <summary>
        /// An optional culture name. Empty follows the App UI language context, or the current culture when no language is supplied.
        /// </summary>
        [CreateProperty]
        [UxmlAttribute]
        public string culture
        {
            get => _culture;
            set
            {
                value ??= string.Empty;
                if (_culture == value)
                {
                    return;
                }

                _culture = value;
                ResolveCulture();
                RefreshText();
                NotifyPropertyChanged(in CultureProperty);
            }
        }

        protected abstract bool TryFormatValue(Span<char> destination, out int charsWritten, ReadOnlySpan<char> numericFormat, IFormatProvider provider);

        protected void RefreshText()
        {
            var suffixLength = _suffix.Length;
            if (_buffer.Length <= suffixLength)
            {
                _buffer = new char[checked(suffixLength + 64)];
            }

            int charsWritten;
            while (!TryFormatValue(_buffer.AsSpan(0, _buffer.Length - suffixLength), out charsWritten, _format.AsSpan(),
                       _cultureInfo ?? CultureInfo.CurrentCulture))
            {
                _buffer = new char[checked(_buffer.Length * 2)];
            }

            _suffix.AsSpan().CopyTo(_buffer.AsSpan(charsWritten));
            var formatted = _buffer.AsSpan(0, charsWritten + suffixLength);
            if (panel == null)
            {
                // Detached elements have no native text buffer. Keep their text readable without SetText's fallback warning.
                text = new string(formatted);
            }
            else
            {
                SetText(formatted);
            }
        }

        private void OnAttached(AttachToPanelEvent evt)
        {
            ResolveCulture();
            EnsureTextCapacity(_buffer.Length);
            RefreshText();
        }

        private void OnLanguageChanged(ContextChangedEvent<LangContext> evt)
        {
            ResolveCulture();
            RefreshText();
        }

        private void ResolveCulture()
        {
            var cultureName = string.IsNullOrEmpty(_culture) ? this.GetContext<LangContext>()?.lang : _culture;
            _cultureInfo = string.IsNullOrEmpty(cultureName) ? null : CultureInfo.GetCultureInfo(cultureName);
        }
    }
}
