namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core;
    using BovineLabs.Core.Utility;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AnchorAppBuilder : AnchorAppBuilder<AnchorApp>
    {
    }

    [RequireComponent(typeof(PanelRenderer))]
    public abstract class AnchorAppBuilder<T> : MonoBehaviour
        where T : AnchorApp, new()
    {
        private PanelRenderer _panelRenderer;

        private AnchorServiceProvider _serviceProvider;
        private T _anchorApp;
        private VisualElement _hostRootVisualElement;
        private VisualElement _appRootVisualElement;
        private IAnchorNavHostReloadState _pendingNavigationState;
        private bool _visualGenerationActive;
        private int _lastPanelVersion = -1;

        protected IReadOnlyList<StyleSheet> DebugStyleSheets => AnchorSettings.I.DebugStyleSheets;

        protected virtual Type LocalStorageService { get; } = typeof(LocalStoragePlayerPrefsService);

        protected virtual Type ViewModelService { get; } = typeof(ViewModelService);

        protected virtual Type AudioService { get; } = typeof(AudioService);

        protected virtual Type UXMLService { get; } = typeof(UXMLService);

        /// <summary>
        /// Must implement IAnchorPanel and have a public parameterless constructor.
        /// </summary>
        protected virtual Type PanelType { get; } = typeof(AnchorPanel);

        private void Awake()
        {
            _panelRenderer = GetComponent<PanelRenderer>();
        }

        private void OnEnable()
        {
            _hostRootVisualElement = null;
            _panelRenderer.RegisterUIReloadCallback(OnPanelRendererReload);
            ((IPanelComponent)_panelRenderer).PerformValidation(true);
        }

        private void OnDisable()
        {
            _panelRenderer.UnregisterUIReloadCallback(OnPanelRendererReload);
            _lastPanelVersion = -1;

            try
            {
                ReleaseCurrentVisualGeneration();
            }
            finally
            {
                _hostRootVisualElement = null;
            }
        }

        private void OnDestroy()
        {
            _panelRenderer.UnregisterUIReloadCallback(OnPanelRendererReload);
            ShutdownApp();
        }

        private void Update()
        {
            _anchorApp?.Update();
        }

        protected virtual void OnConfigureServices(AnchorServiceCollection services)
        {
            services.AddSingleton(typeof(ILocalStorageService), LocalStorageService);
            services.AddSingleton(typeof(IViewModelService), ViewModelService);
            services.AddSingleton(typeof(IAudioService), AudioService);

            if (UXMLService != null)
            {
                services.AddSingleton(typeof(IUXMLService), UXMLService);
            }

            // Register all services
            foreach (var service in ReflectionUtility.GetAllWithAttribute<IsServiceAttribute>())
            {
                var isTransient = service.GetCustomAttribute<TransientAttribute>() != null;
                if (isTransient)
                {
                    services.AddTransient(service);
                }
                else
                {
                    services.AddSingleton(service);
                }

                if (!isTransient && typeof(IAnchorToolbarHost).IsAssignableFrom(service))
                {
                    services.AddAlias(typeof(IAnchorToolbarHost), service);
                }
            }
        }

        protected virtual void OnAppInitialized(T app)
        {
        }

        protected virtual void OnVisualGenerationInitialized(T app)
        {
#if UNITY_INCLUDE_INSTRUMENTATION
            foreach (var style in DebugStyleSheets)
            {
                app.RootVisualElement.styleSheets.Add(style);
            }
#endif

            app.Initialize();
            app.InitializeToolbar();
        }

        protected virtual void OnVisualGenerationShuttingDown(T app)
        {
        }

        protected virtual void OnAppShuttingDown(T app)
        {
        }

        private void InitializeApp()
        {
            var services = new AnchorServiceCollection();
            OnConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();
            _anchorApp = new T();

            try
            {
                _anchorApp.Initialize(_serviceProvider);
                OnAppInitialized(_anchorApp);
            }
            catch
            {
                _anchorApp.Dispose();
                _serviceProvider.Dispose();
                _anchorApp = null;
                _serviceProvider = null;
                throw;
            }
        }

        private IAnchorPanel CreatePanel()
        {
            var panelType = PanelType ?? typeof(AnchorPanel);
            if (!typeof(IAnchorPanel).IsAssignableFrom(panelType))
            {
                throw new InvalidOperationException(
                    $"{nameof(PanelType)} '{panelType.FullName}' must implement {nameof(IAnchorPanel)}.");
            }

            if (panelType.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(PanelType)} '{panelType.FullName}' must have a public parameterless constructor.");
            }

            return (IAnchorPanel)Activator.CreateInstance(panelType);
        }

        private void InitializeVisualGeneration(IAnchorNavHostReloadState navigationState)
        {
            var panel = CreatePanel();
            var root = panel.RootVisualElement ??
                throw new InvalidOperationException($"{panel.GetType().FullName} returned a null root visual element.");

            _appRootVisualElement = root;

            try
            {
                root.pickingMode = PickingMode.Ignore;
                _anchorApp.SetPanel(panel);
                AttachAppRootToHost();
                _visualGenerationActive = true;

                _anchorApp.RestoringNavigationState = navigationState != null;
                try
                {
                    OnVisualGenerationInitialized(_anchorApp);
                }
                finally
                {
                    _anchorApp.RestoringNavigationState = false;
                }

                if (navigationState != null)
                {
                    if (_anchorApp.NavHost == null)
                    {
                        throw new InvalidOperationException("The visual generation did not initialize a navigation host.");
                    }

                    _anchorApp.NavHost.RestoreReloadState(navigationState);
                }

                _anchorApp.RefreshScreenMetrics();
            }
            catch
            {
                ReleaseVisualGeneration();
                throw;
            }
        }

        private void ShutdownApp()
        {
            try
            {
                if (_anchorApp != null)
                {
                    try
                    {
                        InvokeVisualGenerationShuttingDown();
                    }
                    finally
                    {
                        try
                        {
                            OnAppShuttingDown(_anchorApp);
                        }
                        finally
                        {
                            _appRootVisualElement?.RemoveFromHierarchy();
                            _anchorApp.Dispose();
                        }
                    }
                }
            }
            finally
            {
                _serviceProvider?.Dispose();

                _anchorApp = null;
                _serviceProvider = null;
                _appRootVisualElement = null;
                _hostRootVisualElement = null;
                _pendingNavigationState = null;
                _visualGenerationActive = false;
            }
        }

        private void ReleaseVisualGeneration()
        {
            try
            {
                InvokeVisualGenerationShuttingDown();
            }
            finally
            {
                try
                {
                    _appRootVisualElement?.RemoveFromHierarchy();
                    _anchorApp.ReleaseVisualGeneration();
                }
                finally
                {
                    _appRootVisualElement = null;
                }
            }
        }

        private void ReleaseCurrentVisualGeneration()
        {
            if (_anchorApp?.RootVisualElement == null)
            {
                return;
            }

            CaptureNavigationState();
            ReleaseVisualGeneration();
        }

        private void CaptureNavigationState()
        {
            if (_pendingNavigationState != null)
            {
                return;
            }

            var navHost = _anchorApp.NavHost ??
                throw new InvalidOperationException("The current visual generation does not have a navigation host.");
            _pendingNavigationState = navHost.CaptureReloadState() ??
                throw new InvalidOperationException($"{navHost.GetType().FullName} returned a null navigation reload state.");
        }

        private void InvokeVisualGenerationShuttingDown()
        {
            if (!_visualGenerationActive)
            {
                return;
            }

            _visualGenerationActive = false;
            OnVisualGenerationShuttingDown(_anchorApp);
        }

        private void AttachAppRootToHost()
        {
            if (_hostRootVisualElement == null || _appRootVisualElement == null)
            {
                return;
            }

            _hostRootVisualElement.Clear();
            _hostRootVisualElement.Add(_appRootVisualElement);
        }

        private void OnPanelRendererReload(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            if (version == _lastPanelVersion)
            {
                return;
            }

            _hostRootVisualElement = rootElement ?? throw new ArgumentNullException(nameof(rootElement));

            try
            {
                if (_anchorApp == null)
                {
                    InitializeApp();
                }

                ReleaseCurrentVisualGeneration();

                InitializeVisualGeneration(_pendingNavigationState);
                _pendingNavigationState = null;
                _lastPanelVersion = version;
            }
            catch (Exception ex)
            {
                BLGlobalLogger.LogErrorString($"Failed to build Anchor panel visual generation {version}: {ex}");
                throw;
            }
        }
    }
}
