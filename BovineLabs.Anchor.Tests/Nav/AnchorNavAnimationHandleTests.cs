// <copyright file="AnchorNavAnimationHandleTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class AnchorNavAnimationHandleTests
    {
        [Test]
        public void TryFinalizeFromAnimation_IsIdempotent()
        {
            var handle = new AnchorNavAnimationHandle(new VisualElement(), AnimationDescription.None, null);

            Assert.IsTrue(handle.TryFinalizeFromAnimation());
            Assert.IsFalse(handle.TryFinalizeFromAnimation());
            Assert.IsNull(handle.Handle);
        }

        [Test]
        public void CompleteImmediately_InvokesCallbacksOnce_AndClearsHandle()
        {
            var callbackCount = 0;
            var completeCount = 0;
            var callbackValue = -1f;

            var handle = new AnchorNavAnimationHandle(
                new VisualElement(),
                new AnimationDescription
                {
                    DurationMs = 50,
                    Easing = static f => f,
                    Callback = (_, f) =>
                    {
                        callbackCount++;
                        callbackValue = f;
                    },
                },
                () => completeCount++);

            handle.CompleteImmediately();
            handle.CompleteImmediately();

            Assert.AreEqual(1, callbackCount);
            Assert.AreEqual(1f, callbackValue);
            Assert.AreEqual(1, completeCount);
            Assert.IsNull(handle.Handle);
            Assert.IsFalse(handle.TryFinalizeFromAnimation());
        }
    }
}
