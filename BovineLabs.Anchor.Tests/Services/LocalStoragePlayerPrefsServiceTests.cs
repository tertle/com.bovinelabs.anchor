// <copyright file="LocalStoragePlayerPrefsServiceTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using BovineLabs.Anchor.Services;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;

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
                this.service.DeleteJson(key);
                this.service.DeleteBytes(key);
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

        [Test]
        public void JsonGetSet_HandlesValidPayload_AndFallbackOnInvalidPayload()
        {
            var validKey = this.NewKey();
            var invalidKey = this.NewKey();

            var payload = new TestPayload { IntValue = 7, StringValue = "abc" };
            this.service.SetJson(validKey, payload);

            var loaded = this.service.GetJson<TestPayload>(validKey, default);
            Assert.IsNotNull(loaded);
            Assert.AreEqual(7, loaded.IntValue);
            Assert.AreEqual("abc", loaded.StringValue);

            PlayerPrefs.SetString(invalidKey, "{ invalid json");
            var fallback = new TestPayload { IntValue = 99, StringValue = "fallback" };
            LogAssert.Expect(LogType.Exception, new Regex("JSON parse error"));
            var invalidLoaded = this.service.GetJson(invalidKey, fallback);

            Assert.AreSame(fallback, invalidLoaded);
        }

        [Test]
        public void Bytes_FlowSetHasGetDelete_Works()
        {
            var key = this.NewKey();
            var bytes = new byte[] { 1, 2, 3, 4 };

            this.service.SetBytes(key, bytes);

            Assert.IsTrue(this.service.HasBytes(key));
            CollectionAssert.AreEqual(bytes, this.service.GetBytes(key));

            this.service.DeleteBytes(key);

            Assert.IsFalse(this.service.HasBytes(key));
            Assert.IsNull(this.service.GetBytes(key));
        }

        private string NewKey()
        {
            var key = $"anchor-tests-{Guid.NewGuid():N}";
            this.keysToCleanup.Add(key);
            return key;
        }

        [Serializable]
        private class TestPayload
        {
            public int IntValue;
            public string StringValue;
        }
    }
}
