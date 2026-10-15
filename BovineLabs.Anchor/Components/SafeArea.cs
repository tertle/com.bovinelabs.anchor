namespace BovineLabs.Anchor.Components
{
    using System;
    using Unity.Properties;
    using UnityEngine.UIElements;

    /// <summary>Applies safe-area padding to the unsafe edges overlapped by the owner's bounds.</summary>
    [VisualElementComponent]
    public partial struct SafeArea : IVisualElementComponent
    {
        [CreateProperty]
        [UxmlAttribute("edges")]
        public AnchorSafeAreaEdges Edges;

        private SafeAreaSubscription _subscription;

        [UxmlCreateInstanceMethod]
        public static SafeArea Create()
        {
            return new SafeArea
            {
                Edges = AnchorSafeAreaEdges.All,
            };
        }

        [OnComponentAdded]
        private static void OnAdded(ref SafeArea self, VisualElement owner)
        {
            self._subscription = new SafeAreaSubscription(owner);
            self._subscription.Attach(owner.panel?.visualTree);
        }

        [OnComponentChanged]
        private static void OnChanged(ref SafeArea self, VisualElement owner)
        {
            self._subscription.UpdatePadding();
        }

        [OnComponentRemoved]
        private static void OnRemoved(ref SafeArea self, VisualElement owner)
        {
            self._subscription.Dispose();
            AnchorSafeAreaUtility.ResetPadding(owner.style);
        }

        [ReleaseComponentResources]
        private static void Release(ref SafeArea self)
        {
            self._subscription.Dispose();
        }

        [RegisterCallback(typeof(AttachToPanelEvent))]
        private static void OnAttachToPanel(ref SafeArea self, VisualElement owner, AttachToPanelEvent evt)
        {
            self._subscription.Attach(evt.destinationPanel?.visualTree);
        }

        [RegisterCallback(typeof(DetachFromPanelEvent))]
        private static void OnDetachFromPanel(ref SafeArea self, VisualElement owner, DetachFromPanelEvent evt)
        {
            self._subscription.Detach();
            AnchorSafeAreaUtility.ResetPadding(owner.style);
        }

        [RegisterCallback(typeof(GeometryChangedEvent))]
        private static void OnGeometryChanged(ref SafeArea self, VisualElement owner, GeometryChangedEvent evt)
        {
            self._subscription.UpdatePadding();
        }

        private sealed class SafeAreaSubscription : IDisposable
        {
            private readonly VisualElement _owner;
            private AnchorApp _anchorApp;
            private VisualElement _panelRoot;
            private bool _disposed;

            public SafeAreaSubscription(VisualElement owner)
            {
                _owner = owner;
            }

            public void Attach(VisualElement root)
            {
                if (!ReferenceEquals(_panelRoot, root))
                {
                    Detach();
                    _panelRoot = root;

                    if (_panelRoot != null)
                    {
                        _panelRoot.RegisterCallback<GeometryChangedEvent>(OnPanelRootGeometryChanged);
                        _anchorApp = AnchorApp.Current;
                        if (_anchorApp != null)
                        {
                            _anchorApp.ScreenMetricsChanged += OnScreenMetricsChanged;
                        }
                    }
                }

                UpdatePadding();
            }

            public void Detach()
            {
                if (_anchorApp != null)
                {
                    _anchorApp.ScreenMetricsChanged -= OnScreenMetricsChanged;
                }

                _panelRoot?.UnregisterCallback<GeometryChangedEvent>(OnPanelRootGeometryChanged);
                _anchorApp = null;
                _panelRoot = null;
            }

            public void UpdatePadding()
            {
                // A screen-metrics notification may already have captured this subscription when it is removed.
                if (_disposed)
                {
                    return;
                }

                AnchorSafeAreaUtility.ApplyPadding(_owner, _owner, _panelRoot, _owner.GetComponent<SafeArea>().Edges);
            }

            public void Dispose()
            {
                _disposed = true;
                Detach();
            }

            private void OnPanelRootGeometryChanged(GeometryChangedEvent evt)
            {
                UpdatePadding();
            }

            private void OnScreenMetricsChanged(AnchorScreenMetrics metrics)
            {
                UpdatePadding();
            }
        }
    }
}
