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

        private AnchorApp anchorApp;
        private VisualElement panelRoot;
        private AnchorSafeAreaEdges safeAreaEdges = AnchorSafeAreaEdges.All;

        public AnchorSafeArea()
        {
            this.AddToClassList(UssClassName);

            this.RegisterCallback<AttachToPanelEvent>(this.OnAttachToPanel);
            this.RegisterCallback<DetachFromPanelEvent>(this.OnDetachFromPanel);
            this.RegisterCallback<GeometryChangedEvent>(this.OnGeometryChanged);
        }

        [UxmlAttribute("edges")]
        [CreateProperty]
        public AnchorSafeAreaEdges Edges
        {
            get => this.safeAreaEdges;
            set
            {
                if (this.safeAreaEdges == value)
                {
                    return;
                }

                this.safeAreaEdges = value;
                this.UpdateSafeArea();
            }
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            this.RegisterPanelRoot(evt.destinationPanel?.visualTree);
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            this.UnregisterPanelRoot();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            this.UpdateSafeArea();
        }

        private void RegisterPanelRoot(VisualElement root)
        {
            if (ReferenceEquals(this.panelRoot, root))
            {
                this.UpdateSafeArea();
                return;
            }

            this.UnregisterPanelRoot();
            this.panelRoot = root;

            if (this.panelRoot == null)
            {
                this.anchorApp = null;
                AnchorSafeAreaUtility.ResetPadding(this.style);
                return;
            }

            this.panelRoot.RegisterCallback<GeometryChangedEvent>(this.OnPanelRootGeometryChanged);

            this.anchorApp = AnchorApp.Current;
            if (this.anchorApp != null)
            {
                this.anchorApp.ScreenMetricsChanged += this.OnScreenMetricsChanged;
            }

            this.UpdateSafeArea();
        }

        private void UnregisterPanelRoot()
        {
            if (this.anchorApp != null)
            {
                this.anchorApp.ScreenMetricsChanged -= this.OnScreenMetricsChanged;
            }

            this.panelRoot?.UnregisterCallback<GeometryChangedEvent>(this.OnPanelRootGeometryChanged);

            this.anchorApp = null;
            this.panelRoot = null;
            AnchorSafeAreaUtility.ResetPadding(this.style);
        }

        private void OnPanelRootGeometryChanged(GeometryChangedEvent evt)
        {
            this.UpdateSafeArea();
        }

        private void OnScreenMetricsChanged(AnchorScreenMetrics metrics)
        {
            this.UpdateSafeArea();
        }

        private void UpdateSafeArea()
        {
            AnchorSafeAreaUtility.ApplyPadding(this, this, this.panelRoot, this.safeAreaEdges);
        }
    }
}
