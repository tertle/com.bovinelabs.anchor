// <copyright file="LocalStoragePlayerPrefsServiceTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Services;
    using NUnit.Framework;

    public class LocalStoragePlayerPrefsServiceTests
    {
        private readonly List<string> keysToCleanup = new();
        private LocalStoragePlayerPrefsService service;

        [SetUp]
        public void SetUp()
        {
            this.service = new LocalStoragePlayerPrefsService();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var key in this.keysToCleanup)
            {
                this.service.DeleteKey(key);
            }

            this.keysToCleanup.Clear();
        }

        [Test]
        public void PrimitiveValues_RoundTripStringIntAndBool()
        {
            var stringKey = this.NewKey();
            var intKey = this.NewKey();
            var boolKey = this.NewKey();

            this.service.SetValue(stringKey, "hello");
            this.service.SetValue(intKey, 123);
            this.service.SetValue(boolKey, true);

            Assert.AreEqual("hello", this.service.GetValue(stringKey, "fallback"));
            Assert.AreEqual(123, this.service.GetValue(intKey, 0));
            Assert.IsTrue(this.service.GetValue(boolKey, false));
        }

        private string NewKey()
        {
            var key = $"anchor-tests-{Guid.NewGuid():N}";
            this.keysToCleanup.Add(key);
            return key;
        }
    }
}
