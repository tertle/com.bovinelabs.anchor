// <copyright file="AnchorNavActionTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;
    using Unity.AppUI.Navigation;

    public class AnchorNavActionTests
    {
        [Test]
        public void MergeArguments_OverridesByName_AndPreservesDefaults()
        {
            var action = new AnchorNavAction(
                "destination",
                new AnchorNavOptions(),
                new[]
                {
                    new Argument("a", "1"),
                    new Argument("b", "2"),
                });

            var merged = action.MergeArguments(
                new Argument("b", "22"),
                new Argument("c", "3"));

            Assert.AreEqual(3, merged.Length);
            Assert.AreEqual(new Argument("a", "1"), merged[0]);
            Assert.AreEqual(new Argument("b", "22"), merged[1]);
            Assert.AreEqual(new Argument("c", "3"), merged[2]);
        }

        [Test]
        public void MergeArguments_IgnoresNullArguments()
        {
            var action = new AnchorNavAction(
                "destination",
                new AnchorNavOptions(),
                new[]
                {
                    new Argument("x", "1"),
                });

            var merged = action.MergeArguments(null, new Argument("y", "2"), null);

            Assert.AreEqual(2, merged.Length);
            Assert.AreEqual(new Argument("x", "1"), merged[0]);
            Assert.AreEqual(new Argument("y", "2"), merged[1]);
        }
    }
}
