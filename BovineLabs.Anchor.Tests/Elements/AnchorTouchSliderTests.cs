// <copyright file="AnchorTouchSliderTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Elements
{
    using System;
    using BovineLabs.Anchor.Elements;
    using NUnit.Framework;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    public class AnchorTouchSliderTests
    {
        private const string WorkaroundUssClassName = "bl-touchslider-workaround";

        [Test]
        public void FloatSlider_ExtendsAppUiAndPublishesRetainedNotifications()
        {
            var slider = new AnchorTouchSliderFloat();

            Assert.IsInstanceOf<TouchSliderFloat>(slider);
            AssertBindingNotifications(slider, value => slider.size = value, value => slider.label = value);
            Assert.AreEqual(Size.L, slider.size);
            Assert.AreEqual("Volume", slider.label);
        }

        [Test]
        public void IntSlider_ExtendsAppUiAndPublishesRetainedNotifications()
        {
            var slider = new AnchorTouchSliderInt();

            Assert.IsInstanceOf<TouchSliderInt>(slider);
            AssertBindingNotifications(slider, value => slider.size = value, value => slider.label = value);
            Assert.AreEqual(Size.L, slider.size);
            Assert.AreEqual("Volume", slider.label);
        }

        private static void AssertBindingNotifications(VisualElement slider, Action<Size> setSize, Action<string> setLabel)
        {
            var notificationCount = 0;
            ((INotifyBindablePropertyChanged)slider).propertyChanged += (_, _) => notificationCount++;

            setSize(Size.L);
            setLabel("Volume");

            Assert.AreEqual(2, notificationCount);
            Assert.IsTrue(slider.ClassListContains(WorkaroundUssClassName));

            setSize(Size.L);
            setLabel("Volume");

            Assert.AreEqual(2, notificationCount);
        }
    }
}
