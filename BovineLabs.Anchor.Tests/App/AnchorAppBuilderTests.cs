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
    using UnityEngine.UIElements;
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

#if UNITY_6000_5_OR_NEWER
        [Test]
        public void OnPanelRendererReload_AttachesAppRootToReloadedHost()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilder>();
            var hostRoot = new VisualElement();
            hostRoot.Add(new Label("stale"));
            var app = new TestBuilderApp();
            using var provider = new AnchorServiceProvider(new AnchorServiceCollection());

            try
            {
                app.Initialize(provider, new AnchorPanel());
                SetField(builder, "anchorApp", app);
                SetField(builder, "appRootVisualElement", app.RootVisualElement);

                InvokePanelRendererReload(builder, hostRoot);

                Assert.AreEqual(1, hostRoot.childCount);
                Assert.AreSame(app.RootVisualElement, hostRoot[0]);
            }
            finally
            {
                app.Dispose();
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void AnchorAppBuilder_UsesOnlyPanelRendererField_OnUnity65Plus()
        {
            var builderType = typeof(TestAnchorBuilder).BaseType;

            Assert.IsNotNull(builderType?.GetField("panelRenderer", BindingFlags.Instance | BindingFlags.NonPublic));
            Assert.IsNull(builderType?.GetField("uiDocument", BindingFlags.Instance | BindingFlags.NonPublic));
        }
#endif

        private static void AssertService(AnchorServiceCollection services, Type serviceType, Type implementationType)
        {
            var descriptor = services.LastOrDefault(d => d.ServiceType == serviceType);
            Assert.IsNotNull(descriptor, $"Expected {serviceType.Name} registration.");
            Assert.AreEqual(implementationType, descriptor.ImplementationType);
        }

        private static void SetStateField(object instance, AnchorNavHostSaveState state)
        {
            SetField(instance, "state", state);
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().BaseType?.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, fieldName);
            }

            field.SetValue(instance, value);
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

#if UNITY_6000_5_OR_NEWER
        private static void InvokePanelRendererReload(object instance, VisualElement hostRoot)
        {
            var method = instance.GetType().BaseType?.GetMethod("OnPanelRendererReload", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(instance.GetType().FullName, "OnPanelRendererReload");
            }

            method.Invoke(instance, new object[] { null, hostRoot });
        }
#endif

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
