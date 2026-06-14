// <copyright file="AnchorNavHostBurstTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;
    using Unity.Collections;

    public class AnchorNavHostBurstTests
    {
        [Test]
        public void CurrentDestination_NoHost_ReturnsDefault()
        {
            var destination = AnchorNavHost.Burst.CurrentDestination();

            Assert.AreEqual(default(FixedString32Bytes), destination);
        }

        [Test]
        public void CanGoBack_NoHost_ReturnsFalse()
        {
            Assert.IsFalse(AnchorNavHost.Burst.CanGoBack());
        }

        [Test]
        public void HasActivePopups_NoHost_ReturnsFalse()
        {
            Assert.IsFalse(AnchorNavHost.Burst.HasActivePopups());
        }

        [Test]
        public void Toggle_NoHost_ReturnsFalse()
        {
            Assert.IsFalse(AnchorNavHost.Burst.Toggle(default(FixedString32Bytes)));
        }

        [Test]
        public void PopBackStack_NoHost_ReturnsFalse()
        {
            Assert.IsFalse(AnchorNavHost.Burst.PopBackStack());
            Assert.IsFalse(AnchorNavHost.Burst.PopBackStackToPanel());
        }

        [Test]
        public void SaveStateHandle_NoHost_ReturnsZero()
        {
            Assert.AreEqual(0, AnchorNavHost.Burst.SaveStateHandle());
        }

    }
}
