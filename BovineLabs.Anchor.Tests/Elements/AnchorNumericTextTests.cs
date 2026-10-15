namespace BovineLabs.Anchor.Tests.Elements
{
    using BovineLabs.Anchor.Elements;
    using NUnit.Framework;
    using Unity.AppUI.Core;
    using Unity.AppUI.UI;
    using Unity.Properties;
    using UnityEngine.UIElements;

    public class AnchorNumericTextTests
    {
        [TestCase(long.MinValue, "-9223372036854775808")]
        [TestCase(long.MaxValue, "9223372036854775807")]
        [TestCase(9_007_199_254_740_993L, "9007199254740993")]
        public void IntegerValue_PreservesExactDigits(long value, string expected)
        {
            var element = new AnchorIntegerText {
                culture = "en-US",
                number = value
            };

            Assert.AreEqual(expected, element.text);
        }

        [Test]
        public void FormatAndSuffixChanges_RefreshExistingValue()
        {
            var element = new AnchorIntegerText {
                culture = "en-US",
                number = 42,
                suffix = " fps"
            };
            Assert.AreEqual("42 fps", element.text);

            element.format = "D4";
            element.suffix = " Hz";

            Assert.AreEqual("0042 Hz", element.text);
        }

        [TestCase("en-US", "12.25 ms")]
        [TestCase("fr-FR", "12,25 ms")]
        public void NumberValue_UsesRequestedNumericCulture(string culture, string expected)
        {
            var element = new AnchorNumberText {
                culture = culture,
                format = "0.00",
                suffix = " ms",
                number = 12.25
            };

            Assert.AreEqual(expected, element.text);
        }

        [Test]
        public void LanguageChanges_RefreshValue_AndRespectExplicitCulture()
        {
            var parent = new VisualElement();
            var element = new AnchorNumberText {
                format = "0.0",
                suffix = " ms",
                number = 12.5
            };
            parent.Add(element);

            parent.ProvideContext(new LangContext("fr-FR"));
            Assert.AreEqual("12,5 ms", element.text);

            element.culture = "en-US";
            parent.ProvideContext(new LangContext("de-DE"));
            Assert.AreEqual("12.5 ms", element.text);

            element.culture = string.Empty;
            Assert.AreEqual("12,5 ms", element.text);
        }

        [Test]
        public void LargeFormatAndSuffix_GrowBufferWithoutTruncation()
        {
            var suffix = new string('x', 128);
            var element = new AnchorIntegerText {
                culture = "en-US",
                format = "D99",
                suffix = suffix,
                number = 7
            };

            Assert.AreEqual(new string('0', 98) + "7" + suffix, element.text);
        }

        [Test]
        public void BindingConverters_ApplyRawIntegerAndFloatValues()
        {
            VisualElement integer = new AnchorIntegerText {
                culture = "en-US",
                suffix = " fps"
            };
            VisualElement number = new AnchorNumberText {
                culture = "en-US",
                format = "0.0",
                suffix = " ms"
            };
            var converters = new DataBinding().sourceToUiConverters;
            var valuePath = new PropertyPath("number");

            Assert.IsTrue(converters.TrySetValue(ref integer, valuePath, 60, out _));
            Assert.IsTrue(converters.TrySetValue(ref number, valuePath, 16.26f, out _));
            Assert.AreEqual("60 fps", ((AnchorIntegerText)integer).text);
            Assert.AreEqual("16.3 ms", ((AnchorNumberText)number).text);
        }
    }
}
