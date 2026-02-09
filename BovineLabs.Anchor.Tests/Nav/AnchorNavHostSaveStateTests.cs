// <copyright file="AnchorNavHostSaveStateTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;
    using Unity.AppUI.Navigation;

    public class AnchorNavHostSaveStateTests
    {
        [Test]
        public void StackItem_ClonesOptionsAndArguments()
        {
            var options = new AnchorNavOptions
            {
                PopupBaseDestination = "baseA",
                PopupBaseArguments = new[] { new Argument("argA", "1") },
            };
            var arguments = new[] { new Argument("nameA", "valueA") };

            var item = new AnchorNavHostSaveState.StackItem("dest", options, arguments, true);

            options.PopupBaseDestination = "baseB";
            arguments[0] = new Argument("nameB", "valueB");

            Assert.AreNotSame(options, item.Options);
            Assert.AreNotSame(arguments, item.Arguments);
            Assert.AreEqual("baseA", item.Options.PopupBaseDestination);
            Assert.AreEqual(new Argument("nameA", "valueA"), item.Arguments[0]);
            Assert.IsTrue(item.IsPopup);
        }

        [Test]
        public void BackStackEntry_ClonesOptionsAndArguments()
        {
            var options = new AnchorNavOptions
            {
                PopupBaseDestination = "baseA",
                PopupBaseArguments = new[] { new Argument("argA", "1") },
            };
            var arguments = new[] { new Argument("nameA", "valueA") };
            var snapshot = new[]
            {
                new AnchorNavHostSaveState.StackItem("snap", new AnchorNavOptions(), new[] { new Argument("s", "1") }, false),
            };

            var entry = new AnchorNavHostSaveState.BackStackEntry("dest", options, arguments, snapshot);

            options.PopupBaseDestination = "baseB";
            arguments[0] = new Argument("nameB", "valueB");

            Assert.AreNotSame(options, entry.Options);
            Assert.AreNotSame(arguments, entry.Arguments);
            Assert.AreEqual("baseA", entry.Options.PopupBaseDestination);
            Assert.AreEqual(new Argument("nameA", "valueA"), entry.Arguments[0]);
            Assert.AreSame(snapshot, entry.Snapshot);
        }
    }
}
