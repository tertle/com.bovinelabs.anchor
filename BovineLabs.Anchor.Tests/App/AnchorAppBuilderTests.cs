// <copyright file="AnchorAppBuilderTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Linq;
    using System.Reflection;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using Unity.AppUI.MVVM;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public class AnchorAppBuilderTests
    {
        [Test]
        public void OnConfiguringApp_RegistersDefaultServices()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilder>();

            try
            {
                var appBuilder = AppBuilder.InstantiateWith<TestBuilderApp, UIToolkitHost>();
                builder.InvokeConfigure(appBuilder);

                AssertService(appBuilder.services, typeof(ILocalStorageService), typeof(TestLocalStorageService));
                AssertService(appBuilder.services, typeof(IViewModelService), typeof(ViewModelService));
                AssertService(appBuilder.services, typeof(IUXMLService), typeof(TestUxmlService));
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void OnConfiguringApp_WithNullUxmlService_DoesNotRegisterUxml()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilderWithoutUxml>();

            try
            {
                var appBuilder = AppBuilder.InstantiateWith<TestBuilderApp, UIToolkitHost>();
                builder.InvokeConfigure(appBuilder);

                AssertService(appBuilder.services, typeof(ILocalStorageService), typeof(TestLocalStorageService));
                AssertService(appBuilder.services, typeof(IViewModelService), typeof(ViewModelService));
                Assert.IsFalse(appBuilder.services.Any(d => d.serviceType == typeof(IUXMLService)));
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
            app.rootVisualElement = new VisualElement();

            try
            {
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
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void OnAppShuttingDown_CapturesCurrentNavState()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilder>();
            var app = new TestBuilderApp();
            app.Initialize();
            app.NavHost.CurrentDestination = "saved-destination";

            try
            {
                builder.InvokeShuttingDown(app);
                var state = GetStateField(builder);

                Assert.IsNotNull(state);
                Assert.AreEqual("saved-destination", state.CurrentDestination);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private static void AssertService(IServiceCollection services, Type serviceType, Type implementationType)
        {
            var descriptor = services.FirstOrDefault(d => d.serviceType == serviceType);
            Assert.IsNotNull(descriptor, $"Expected {serviceType.Name} registration.");
            Assert.AreEqual(implementationType, descriptor.implementationType);
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

            public void InvokeConfigure(AppBuilder appBuilder)
            {
                this.OnConfiguringApp(appBuilder);
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

            public void InvokeConfigure(AppBuilder appBuilder)
            {
                this.OnConfiguringApp(appBuilder);
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
