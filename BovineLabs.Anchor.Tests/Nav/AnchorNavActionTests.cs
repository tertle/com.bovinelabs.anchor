// <copyright file="AnchorNavActionTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;

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
                    AnchorNavArgument.String("a", "1"),
                    AnchorNavArgument.String("b", "2"),
                });

            var merged = action.MergeArguments(
                AnchorNavArgument.String("b", "22"),
                AnchorNavArgument.String("c", "3"));

            Assert.AreEqual(3, merged.Length);
            Assert.AreEqual(AnchorNavArgument.String("a", "1"), merged[0]);
            Assert.AreEqual(AnchorNavArgument.String("b", "22"), merged[1]);
            Assert.AreEqual(AnchorNavArgument.String("c", "3"), merged[2]);
        }

        [Test]
        public void MergeArguments_IgnoresNullArguments()
        {
            var action = new AnchorNavAction(
                "destination",
                new AnchorNavOptions(),
                new[]
                {
                    AnchorNavArgument.String("x", "1"),
                });

            var merged = action.MergeArguments(null, AnchorNavArgument.String("y", "2"), null);

            Assert.AreEqual(2, merged.Length);
            Assert.AreEqual(AnchorNavArgument.String("x", "1"), merged[0]);
            Assert.AreEqual(AnchorNavArgument.String("y", "2"), merged[1]);
        }
    }
}
