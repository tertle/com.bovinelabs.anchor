// <copyright file="AnchorNavHostBurstTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using System;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using Unity.Collections;

    public class AnchorNavHostBurstTests
    {
        [Test]
        public void NoHost_ReturnsDefaults()
        {
            var destination = AnchorNavHost.Burst.CurrentDestination();

            Assert.AreEqual(default(FixedString32Bytes), destination);
            Assert.IsFalse(AnchorNavHost.Burst.CanGoBack());
            Assert.IsFalse(AnchorNavHost.Burst.HasActivePopups());
            Assert.IsFalse(AnchorNavHost.Burst.Toggle(default(FixedString32Bytes)));
            Assert.IsFalse(AnchorNavHost.Burst.PopBackStack());
            Assert.IsFalse(AnchorNavHost.Burst.PopBackStackToPanel());
            Assert.AreEqual(0, AnchorNavHost.Burst.SaveStateHandle());
        }

        [Test]
        public void CurrentDestination_AtUtf8Capacity_IsRepresented()
        {
            var destination = new string('a', 27) + "é";

            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen(destination);
            harness.Host.Navigate(destination);

            Assert.AreEqual(destination, AnchorNavHost.Burst.CurrentDestination().ToString());
        }

        [Test]
        public void CurrentDestination_BeyondUtf8Capacity_Throws()
        {
            var destination = new string('a', 28) + "é";

            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen(destination);
            harness.Host.Navigate(destination);

            Assert.Throws<ArgumentException>(() => AnchorNavHost.Burst.CurrentDestination());
        }
    }
}
