// <copyright file="AnchorTouchSliderTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Elements
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Elements;
    using NUnit.Framework;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    public class AnchorTouchSliderTests
    {
        [Test]
        public void FloatSlider_PublishesRetainedNotificationsOnlyWhenValuesChange()
        {
            var slider = new AnchorTouchSliderFloat();

            AssertBindingNotifications(slider, value => slider.size = value, value => slider.label = value);
            Assert.AreEqual(Size.L, slider.size);
            Assert.AreEqual("Volume", slider.label);
        }

        [Test]
        public void IntSlider_PublishesRetainedNotificationsOnlyWhenValuesChange()
        {
            var slider = new AnchorTouchSliderInt();

            AssertBindingNotifications(slider, value => slider.size = value, value => slider.label = value);
            Assert.AreEqual(Size.L, slider.size);
            Assert.AreEqual("Volume", slider.label);
        }

        private static void AssertBindingNotifications(VisualElement slider, Action<Size> setSize, Action<string> setLabel)
        {
            var changedProperties = new List<string>();
            ((INotifyBindablePropertyChanged)slider).propertyChanged += (_, args) => changedProperties.Add(args.propertyName);

            setSize(Size.L);
            setLabel("Volume");

            CollectionAssert.AreEqual(new[] { "size", "label" }, changedProperties);

            setSize(Size.L);
            setLabel("Volume");

            CollectionAssert.AreEqual(new[] { "size", "label" }, changedProperties);
        }
    }
}
