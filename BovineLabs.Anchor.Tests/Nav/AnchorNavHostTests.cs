namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;

    public class AnchorNavHostTests
    {
        [Test]
        public void ClearBackStack_LeavesCurrentDestination()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");
            harness.RegisterScreen("B");

            harness.Host.Navigate("A");
            harness.Host.Navigate("B");

            harness.Host.ClearBackStack();

            Assert.IsFalse(harness.Host.CanGoBack);
            Assert.AreEqual("B", harness.Host.CurrentDestination);
        }

        [Test]
        public void Toggle_LowerPopupInActiveBranch_RemovesMatchAndPopupsAboveIt()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup-a");
            harness.RegisterScreen("popup-b");

            harness.Host.Navigate("base");
            harness.Host.Navigate("popup-a", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });
            harness.Host.Navigate("popup-b", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            var toggled = harness.Host.Toggle("popup-a");

            Assert.IsTrue(toggled);
            Assert.IsFalse(harness.Host.HasActivePopups);
            Assert.AreEqual("base", harness.Host.CurrentDestination);
        }
    }
}
