namespace BovineLabs.Anchor.Samples.Showcase
{
    using System;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Particles;
    using BovineLabs.Anchor.Services;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Hash128 = Unity.Entities.Hash128;

    public sealed class AnchorShowcaseAppBuilder : AnchorAppBuilder, IUXMLService
    {
        [NoAutoStaticsCleanup]
        private static readonly string[] Destinations = { "bindings", "collections", "particles", "animations", "ecs" };

        [SerializeField]
        private VisualTreeAsset _catalog;

        [SerializeField]
        private VisualTreeAsset[] _views;

        [SerializeField]
        private UIParticleEffect[] _effects;

        private AnchorNavAnimation[] _animations;

        [SerializeField]
        private Hash128 _ecsScene;

        private AnchorNavHost _navigation;
        private Button[] _tabs;
        private IDisposable _presenter;
        private EcsDashboardPresenter _dashboard;
        private int _activeScenario;

        public int ActiveScenario => _activeScenario;

        public void Configure(VisualTreeAsset catalog, VisualTreeAsset[] views, UIParticleEffect[] effects, Hash128 ecsScene)
        {
            _catalog = catalog;
            _views = views;
            _effects = effects;
            _ecsScene = ecsScene;
        }

        public void SelectScenario(int index)
        {
            if ((uint)index >= Destinations.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            _activeScenario = index;
            _navigation.Navigate(Destinations[index], new AnchorNavOptions { StackStrategy = AnchorStackStrategy.PopAll });
        }

        public VisualTreeAsset GetAsset(string assetName) => assetName switch
        {
            "bindings" => _views[0],
            "collections" => _views[1],
            "particles" => _views[2],
            "animations" => _views[3],
            "ecs" => _views[4],
            "animation-home" => _views[5],
            "animation-details" => _views[6],
            "animation-popup" => _views[7],
            _ => throw new ArgumentException($"Unknown showcase destination '{assetName}'.", nameof(assetName)),
        };

        public VisualElement Instantiate(string assetName)
        {
            var view = GetAsset(assetName).Instantiate();
            view.userData = assetName;
            foreach (var element in view.Query().Build())
            {
                if (element.dataSourceType != null)
                {
                    element.dataSource = AnchorApp.Current.Services.GetRequiredService(element.dataSourceType);
                }
            }

            return view;
        }

        protected override void OnConfigureServices(AnchorServiceCollection services)
        {
            base.OnConfigureServices(services);
            services.AddSingletonInstance(typeof(IUXMLService), this);
        }

        protected override void OnAppInitialized(AnchorApp app)
        {
            _animations = new AnchorNavAnimation[4];
            for (var index = 0; index < _animations.Length; index++)
            {
                var animation = ScriptableObject.CreateInstance<ShowcaseNavAnimation>();
                animation.Configure(index + 1, index % 2 == 0, index >= 2);
                _animations[index] = animation;
            }
        }

        protected override void OnAppShuttingDown(AnchorApp app)
        {
            foreach (var animation in _animations)
            {
                Destroy(animation);
            }

            _animations = null;
        }

        protected override void OnVisualGenerationInitialized(AnchorApp app)
        {
            // The showcase owns its routes and visual shell, independently of the host project's AnchorSettings.
            app.Theme = "dark";
            app.Scale = "medium";
            var catalog = app.RootVisualElement;
            _catalog.CloneTree(catalog);
            _navigation = catalog.Q<AnchorNavHost>("showcase-host");
            app.NavHost = _navigation;
            _navigation.EnteredDestination += OnEnteredDestination;
            _navigation.DestinationChanged += OnDestinationChanged;
            _tabs = new Button[Destinations.Length];
            for (var index = 0; index < _tabs.Length; index++)
            {
                var tab = catalog.Q<Button>($"scenario-{index}");
                tab.userData = index;
                tab.RegisterCallback<ClickEvent>(OnTabClicked);
                _tabs[index] = tab;
            }

            SelectScenario(_activeScenario);
        }

        protected override void OnVisualGenerationShuttingDown(AnchorApp app)
        {
            ReleasePresenter();
            _navigation.EnteredDestination -= OnEnteredDestination;
            _navigation.DestinationChanged -= OnDestinationChanged;
            foreach (var tab in _tabs)
            {
                tab.UnregisterCallback<ClickEvent>(OnTabClicked);
            }

            _tabs = null;
            _navigation = null;
        }

        private void LateUpdate() => _dashboard?.Update();

        private void OnTabClicked(ClickEvent evt)
        {
            var index = (int)((Button)evt.currentTarget).userData;
            if (index != _activeScenario)
            {
                SelectScenario(index);
            }
        }

        private void OnEnteredDestination(AnchorNavHost host, VisualElement view, AnchorNavArgument[] arguments)
        {
            ReleasePresenter();
            var destination = (string)view.userData;
            _activeScenario = Array.IndexOf(Destinations, destination);
            _presenter = destination switch
            {
                "particles" => new ParticleShowcasePresenter(view, _effects),
                "animations" => new AnimationShowcasePresenter(view, _animations),
                "ecs" => _dashboard = new EcsDashboardPresenter(_ecsScene),
                _ => null,
            };
        }

        private void OnDestinationChanged(AnchorNavHost host, string destination)
        {
            for (var index = 0; index < _tabs.Length; index++)
            {
                _tabs[index].EnableInClassList("is-selected", Destinations[index] == destination);
            }
        }

        private void ReleasePresenter()
        {
            _presenter?.Dispose();
            _presenter = null;
            _dashboard = null;
        }
    }
}
