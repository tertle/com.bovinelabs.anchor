// <copyright file="AnchorNavHostStateHandleTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;

    public class AnchorNavHostStateHandleTests
    {
        [Test]
        public void SaveState_CapturesActiveAndBackStack()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");
            harness.RegisterScreen("B");
            harness.RegisterScreen("P");

            harness.Host.Navigate("A");
            harness.Host.Navigate("B");
            harness.Host.Navigate("P", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            var state = harness.Host.SaveState();

            Assert.AreEqual("P", state.CurrentDestination);
            Assert.AreEqual(2, state.ActiveStack.Count);
            Assert.AreEqual("B", state.ActiveStack[0].Destination);
            Assert.IsFalse(state.ActiveStack[0].IsPopup);
            Assert.AreEqual("P", state.ActiveStack[1].Destination);
            Assert.IsTrue(state.ActiveStack[1].IsPopup);
            Assert.AreEqual(2, state.BackStack.Count);
            Assert.AreEqual("A", state.BackStack[0].Destination);
            Assert.AreEqual("B", state.BackStack[1].Destination);
        }

        [Test]
        public void RestoreState_Null_IsSafeNoOp()
        {
            using var harness = new TestAnchorNavHostHarness();

            Assert.DoesNotThrow(() => harness.Host.RestoreState(null));
        }

        [Test]
        public void SaveStateHandle_ReturnsIncreasingValues()
        {
            using var harness = new TestAnchorNavHostHarness();

            var first = harness.Host.SaveStateHandle();
            var second = harness.Host.SaveStateHandle();

            Assert.Greater(first, 0);
            Assert.Greater(second, first);
        }

        [Test]
        public void ReleaseStateHandle_InvalidHandle_ReturnsFalse()
        {
            using var harness = new TestAnchorNavHostHarness();

            var released = harness.Host.ReleaseStateHandle(9999, restore: false);

            Assert.IsFalse(released);
        }

        [Test]
        public void ReleaseStateHandle_ValidRestoreFalse_RemovesSavedStateOnly()
        {
            using var harness = new TestAnchorNavHostHarness();

            var handle = harness.Host.SaveStateHandle();

            Assert.IsTrue(harness.Host.ReleaseStateHandle(handle, restore: false));
            Assert.IsFalse(harness.Host.ReleaseStateHandle(handle, restore: false));
        }

        [Test]
        public void ReleaseStateHandle_ValidRestoreTrue_RestoresSavedNavigation()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");

            harness.Host.Navigate("base");
            harness.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });
            var handle = harness.Host.SaveStateHandle();

            harness.Host.ClearNavigation();

            var released = harness.Host.ReleaseStateHandle(handle, restore: true);

            Assert.IsTrue(released);
            Assert.AreEqual("popup", harness.Host.CurrentDestination);
            Assert.IsTrue(harness.Host.HasActivePopups);
        }

        [Test]
        public void RestoredState_PreservesPopupBaseLayering()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");

            harness.Host.Navigate("base");
            harness.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            var state = harness.Host.SaveState();

            harness.Host.ClearNavigation();
            harness.Host.RestoreState(state);

            Assert.AreEqual("popup", harness.Host.CurrentDestination);
            Assert.IsTrue(harness.Host.HasActivePopups);
            Assert.IsTrue(harness.Host.ClosePopup("popup"));
            Assert.AreEqual("base", harness.Host.CurrentDestination);
            Assert.IsFalse(harness.Host.HasActivePopups);
        }
    }
}
