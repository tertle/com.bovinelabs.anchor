// <copyright file="OptionPagerTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_APPUI
namespace BovineLabs.Anchor.Tests.Elements
{
    using System.Collections.Generic;
    using System.Reflection;
    using BovineLabs.Anchor.Elements;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class OptionPagerTests
    {
        [Test]
        public void SourceItems_UpdatesOptionsCount_AndClampsSelection()
        {
            var pager = new OptionPager
            {
                selectedIndex = 10,
                sourceItems = new List<string> { "A", "B", "C" },
            };

            Assert.AreEqual(3, pager.optionsCount);
            Assert.AreEqual(2, pager.selectedIndex);
        }

        [Test]
        public void SelectedIndex_ClampsInRange_AndIsNegativeWhenEmpty()
        {
            var pager = new OptionPager
            {
                sourceItems = new List<string> { "A", "B" },
            };

            pager.selectedIndex = -5;
            Assert.AreEqual(0, pager.selectedIndex);

            pager.selectedIndex = 99;
            Assert.AreEqual(1, pager.selectedIndex);

            pager.sourceItems = new List<string>();
            Assert.AreEqual(-1, pager.selectedIndex);
        }

        [Test]
        public void WrapTrue_PreviousAndNext_WrapAtBounds()
        {
            var pager = new OptionPager
            {
                wrap = true,
                sourceItems = new List<string> { "A", "B", "C" },
                selectedIndex = 0,
            };

            InvokePrivate(pager, "SelectPrevious");
            Assert.AreEqual(2, pager.selectedIndex);

            InvokePrivate(pager, "SelectNext");
            Assert.AreEqual(0, pager.selectedIndex);
        }

        [Test]
        public void WrapFalse_PreviousAndNext_LockAtBounds()
        {
            var pager = new OptionPager
            {
                wrap = false,
                sourceItems = new List<string> { "A", "B", "C" },
                selectedIndex = 0,
            };

            InvokePrivate(pager, "SelectPrevious");
            Assert.AreEqual(0, pager.selectedIndex);

            pager.selectedIndex = 2;
            InvokePrivate(pager, "SelectNext");
            Assert.AreEqual(2, pager.selectedIndex);
        }

        [Test]
        public void EmptyText_UpdatesDisplayWhenNoItems()
        {
            var pager = new OptionPager
            {
                emptyText = "Nothing",
                sourceItems = new List<string>(),
            };

            Assert.AreEqual("Nothing", pager.selectedText);
            Assert.AreEqual("Nothing", pager.SelectedItemElement.label);
        }

        [Test]
        public void ShowIndicator_TogglesIndicatorDisplay()
        {
            var pager = new OptionPager();

            pager.showIndicator = false;
            Assert.AreEqual(DisplayStyle.None, pager.Indicator.style.display.value);

            pager.showIndicator = true;
            Assert.AreEqual(DisplayStyle.Flex, pager.Indicator.style.display.value);
        }

        [Test]
        public void SelectedText_FollowsSelectedItemText()
        {
            var pager = new OptionPager
            {
                sourceItems = new List<string> { "A", "B" },
                selectedIndex = 1,
            };

            Assert.AreEqual("B", pager.selectedText);
        }

        [Test]
        public void SelectedIndex_SetSameValue_IsNoOp()
        {
            var pager = new OptionPager
            {
                sourceItems = new List<string> { "A", "B" },
            };

            pager.selectedIndex = 1;
            var textAfterFirstChange = pager.selectedText;
            pager.selectedIndex = 1;

            Assert.AreEqual(1, pager.selectedIndex);
            Assert.AreEqual(textAfterFirstChange, pager.selectedText);
        }

        private static void InvokePrivate(OptionPager pager, string methodName)
        {
            var method = typeof(OptionPager).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            method!.Invoke(pager, null);
        }
    }
}
#endif
