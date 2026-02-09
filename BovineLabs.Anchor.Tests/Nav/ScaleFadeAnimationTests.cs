// <copyright file="ScaleFadeAnimationTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav.Animations;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class ScaleFadeAnimationTests
    {
        [Test]
        public void ScaleFadeIn_Callback_ProducesExpectedScaleAndOpacity()
        {
            var animation = ScriptableObject.CreateInstance<ScaleFadeInAnimation>();

            try
            {
                var callback = animation.GetDescription().Callback;
                var element = new VisualElement();

                callback.Invoke(element, 0f);
                AssertState(element, 1.2f, 0f);

                callback.Invoke(element, 0.5f);
                AssertState(element, 1.1f, 0.5f);

                callback.Invoke(element, 1f);
                AssertState(element, 1f, 1f);
            }
            finally
            {
                Object.DestroyImmediate(animation);
            }
        }

        [Test]
        public void ScaleFadeOut_Callback_ProducesExpectedScaleAndOpacity()
        {
            var animation = ScriptableObject.CreateInstance<ScaleFadeOutAnimation>();

            try
            {
                var callback = animation.GetDescription().Callback;
                var element = new VisualElement();

                callback.Invoke(element, 0f);
                AssertState(element, 1f, 1f);

                callback.Invoke(element, 0.5f);
                AssertState(element, 1.1f, 0.5f);

                callback.Invoke(element, 1f);
                AssertState(element, 1.2f, 0f);
            }
            finally
            {
                Object.DestroyImmediate(animation);
            }
        }

        private static void AssertState(VisualElement element, float expectedScale, float expectedOpacity)
        {
            var scale = element.style.scale.value.value.x;
            var opacity = element.style.opacity.value;

            Assert.That(scale, Is.EqualTo(expectedScale).Within(0.0001f));
            Assert.That(opacity, Is.EqualTo(expectedOpacity).Within(0.0001f));
        }
    }
}
