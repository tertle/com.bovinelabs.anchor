// <copyright file="ViewModelServiceTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Services
{
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;

    public class ViewModelServiceTests
    {
        [Test]
        public void Load_CachesAndReturnsSameInstance()
        {
            using var scope = new TestAppServiceScope(typeof(TestObservableObject));
            var service = new ViewModelService();

            var first = service.Load<TestObservableObject>();
            var second = service.Load<TestObservableObject>();

            Assert.AreSame(first, second);
        }

        [Test]
        public void Get_ReturnsNullBeforeLoad_AndLoadedAfterLoad()
        {
            using var scope = new TestAppServiceScope(typeof(TestObservableObject));
            var service = new ViewModelService();

            Assert.IsNull(service.Get<TestObservableObject>());

            var loaded = service.Load<TestObservableObject>();

            Assert.AreSame(loaded, service.Get<TestObservableObject>());
        }

        [Test]
        public void Unload_RemovesCachedInstance()
        {
            using var scope = new TestAppServiceScope(typeof(TestObservableObject));
            var service = new ViewModelService();

            service.Load<TestObservableObject>();
            Assert.IsNotNull(service.Get<TestObservableObject>());

            service.Unload<TestObservableObject>();

            Assert.IsNull(service.Get<TestObservableObject>());
        }
    }
}
