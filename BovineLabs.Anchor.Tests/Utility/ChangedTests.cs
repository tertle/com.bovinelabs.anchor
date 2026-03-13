// <copyright file="ChangedTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Utility
{
    using NUnit.Framework;

    public class ChangedTests
    {
        [Test]
        public void ImplicitConversion_SetsHasChangedTrue()
        {
            Changed<int> changed = 5;

            Assert.AreEqual(5, changed.Value);
            Assert.IsTrue(changed.HasChanged);
        }
    }
}
