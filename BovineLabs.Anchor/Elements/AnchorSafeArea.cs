namespace BovineLabs.Anchor.Elements
{
    using Unity.Properties;
    using UnityEngine.UIElements;

    /// <summary>
    /// Applies safe-area padding only to unsafe edges overlapped by this element's own bounds.
    /// </summary>
    [UxmlElement]
    public partial class AnchorSafeArea : VisualElement // ExVisualElement TODO figure out issues with this
    {
        public const string UssClassName = "bl-anchor-safe-area";

        private AnchorApp _anchorApp;
        private VisualElement _panelRoot;
        private AnchorSafeAreaEdges _safeAreaEdges = AnchorSafeAreaEdges.All;

        public AnchorSafeArea()
        {
            AddToClassList(UssClassName);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        [UxmlAttribute("edges")]
        [CreateProperty]
        public AnchorSafeAreaEdges Edges
        {
            get => _safeAreaEdges;
            set
            {
                if (_safeAreaEdges == value)
                {
                    return;
                }

                _safeAreaEdges = value;
                UpdateSafeArea();
            }
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            RegisterPanelRoot(evt.destinationPanel?.visualTree);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            UnregisterPanelRoot();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateSafeArea();
        }

        private void RegisterPanelRoot(VisualElement root)
        {
            if (ReferenceEquals(_panelRoot, root))
            {
                UpdateSafeArea();
                return;
            }

            UnregisterPanelRoot();
            _panelRoot = root;

            if (_panelRoot == null)
            {
                _anchorApp = null;
                AnchorSafeAreaUtility.ResetPadding(style);
                return;
            }

            _panelRoot.RegisterCallback<GeometryChangedEvent>(OnPanelRootGeometryChanged);

            _anchorApp = AnchorApp.Current;
            if (_anchorApp != null)
            {
                _anchorApp.ScreenMetricsChanged += OnScreenMetricsChanged;
            }

            UpdateSafeArea();
        }

        private void UnregisterPanelRoot()
        {
            if (_anchorApp != null)
            {
                _anchorApp.ScreenMetricsChanged -= OnScreenMetricsChanged;
            }

            _panelRoot?.UnregisterCallback<GeometryChangedEvent>(OnPanelRootGeometryChanged);

            _anchorApp = null;
            _panelRoot = null;
            AnchorSafeAreaUtility.ResetPadding(style);
        }

        private void OnPanelRootGeometryChanged(GeometryChangedEvent evt)
        {
            UpdateSafeArea();
        }

        private void OnScreenMetricsChanged(AnchorScreenMetrics metrics)
        {
            UpdateSafeArea();
        }

        private void UpdateSafeArea()
        {
            AnchorSafeAreaUtility.ApplyPadding(this, this, _panelRoot, _safeAreaEdges);
        }
    }
}
