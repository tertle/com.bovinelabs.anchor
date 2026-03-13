// <copyright file="SequenceComparerTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Utility
{
    using System.Collections.Generic;
    using NUnit.Framework;

    public class SequenceComparerTests
    {
        [Test]
        public void SequenceComparer_UsesElementWiseEquality()
        {
            var comparer = new SequenceComparer<int>();

            Assert.IsTrue(comparer.Equals(new[] { 1, 2, 3 }, new[] { 1, 2, 3 }));
            Assert.IsFalse(comparer.Equals(new[] { 1, 2, 3 }, new[] { 1, 3, 2 }));
        }

        [Test]
        public void SequenceListComparer_UsesElementWiseEquality()
        {
            var comparer = new SequenceListComparer<int>();
            var left = new List<int> { 10, 20 };
            var same = new List<int> { 10, 20 };
            var different = new List<int> { 20, 10 };

            Assert.IsTrue(comparer.Equals(left, same));
            Assert.IsFalse(comparer.Equals(left, different));
        }
    }
}
