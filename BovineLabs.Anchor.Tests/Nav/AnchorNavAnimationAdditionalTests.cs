// <copyright file="AnchorNavAnimationAdditionalTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using System;
    using System.Reflection;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Core.ObjectManagement;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public class AnchorNavAnimationAdditionalTests
    {
        [Test]
        public void GetDescription_ReturnsCurrentDurationAndCallbacks()
        {
            var animation = ScriptableObject.CreateInstance<TestAnimation>();

            try
            {
                SetDuration(animation, 222);

                var description = animation.GetDescription();
                var element = new VisualElement();
                description.Callback.Invoke(element, 0.25f);

                Assert.AreEqual(222, description.DurationMs);
                Assert.IsNotNull(description.Easing);
                Assert.AreEqual(0.25f, animation.LastFactor);
                Assert.AreSame(element, animation.LastElement);
            }
            finally
            {
                Object.DestroyImmediate(animation);
            }
        }

        [Test]
        public void Reset_UsesDefaultDuration()
        {
            var animation = ScriptableObject.CreateInstance<TestAnimation>();

            try
            {
                SetDuration(animation, 5);
                animation.InvokeReset();

                Assert.AreEqual(77, animation.GetDescription().DurationMs);
            }
            finally
            {
                Object.DestroyImmediate(animation);
            }
        }

        [Test]
        public void IUidSetter_UpdatesExposedId()
        {
            var animation = ScriptableObject.CreateInstance<TestAnimation>();

            try
            {
                ((IUID)animation).ID = 99;

                Assert.AreEqual(99, animation.ID);
            }
            finally
            {
                Object.DestroyImmediate(animation);
            }
        }

        private static void SetDuration(AnchorNavAnimation animation, int duration)
        {
            var durationField = typeof(AnchorNavAnimation).GetField("duration", BindingFlags.Instance | BindingFlags.NonPublic);
            if (durationField == null)
            {
                throw new MissingFieldException(typeof(AnchorNavAnimation).FullName, "duration");
            }

            durationField.SetValue(animation, duration);
        }

        private sealed class TestAnimation : AnchorNavAnimation
        {
            public VisualElement LastElement { get; private set; }

            public float LastFactor { get; private set; }

            protected override Func<float, float> EasingFunction => static f => f;

            protected override Action<VisualElement, float> Callback => (element, factor) =>
            {
                this.LastElement = element;
                this.LastFactor = factor;
            };

            protected override int DefaultDuration => 77;

            public void InvokeReset()
            {
                this.Reset();
            }
        }
    }
}
