// <copyright file="AnchorAppBuilderTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Reflection;
    using BovineLabs.Anchor.Audio;
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
            using var provider = CreateAudioServiceProvider();

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

        [Test]
        public void OnConfigureServices_CustomAudioServiceRegistrationWins()
        {
            var gameObject = new GameObject("builder");
            var builder = gameObject.AddComponent<TestAnchorBuilder>();
            var services = new AnchorServiceCollection();
            var audioService = new TestAudioService();

            try
            {
                builder.InvokeConfigure(services);
                services.AddSingletonInstance(typeof(IAudioService), audioService);

                using var provider = services.BuildServiceProvider();

                Assert.AreSame(audioService, provider.GetRequiredService<IAudioService>());
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void PanelRendererReload_RebuildsAppAndRestoresNavigationStateAfterReleasedVisualTree()
        {
            var gameObject = new GameObject("builder");
            gameObject.SetActive(false);
            gameObject.AddComponent<PanelRenderer>();
            var builder = gameObject.AddComponent<LifecycleTestAnchorBuilder>();
            InvokeLifecycleMethod(builder, "Awake");
            var firstHostRoot = new VisualElement();
            var reloadedHostRoot = new VisualElement();

            try
            {
                InvokePanelRendererReload(builder, firstHostRoot, 1);

                var firstApp = AnchorApp.Current as LifecycleTestAnchorApp;
                Assert.IsNotNull(firstApp);
                firstApp.NavHost.CurrentDestination = "preserved-destination";

                var firstAppRoot = firstApp.RootVisualElement;
                Assert.AreEqual(1, firstHostRoot.childCount);
                Assert.AreSame(firstHostRoot, firstAppRoot.parent);

                ReleaseVisualTreeResources(firstAppRoot);
                Assert.IsTrue(firstAppRoot.resourcesReleased);

                Assert.DoesNotThrow(() => InvokePanelRendererReload(builder, reloadedHostRoot, 2));

                var reloadedApp = AnchorApp.Current as LifecycleTestAnchorApp;
                Assert.IsNotNull(reloadedApp);
                Assert.AreNotSame(firstApp, reloadedApp);
                Assert.IsNull(firstApp.RootVisualElement);
                Assert.AreNotSame(firstAppRoot, reloadedApp.RootVisualElement);
                Assert.AreEqual(1, reloadedHostRoot.childCount);
                Assert.AreSame(reloadedHostRoot, reloadedApp.RootVisualElement.parent);
                Assert.AreEqual("preserved-destination", reloadedApp.NavHost.CurrentDestination);

                InvokePanelRendererReload(builder, reloadedHostRoot, 2);
                Assert.AreSame(reloadedApp, AnchorApp.Current);
            }
            finally
            {
                InvokeLifecycleMethod(builder, "OnDestroy");
                Object.DestroyImmediate(gameObject);
            }
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

        private static AnchorServiceProvider CreateAudioServiceProvider()
        {
            var services = new AnchorServiceCollection();
            services.AddSingleton(typeof(IAudioService), typeof(AudioService));
            return services.BuildServiceProvider();
        }

        private static void InvokePanelRendererReload(object instance, VisualElement hostRoot, int version)
        {
            var method = instance.GetType().BaseType?.GetMethod("OnPanelRendererReload", BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(instance.GetType().FullName, "OnPanelRendererReload");
            }

            method.Invoke(instance, new object[] { null, hostRoot, version });
        }

        private static void InvokeLifecycleMethod(object instance, string methodName)
        {
            var method = instance.GetType().BaseType?.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(instance.GetType().FullName, methodName);
            }

            method.Invoke(instance, null);
        }

        private static void ReleaseVisualTreeResources(VisualElement root)
        {
            root.RemoveFromHierarchy();

            while (root.hierarchy.childCount > 0)
            {
                ReleaseVisualTreeResources(root.hierarchy[0]);
            }

            root.ReleaseResources();
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

        private sealed class TestBuilderApp : AnchorApp
        {
            public int InitializeCalls { get; private set; }

            public override void Initialize()
            {
                this.InitializeCalls++;
                this.NavHost ??= new AnchorNavHost();
            }
        }

        private sealed class LifecycleTestAnchorBuilder : AnchorAppBuilder<LifecycleTestAnchorApp>
        {
            protected override void OnConfigureServices(AnchorServiceCollection services)
            {
            }
        }

        private sealed class LifecycleTestAnchorApp : AnchorApp
        {
            public override void Initialize()
            {
                var navHost = new AnchorNavHost();
                this.NavHost = navHost;
                this.RootVisualElement.Add(navHost);
            }
        }

        private sealed class TestAudioService : IAudioService
        {
            public void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
            {
            }
        }
    }
}
