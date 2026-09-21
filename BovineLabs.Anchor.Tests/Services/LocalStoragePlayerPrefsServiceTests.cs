namespace BovineLabs.Anchor.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Services;
    using NUnit.Framework;

    public class LocalStoragePlayerPrefsServiceTests
    {
        private readonly List<string> _keysToCleanup = new();
        private LocalStoragePlayerPrefsService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new LocalStoragePlayerPrefsService();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var key in _keysToCleanup)
            {
                _service.DeleteKey(key);
            }

            _keysToCleanup.Clear();
        }

        [Test]
        public void PrimitiveValues_RoundTripStringIntAndBool()
        {
            var stringKey = NewKey();
            var intKey = NewKey();
            var boolKey = NewKey();

            _service.SetValue(stringKey, "hello");
            _service.SetValue(intKey, 123);
            _service.SetValue(boolKey, true);

            Assert.AreEqual("hello", _service.GetValue(stringKey, "fallback"));
            Assert.AreEqual(123, _service.GetValue(intKey, 0));
            Assert.IsTrue(_service.GetValue(boolKey, false));
        }

        private string NewKey()
        {
            var key = $"anchor-tests-{Guid.NewGuid():N}";
            _keysToCleanup.Add(key);
            return key;
        }
    }
}
