// <copyright file="AnchorNavHostTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class AnchorNavHostTests
    {
        [Test]
        public void Navigate_InvalidDestination_ReturnsFalse()
        {
            using var harness = new TestAnchorNavHostHarness();

            Assert.IsFalse(harness.Host.Navigate(null));
            Assert.IsFalse(harness.Host.Navigate(string.Empty));
            Assert.IsFalse(harness.Host.Navigate("   "));
        }

        [Test]
        public void Navigate_DefaultOptions_SetsCurrentDestination()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");

            var result = harness.Host.Navigate("A");

            Assert.IsTrue(result);
            Assert.AreEqual("A", harness.Host.CurrentDestination);
            Assert.IsFalse(harness.Host.CanGoBack);
        }

        [Test]
        public void Navigate_CreatedItem_UsesIgnorePickingMode()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");

            harness.Host.Navigate("A");

            Assert.AreEqual(1, harness.Host.contentContainer.childCount);
            Assert.AreEqual(PickingMode.Ignore, harness.Host.contentContainer[0].pickingMode);
        }

        [Test]
        public void Navigate_FromAtoB_PushesBackStack()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");
            harness.RegisterScreen("B");

            harness.Host.Navigate("A");
            harness.Host.Navigate("B");

            Assert.AreEqual("B", harness.Host.CurrentDestination);
            Assert.IsTrue(harness.Host.CanGoBack);
        }

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
        public void ClearNavigation_ClearsCurrentAndBackStack()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");
            harness.RegisterScreen("B");

            harness.Host.Navigate("A");
            harness.Host.Navigate("B");

            harness.Host.ClearNavigation();

            Assert.IsFalse(harness.Host.CanGoBack);
            Assert.IsFalse(harness.Host.HasActivePopups);
            Assert.IsNull(harness.Host.CurrentDestination);
        }

        [Test]
        public void CloseAllPopups_WhenNone_ReturnsFalse()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");
            harness.Host.Navigate("A");

            var closed = harness.Host.CloseAllPopups();

            Assert.IsFalse(closed);
        }

        [Test]
        public void NavigatePopupOnCurrent_SetsPopupState()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");

            harness.Host.Navigate("base");

            var result = harness.Host.Navigate(
                "popup",
                new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            Assert.IsTrue(result);
            Assert.AreEqual("popup", harness.Host.CurrentDestination);
            Assert.IsTrue(harness.Host.HasActivePopups);
        }

        [Test]
        public void ClosePopup_Unknown_ReturnsFalse()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");

            harness.Host.Navigate("base");
            harness.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            var closed = harness.Host.ClosePopup("missing");

            Assert.IsFalse(closed);
            Assert.IsTrue(harness.Host.HasActivePopups);
        }

        [Test]
        public void ClosePopup_Existing_RemovesPopupAndRestoresBase()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");

            harness.Host.Navigate("base");
            harness.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            var closed = harness.Host.ClosePopup("popup");

            Assert.IsTrue(closed);
            Assert.IsFalse(harness.Host.HasActivePopups);
            Assert.AreEqual("base", harness.Host.CurrentDestination);
        }

        [Test]
        public void Toggle_ActivePopup_RemovesPopupAndRestoresBase()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");

            harness.Host.Navigate("base");
            harness.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            var toggled = harness.Host.Toggle("popup");

            Assert.IsTrue(toggled);
            Assert.IsFalse(harness.Host.HasActivePopups);
            Assert.AreEqual("base", harness.Host.CurrentDestination);
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

        [Test]
        public void PopBackStackToPanel_RemovesPopupLayersFromPoppedSnapshot()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");
            harness.RegisterScreen("second");

            harness.Host.Navigate("base");
            harness.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });
            harness.Host.Navigate("second");

            var popped = harness.Host.PopBackStackToPanel();

            Assert.IsTrue(popped);
            Assert.AreEqual("popup", harness.Host.CurrentDestination);
            Assert.IsFalse(harness.Host.HasActivePopups);
        }

        [Test]
        public void TryGetAnimation_Zero_ReturnsTrueWithNullAnimation()
        {
            using var harness = new TestAnchorNavHostHarness();

            var result = harness.Host.TryGetAnimation(0, out var animation);

            Assert.IsTrue(result);
            Assert.IsNull(animation);
        }

        [Test]
        public void MissingAnimationId_DoesNotThrowAndFallsBack()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");
            harness.Host.Navigate("A");

            Assert.DoesNotThrow(() => harness.Host.ClearNavigation(9999));
            Assert.IsNull(harness.Host.CurrentDestination);
        }
    }
}
