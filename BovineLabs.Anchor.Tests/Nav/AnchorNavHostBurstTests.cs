namespace BovineLabs.Anchor.Tests.Nav
{
    using System;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;

    public class AnchorNavHostBurstTests
    {
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
