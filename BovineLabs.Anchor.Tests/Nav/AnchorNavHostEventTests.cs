// <copyright file="AnchorNavHostEventTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;

    public class AnchorNavHostEventTests
    {
        [Test]
        public void DestinationChanged_FiresOnlyOnActualDestinationChange()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");

            var events = new List<string>();
            harness.Host.DestinationChanged += (_, destination) => events.Add(destination);

            harness.Host.Navigate("A");
            harness.Host.Navigate("A");

            CollectionAssert.AreEqual(new[] { "A" }, events);
        }

        [Test]
        public void EnteredExitedDestination_OrdersExitBeforeEnter_OnTransition()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("A");
            harness.RegisterScreen("B");

            var events = new List<string>();
            harness.Host.EnteredDestination += (_, ve, _) => events.Add($"enter:{ve.name}");
            harness.Host.ExitedDestination += (_, ve, _) => events.Add($"exit:{ve.name}");

            harness.Host.Navigate("A");
            harness.Host.Navigate("B");

            CollectionAssert.AreEqual(
                new[]
                {
                    "enter:A",
                    "exit:A",
                    "enter:B",
                },
                events);
        }

        [Test]
        public void PopupClose_EmitsExitForPopup()
        {
            using var harness = new TestAnchorNavHostHarness();
            harness.RegisterScreen("base");
            harness.RegisterScreen("popup");

            var events = new List<string>();
            harness.Host.EnteredDestination += (_, ve, _) => events.Add($"enter:{ve.name}");
            harness.Host.ExitedDestination += (_, ve, _) => events.Add($"exit:{ve.name}");

            harness.Host.Navigate("base");
            harness.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            events.Clear();
            harness.Host.ClosePopup("popup");

            CollectionAssert.AreEqual(new[] { "exit:popup" }, events);
        }

        [Test]
        public void NavigationScreenReceiver_GetsEnterAndExitArguments()
        {
            using var harness = new TestAnchorNavHostHarness();
            var aReceiver = harness.RegisterScreen("A");
            var bReceiver = harness.RegisterScreen("B");

            harness.Host.Navigate("A", AnchorNavArgument.String("k", "1"));
            harness.Host.Navigate("B", AnchorNavArgument.String("k", "2"));

            Assert.AreEqual(1, aReceiver.EnterCount);
            Assert.AreEqual(1, aReceiver.ExitCount);
            Assert.AreEqual(AnchorNavArgument.String("k", "1"), aReceiver.LastEnterArguments[0]);
            Assert.AreEqual(AnchorNavArgument.String("k", "1"), aReceiver.LastExitArguments[0]);

            Assert.AreEqual(1, bReceiver.EnterCount);
            Assert.AreEqual(0, bReceiver.ExitCount);
            Assert.AreEqual(AnchorNavArgument.String("k", "2"), bReceiver.LastEnterArguments[0]);
        }

        [Test]
        public void ArgumentsWithoutScreenHandler_IsSafeNoThrow()
        {
            using var scope = new TestAnchorAppScope(static services =>
            {
                services.AddSingleton(typeof(TestVisualElementFactory));
                services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
            });

            var host = new AnchorNavHost();
            scope.App.NavHost = host;

            Assert.DoesNotThrow(() => host.Navigate("no-handler", AnchorNavArgument.String("value", "1")));
            Assert.AreEqual("no-handler", host.CurrentDestination);
        }
    }
}
