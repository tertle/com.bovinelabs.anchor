// <copyright file="AnchorNavStackSnapshotTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;

    public class AnchorNavStackSnapshotTests
    {
        [Test]
        public void WithoutPopups_NoPopups_ReturnsSameInstance()
        {
            var snapshot = new AnchorNavStackSnapshot(
                new[]
                {
                    new AnchorNavStackItem("a", new AnchorNavOptions(), null, false),
                    new AnchorNavStackItem("b", new AnchorNavOptions(), null, false),
                });

            var filtered = snapshot.WithoutPopups();

            Assert.AreSame(snapshot, filtered);
        }

        [Test]
        public void WithoutPopups_WithPopups_ReturnsFilteredSnapshot()
        {
            var snapshot = new AnchorNavStackSnapshot(
                new[]
                {
                    new AnchorNavStackItem("a", new AnchorNavOptions(), null, false),
                    new AnchorNavStackItem("popup", new AnchorNavOptions(), null, true),
                    new AnchorNavStackItem("b", new AnchorNavOptions(), null, false),
                });

            var filtered = snapshot.WithoutPopups();

            Assert.AreNotSame(snapshot, filtered);
            Assert.AreEqual(2, filtered.Items.Count);
            Assert.AreEqual("a", filtered.Items[0].Destination);
            Assert.AreEqual("b", filtered.Items[1].Destination);
            Assert.IsFalse(filtered.HasPopups);
        }
    }
}
