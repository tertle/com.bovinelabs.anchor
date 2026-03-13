// <copyright file="AnchorNavHostSaveStateTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;

    public class AnchorNavHostSaveStateTests
    {
        [Test]
        public void StackItem_ClonesOptionsAndArguments()
        {
            var options = new AnchorNavOptions
            {
                PopupBaseDestination = "baseA",
                PopupBaseArguments = new[] { AnchorNavArgument.String("argA", "1") },
            };
            var arguments = new[] { AnchorNavArgument.String("nameA", "valueA") };

            var item = new AnchorNavHostSaveState.StackItem("dest", options, arguments, true);

            options.PopupBaseDestination = "baseB";
            arguments[0] = AnchorNavArgument.String("nameB", "valueB");

            Assert.AreNotSame(options, item.Options);
            Assert.AreNotSame(arguments, item.Arguments);
            Assert.AreEqual("baseA", item.Options.PopupBaseDestination);
            Assert.AreEqual(AnchorNavArgument.String("nameA", "valueA"), item.Arguments[0]);
            Assert.IsTrue(item.IsPopup);
        }

        [Test]
        public void BackStackEntry_ClonesOptionsAndArguments()
        {
            var options = new AnchorNavOptions
            {
                PopupBaseDestination = "baseA",
                PopupBaseArguments = new[] { AnchorNavArgument.String("argA", "1") },
            };
            var arguments = new[] { AnchorNavArgument.String("nameA", "valueA") };
            var snapshot = new[]
            {
                new AnchorNavHostSaveState.StackItem("snap", new AnchorNavOptions(), new[] { AnchorNavArgument.String("s", "1") }, false),
            };

            var entry = new AnchorNavHostSaveState.BackStackEntry("dest", options, arguments, snapshot);

            options.PopupBaseDestination = "baseB";
            arguments[0] = AnchorNavArgument.String("nameB", "valueB");

            Assert.AreNotSame(options, entry.Options);
            Assert.AreNotSame(arguments, entry.Arguments);
            Assert.AreEqual("baseA", entry.Options.PopupBaseDestination);
            Assert.AreEqual(AnchorNavArgument.String("nameA", "valueA"), entry.Arguments[0]);
            Assert.AreSame(snapshot, entry.Snapshot);
        }
    }
}
