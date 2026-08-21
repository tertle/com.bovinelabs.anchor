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
    }
}
