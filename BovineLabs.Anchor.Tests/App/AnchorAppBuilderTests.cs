// <copyright file="AnchorAppBuilderTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using BovineLabs.Anchor.Audio;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using BovineLabs.Anchor.Toolbar;
    using NUnit.Framework;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public unsafe class AnchorAppBuilderTests
    {
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
        public void PanelRendererReload_RetainsDurableStateAndReplacesReleasedVisualGeneration()
        {
            var previousStartDestination = GetAnchorSetting<string>("startDestination");
            var gameObject = CreateBuilderGameObject(out var builder);
            var firstHostRoot = new VisualElement();
            var reloadedHostRoot = new VisualElement();
            var thirdHostRoot = new VisualElement();
            var uiHelper = default(UIHelper<ReloadViewModel, ReloadData>);
            var uiBound = false;
            var shutdownCalls = 0;
            var destroyed = false;

            void OnShuttingDown()
            {
                shutdownCalls++;
            }

            AnchorApp.ShuttingDown += OnShuttingDown;

            try
            {
                SetAnchorSetting("startDestination", string.Empty);
                InvokePanelRendererReload(builder, firstHostRoot, 1);

                var app = (LifecycleTestAnchorApp)AnchorApp.Current;
                var provider = app.Services;
                var trackedService = provider.GetRequiredService<TrackedService>();
                var toolbarHost = provider.GetRequiredService<TrackingToolbarHost>();
                var factory = provider.GetRequiredService<TestVisualElementFactory>();
                factory.Register("A", static () => new VisualElement { name = "A" });
                factory.Register("B", static () => new VisualElement { name = "B" });
                factory.Register("P", static () => new VisualElement { name = "P" });
                uiHelper.Bind();
                uiBound = true;
                var viewModelService = provider.GetRequiredService<IViewModelService>();
                var viewModel = viewModelService.Get<ReloadViewModel>();
                var viewModelAddress = (IntPtr)UnsafeUtility.AddressOf(ref viewModel.Value);
                uiHelper.Binding.Counter = 17;

                var firstPanel = app.Panel;
                var firstAppRoot = app.RootVisualElement;
                var firstNavHost = (AnchorNavHost)app.NavHost;
                firstNavHost.Navigate("A");
                var savedHandle = firstNavHost.SaveStateHandle();
                firstNavHost.Navigate("B");
                firstNavHost.Navigate("P", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });
                app.Panel.Theme = "light";
                app.Panel.Scale = "large";

                Assert.AreEqual(1, builder.AppInitializedCalls);
                Assert.AreEqual(1, builder.VisualGenerationInitializedCalls);
                Assert.AreEqual(0, builder.VisualGenerationShuttingDownCalls);
                Assert.AreEqual(1, app.InitializeCalls);
                Assert.AreEqual(1, toolbarHost.CreateCalls);
                Assert.AreEqual(0, toolbarHost.ReleaseCalls);
                Assert.AreEqual(1, firstHostRoot.childCount);
                Assert.AreSame(firstHostRoot, firstAppRoot.parent);

                ReleaseVisualTreeResources(firstAppRoot);
                Assert.IsTrue(firstAppRoot.resourcesReleased);

                Assert.DoesNotThrow(() => InvokePanelRendererReload(builder, reloadedHostRoot, 2));

                Assert.AreSame(app, AnchorApp.Current);
                Assert.AreSame(provider, app.Services);
                Assert.AreSame(trackedService, provider.GetRequiredService<TrackedService>());
                Assert.AreSame(toolbarHost, provider.GetRequiredService<TrackingToolbarHost>());
                Assert.AreEqual(0, trackedService.DisposeCalls);
                Assert.AreEqual(0, shutdownCalls);
                Assert.AreEqual(1, builder.AppInitializedCalls);
                Assert.AreEqual(2, builder.VisualGenerationInitializedCalls);
                Assert.AreEqual(1, builder.VisualGenerationShuttingDownCalls);
                Assert.AreSame(firstAppRoot, builder.VisualGenerationShuttingDownRoots[0]);
                Assert.AreSame(firstNavHost, builder.VisualGenerationShuttingDownNavHosts[0]);
                Assert.AreEqual(2, app.InitializeCalls);
                Assert.AreEqual(2, toolbarHost.CreateCalls);
                Assert.AreEqual(1, toolbarHost.ReleaseCalls);
                Assert.AreNotSame(firstPanel, app.Panel);
                Assert.AreNotSame(firstAppRoot, app.RootVisualElement);
                Assert.AreNotSame(firstNavHost, app.NavHost);
                Assert.AreEqual("light", app.Panel.Theme);
                Assert.AreEqual("large", app.Panel.Scale);
                Assert.AreEqual(1, reloadedHostRoot.childCount);
                Assert.AreSame(reloadedHostRoot, app.RootVisualElement.parent);

                var reloadedNavHost = (AnchorNavHost)app.NavHost;
                var reloadedState = reloadedNavHost.SaveState();
                Assert.AreEqual("P", reloadedState.CurrentDestination);
                Assert.AreEqual(2, reloadedState.ActiveStack.Count);
                Assert.AreEqual("B", reloadedState.ActiveStack[0].Destination);
                Assert.AreEqual("P", reloadedState.ActiveStack[1].Destination);
                Assert.IsTrue(reloadedState.ActiveStack[1].IsPopup);
                Assert.AreEqual(2, reloadedState.BackStack.Count);
                Assert.AreSame(viewModel, viewModelService.Get<ReloadViewModel>());
                Assert.AreEqual(viewModelAddress, (IntPtr)UnsafeUtility.AddressOf(ref viewModel.Value));
                Assert.AreEqual(17, uiHelper.Binding.Counter);
                Assert.AreEqual(1, viewModel.LoadCalls);
                Assert.AreEqual(0, viewModel.UnloadCalls);

                Assert.IsTrue(reloadedNavHost.ReleaseStateHandle(savedHandle));
                Assert.AreEqual("A", reloadedNavHost.CurrentDestination);
                Assert.Greater(reloadedNavHost.SaveStateHandle(), savedHandle);

                var secondAppRoot = app.RootVisualElement;
                ReleaseVisualTreeResources(secondAppRoot);
                InvokePanelRendererReload(builder, thirdHostRoot, 3);

                Assert.AreSame(app, AnchorApp.Current);
                Assert.AreSame(provider, app.Services);
                Assert.AreNotSame(secondAppRoot, app.RootVisualElement);
                Assert.AreEqual("A", app.NavHost.CurrentDestination);
                Assert.AreSame(viewModel, viewModelService.Get<ReloadViewModel>());
                Assert.AreEqual(viewModelAddress, (IntPtr)UnsafeUtility.AddressOf(ref viewModel.Value));
                Assert.AreEqual(17, uiHelper.Binding.Counter);
                Assert.AreEqual(3, builder.VisualGenerationInitializedCalls);
                Assert.AreEqual(2, builder.VisualGenerationShuttingDownCalls);
                Assert.AreSame(secondAppRoot, builder.VisualGenerationShuttingDownRoots[1]);
                Assert.AreEqual(3, app.InitializeCalls);
                Assert.AreEqual(3, toolbarHost.CreateCalls);
                Assert.AreEqual(2, toolbarHost.ReleaseCalls);

                var currentRoot = app.RootVisualElement;
                var currentNavHost = app.NavHost;
                InvokePanelRendererReload(builder, thirdHostRoot, 3);
                Assert.AreSame(currentRoot, app.RootVisualElement);
                Assert.AreSame(currentNavHost, app.NavHost);
                Assert.AreEqual(3, builder.VisualGenerationInitializedCalls);
                Assert.AreEqual(2, builder.VisualGenerationShuttingDownCalls);

                uiHelper.Unbind();
                uiBound = false;
                Assert.AreEqual(1, viewModel.UnloadCalls);
                Assert.IsNull(viewModelService.Get<ReloadViewModel>());

                InvokeLifecycleMethod(builder, "OnDestroy");
                destroyed = true;

                Assert.AreEqual(1, shutdownCalls);
                Assert.AreEqual(1, builder.AppShuttingDownCalls);
                Assert.AreEqual(3, builder.VisualGenerationShuttingDownCalls);
                Assert.AreEqual(3, builder.VisualGenerationShuttingDownCallsAtAppShutdown);
                Assert.AreSame(currentRoot, builder.VisualGenerationShuttingDownRoots[2]);
                Assert.AreSame(currentNavHost, builder.VisualGenerationShuttingDownNavHosts[2]);
                Assert.AreEqual(1, trackedService.DisposeCalls);
                Assert.AreEqual(3, toolbarHost.ReleaseCalls);
                Assert.AreEqual(1, toolbarHost.DisposeCalls);
                Assert.IsNull(AnchorApp.Current);
            }
            finally
            {
                if (uiBound)
                {
                    uiHelper.Unbind();
                }

                if (!destroyed)
                {
                    InvokeLifecycleMethod(builder, "OnDestroy");
                }

                AnchorApp.ShuttingDown -= OnShuttingDown;
                SetAnchorSetting("startDestination", previousStartDestination);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void OnDisable_SamePanelVersionReactivationRetainsAppAndRebuildsVisualGeneration()
        {
            var previousStartDestination = GetAnchorSetting<string>("startDestination");
            var gameObject = CreateBuilderGameObject(out var builder);
            var firstHostRoot = new VisualElement();
            var reactivatedHostRoot = new VisualElement();
            var shutdownCalls = 0;

            void OnShuttingDown()
            {
                shutdownCalls++;
            }

            AnchorApp.ShuttingDown += OnShuttingDown;

            try
            {
                SetAnchorSetting("startDestination", string.Empty);
                InvokePanelRendererReload(builder, firstHostRoot, 7);

                var app = (LifecycleTestAnchorApp)AnchorApp.Current;
                var provider = app.Services;
                var trackedService = provider.GetRequiredService<TrackedService>();
                var firstRoot = app.RootVisualElement;
                app.NavHost.CurrentDestination = "preserved";

                InvokeLifecycleMethod(builder, "OnDisable");

                Assert.AreSame(app, AnchorApp.Current);
                Assert.AreSame(provider, app.Services);
                Assert.AreSame(trackedService, provider.GetRequiredService<TrackedService>());
                Assert.AreEqual(0, trackedService.DisposeCalls);
                Assert.AreEqual(0, shutdownCalls);
                Assert.IsNull(app.RootVisualElement);
                Assert.AreEqual(1, builder.AppInitializedCalls);
                Assert.AreEqual(1, builder.VisualGenerationInitializedCalls);
                Assert.AreEqual(1, builder.VisualGenerationShuttingDownCalls);

                InvokePanelRendererReload(builder, reactivatedHostRoot, 7);

                Assert.AreSame(app, AnchorApp.Current);
                Assert.AreSame(provider, app.Services);
                Assert.AreSame(trackedService, provider.GetRequiredService<TrackedService>());
                Assert.AreNotSame(firstRoot, app.RootVisualElement);
                Assert.AreSame(reactivatedHostRoot, app.RootVisualElement.parent);
                Assert.AreEqual("preserved", app.NavHost.CurrentDestination);
                Assert.AreEqual(0, trackedService.DisposeCalls);
                Assert.AreEqual(0, shutdownCalls);
                Assert.AreEqual(1, builder.AppInitializedCalls);
                Assert.AreEqual(2, builder.VisualGenerationInitializedCalls);
                Assert.AreEqual(1, builder.VisualGenerationShuttingDownCalls);
            }
            finally
            {
                InvokeLifecycleMethod(builder, "OnDestroy");
                AnchorApp.ShuttingDown -= OnShuttingDown;
                SetAnchorSetting("startDestination", previousStartDestination);
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void PanelRendererReload_FailedVisualGenerationCanRetrySameVersionWithoutDisposingApp()
        {
            var previousStartDestination = GetAnchorSetting<string>("startDestination");
            var gameObject = CreateBuilderGameObject(out var builder);
            var firstHostRoot = new VisualElement();
            var reloadedHostRoot = new VisualElement();

            try
            {
                SetAnchorSetting("startDestination", string.Empty);
                InvokePanelRendererReload(builder, firstHostRoot, 1);

                var app = (LifecycleTestAnchorApp)AnchorApp.Current;
                var provider = app.Services;
                var trackedService = provider.GetRequiredService<TrackedService>();
                app.NavHost.CurrentDestination = "preserved";
                ReleaseVisualTreeResources(app.RootVisualElement);

                builder.FailNextVisualGeneration = true;
                LogAssert.Expect(LogType.Error, new Regex("Failed to build Anchor panel visual generation 2"));
                Assert.Throws<TargetInvocationException>(() => InvokePanelRendererReload(builder, reloadedHostRoot, 2));

                Assert.AreSame(app, AnchorApp.Current);
                Assert.AreSame(provider, app.Services);
                Assert.AreEqual(0, trackedService.DisposeCalls);
                Assert.IsNull(app.RootVisualElement);
                Assert.AreEqual(2, builder.VisualGenerationShuttingDownCalls);

                Assert.DoesNotThrow(() => InvokePanelRendererReload(builder, reloadedHostRoot, 2));
                Assert.AreSame(app, AnchorApp.Current);
                Assert.AreSame(provider, app.Services);
                Assert.AreEqual("preserved", app.NavHost.CurrentDestination);
                Assert.AreEqual(3, builder.VisualGenerationInitializedCalls);
                Assert.AreEqual(2, builder.VisualGenerationShuttingDownCalls);
            }
            finally
            {
                InvokeLifecycleMethod(builder, "OnDestroy");
                SetAnchorSetting("startDestination", previousStartDestination);
                Object.DestroyImmediate(gameObject);
            }
        }

        private static GameObject CreateBuilderGameObject(out LifecycleTestAnchorBuilder builder)
        {
            var gameObject = new GameObject("builder");
            gameObject.SetActive(false);
            gameObject.AddComponent<PanelRenderer>();
            builder = gameObject.AddComponent<LifecycleTestAnchorBuilder>();
            InvokeLifecycleMethod(builder, "Awake");
            return gameObject;
        }

        private static T GetAnchorSetting<T>(string fieldName)
        {
            var field = typeof(AnchorSettings).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(typeof(AnchorSettings).FullName, fieldName);
            }

            return (T)field.GetValue(AnchorSettings.I);
        }

        private static void SetAnchorSetting(string fieldName, object value)
        {
            var field = typeof(AnchorSettings).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(typeof(AnchorSettings).FullName, fieldName);
            }

            field.SetValue(AnchorSettings.I, value);
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

        private sealed class TestAnchorBuilder : AnchorAppBuilder<AnchorApp>
        {
            protected override Type LocalStorageService => typeof(TestLocalStorageService);

            protected override Type ViewModelService => typeof(ViewModelService);

            protected override Type UXMLService => typeof(TestUxmlService);

            public void InvokeConfigure(AnchorServiceCollection services)
            {
                this.OnConfigureServices(services);
            }
        }

        private sealed class LifecycleTestAnchorBuilder : AnchorAppBuilder<LifecycleTestAnchorApp>
        {
            public int AppInitializedCalls { get; private set; }

            public int VisualGenerationInitializedCalls { get; private set; }

            public int VisualGenerationShuttingDownCalls { get; private set; }

            public int VisualGenerationShuttingDownCallsAtAppShutdown { get; private set; }

            public int AppShuttingDownCalls { get; private set; }

            public bool FailNextVisualGeneration { get; set; }

            public List<VisualElement> VisualGenerationShuttingDownRoots { get; } = new();

            public List<IAnchorNavHost> VisualGenerationShuttingDownNavHosts { get; } = new();

            protected override void OnConfigureServices(AnchorServiceCollection services)
            {
                services.AddSingleton(typeof(TestVisualElementFactory));
                services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
                services.AddSingleton(typeof(IViewModelService), typeof(ViewModelService));
                services.AddSingleton(typeof(ReloadViewModel));
                services.AddSingleton(typeof(TrackingToolbarHost));
                services.AddAlias(typeof(IAnchorToolbarHost), typeof(TrackingToolbarHost));
                services.AddSingleton(typeof(TrackedService));
            }

            protected override void OnAppInitialized(LifecycleTestAnchorApp app)
            {
                this.AppInitializedCalls++;
                base.OnAppInitialized(app);
            }

            protected override void OnVisualGenerationInitialized(LifecycleTestAnchorApp app)
            {
                this.VisualGenerationInitializedCalls++;

                if (this.FailNextVisualGeneration)
                {
                    this.FailNextVisualGeneration = false;
                    throw new InvalidOperationException("Test visual generation failure.");
                }

                base.OnVisualGenerationInitialized(app);
            }

            protected override void OnVisualGenerationShuttingDown(LifecycleTestAnchorApp app)
            {
                this.VisualGenerationShuttingDownCalls++;
                this.VisualGenerationShuttingDownRoots.Add(app.RootVisualElement);
                this.VisualGenerationShuttingDownNavHosts.Add(app.NavHost);
                base.OnVisualGenerationShuttingDown(app);
            }

            protected override void OnAppShuttingDown(LifecycleTestAnchorApp app)
            {
                this.VisualGenerationShuttingDownCallsAtAppShutdown = this.VisualGenerationShuttingDownCalls;
                this.AppShuttingDownCalls++;
                base.OnAppShuttingDown(app);
            }
        }

        private sealed class LifecycleTestAnchorApp : AnchorApp
        {
            public int InitializeCalls { get; private set; }

            public override void Initialize()
            {
                this.InitializeCalls++;
                base.Initialize();
            }
        }

        private sealed class TrackedService : IDisposable
        {
            public int DisposeCalls { get; private set; }

            public void Dispose()
            {
                this.DisposeCalls++;
            }
        }

        private sealed class TrackingToolbarHost : IAnchorToolbarHost, IDisposable
        {
            private VisualElement currentRoot;

            public int CreateCalls { get; private set; }

            public int ReleaseCalls { get; private set; }

            public int DisposeCalls { get; private set; }

            public VisualElement CreateRootVisualElement()
            {
                this.ReleaseRootVisualElement();
                this.currentRoot = new VisualElement { name = $"toolbar-{++this.CreateCalls}" };
                return this.currentRoot;
            }

            public void ReleaseRootVisualElement()
            {
                if (this.currentRoot == null)
                {
                    return;
                }

                this.currentRoot = null;
                this.ReleaseCalls++;
            }

            public void Dispose()
            {
                this.ReleaseRootVisualElement();
                this.DisposeCalls++;
            }
        }

        private sealed class ReloadViewModel : SystemObservableObject<ReloadData>, ILoadable
        {
            public int LoadCalls { get; private set; }

            public int UnloadCalls { get; private set; }

            public void Load()
            {
                this.LoadCalls++;
            }

            public void Unload()
            {
                this.UnloadCalls++;
            }
        }

        private struct ReloadData
        {
            public int Counter;
        }

        private sealed class TestAudioService : IAudioService
        {
            public void Play(string profileKey, AnchorAudioCue cue, AnchorAudioCueOverride cueOverride)
            {
            }
        }
    }
}
