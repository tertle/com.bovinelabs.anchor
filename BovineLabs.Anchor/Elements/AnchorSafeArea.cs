// <copyright file="AnchorSafeArea.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Elements
{
#if UNITY_APPUI
    using Unity.AppUI.UI;
#endif
    using Unity.Properties;
    using UnityEngine.UIElements;

    /// <summary>
    /// Safe-area-aware container that applies padding only for the unsafe edges overlapped by its own bounds.
    /// </summary>
    [UxmlElement]
#if UNITY_APPUI
    public partial class AnchorSafeArea : VisualElement // ExVisualElement TODO figure out issues with this
#else
    public partial class AnchorSafeArea : VisualElement
#endif
    {
        /// <summary>The main styling class for the safe-area wrapper.</summary>
        public const string UssClassName = "bl-anchor-safe-area";

        private AnchorApp anchorApp;
        private VisualElement panelRoot;
        private AnchorSafeAreaEdges safeAreaEdges = AnchorSafeAreaEdges.All;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorSafeArea"/> class.
        /// </summary>
        public AnchorSafeArea()
        {
            this.AddToClassList(UssClassName);

            this.RegisterCallback<AttachToPanelEvent>(this.OnAttachToPanel);
            this.RegisterCallback<DetachFromPanelEvent>(this.OnDetachFromPanel);
            this.RegisterCallback<GeometryChangedEvent>(this.OnGeometryChanged);
        }

        /// <summary>
        /// Gets or sets the unsafe edges that should be applied when overlapped.
        /// </summary>
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
