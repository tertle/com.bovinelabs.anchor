// <copyright file="AnchorAppBuilderTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using UnityEngine;
    using Object = UnityEngine.Object;

    public class AnchorAppBuilderTests
    {
        [Test]
        public void OnConfigureServices_RegistersDefaultServices()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilder>();
            var services = new AnchorServiceCollection();

            try
            {
                builder.InvokeConfigure(services);

                AssertService(services, typeof(ILocalStorageService), typeof(TestLocalStorageService));
                AssertService(services, typeof(IViewModelService), typeof(ViewModelService));
                AssertService(services, typeof(IUXMLService), typeof(TestUxmlService));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void OnConfigureServices_WithNullUxmlService_DoesNotRegisterUxml()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilderWithoutUxml>();
            var services = new AnchorServiceCollection();

            try
            {
                builder.InvokeConfigure(services);

                AssertService(services, typeof(ILocalStorageService), typeof(TestLocalStorageService));
                AssertService(services, typeof(IViewModelService), typeof(ViewModelService));
                Assert.IsFalse(services.Any(d => d.ServiceType == typeof(IUXMLService)));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void OnAppInitialized_RestoresSavedNavigationState()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilder>();
            var app = new TestBuilderApp();
            using var provider = new AnchorServiceProvider(new AnchorServiceCollection());

            try
            {
                app.Initialize(provider, new AnchorPanel());
                SetStateField(
                    builder,
                    new AnchorNavHostSaveState(
                        "restored",
                        null,
                        null,
                        Array.Empty<AnchorNavHostSaveState.StackItem>(),
                        Array.Empty<AnchorNavHostSaveState.BackStackEntry>()));

                builder.InvokeInitialized(app);

                Assert.AreEqual(1, app.InitializeCalls);
                Assert.IsNotNull(app.NavHost);
                Assert.AreEqual("restored", app.NavHost.CurrentDestination);
            }
            finally
            {
                app.Dispose();
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void OnAppShuttingDown_CapturesCurrentNavState()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilder>();
            var app = new TestBuilderApp();
            using var provider = new AnchorServiceProvider(new AnchorServiceCollection());

            try
            {
                app.Initialize(provider, new AnchorPanel());
                app.Initialize();
                app.NavHost.CurrentDestination = "saved-destination";

                builder.InvokeShuttingDown(app);
                var state = GetStateField(builder);

                Assert.IsNotNull(state);
                Assert.AreEqual("saved-destination", state.CurrentDestination);
            }
            finally
            {
                app.Dispose();
                Object.DestroyImmediate(gameObject);
            }
        }

        private static void AssertService(AnchorServiceCollection services, Type serviceType, Type implementationType)
        {
            var descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);
            Assert.IsNotNull(descriptor, $"Expected {serviceType.Name} registration.");
            Assert.AreEqual(implementationType, descriptor.ImplementationType);
        }

        private static void SetStateField(object instance, AnchorNavHostSaveState state)
        {
            var field = instance.GetType().BaseType?.GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, "state");
            }

            field.SetValue(instance, state);
        }

        private static AnchorNavHostSaveState GetStateField(object instance)
        {
            var field = instance.GetType().BaseType?.GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, "state");
            }

            return (AnchorNavHostSaveState)field.GetValue(instance);
        }

        private sealed class TestAnchorBuilder : AnchorAppBuilder<TestBuilderApp>
        {
            protected override Type LocalStorageService => typeof(TestLocalStorageService);

            protected override Type ViewModelService => typeof(ViewModelService);

            protected override Type UXMLService => typeof(TestUxmlService);

            public void InvokeConfigure(AnchorServiceCollection services)
            {
                this.OnConfigureServices(services);
            }

            public void InvokeInitialized(TestBuilderApp app)
            {
                this.OnAppInitialized(app);
            }

            public void InvokeShuttingDown(TestBuilderApp app)
            {
                this.OnAppShuttingDown(app);
            }
        }

        private sealed class TestAnchorBuilderWithoutUxml : AnchorAppBuilder<TestBuilderApp>
        {
            protected override Type LocalStorageService => typeof(TestLocalStorageService);

            protected override Type ViewModelService => typeof(ViewModelService);

            protected override Type UXMLService => null;

            public void InvokeConfigure(AnchorServiceCollection services)
            {
                this.OnConfigureServices(services);
            }
        }

        private sealed class TestBuilderApp : AnchorApp
        {
            public int InitializeCalls { get; private set; }

            public override void Initialize()
            {
                this.InitializeCalls++;
                this.NavHost ??= new AnchorNavHost();
            }
        }
    }
}
