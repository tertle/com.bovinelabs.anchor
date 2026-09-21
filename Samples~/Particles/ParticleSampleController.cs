namespace BovineLabs.Anchor.Particles.Sample
{
    using UnityEngine;
    using UnityEngine.UIElements;

    [RequireComponent(typeof(UIDocument))]
    public sealed class ParticleSampleController : MonoBehaviour
    {
        private ParticleSamplePresenter _presenter;
        private VisualElement _root;

        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.RegisterCallback<AttachToPanelEvent>(Attach);
            _root.RegisterCallback<DetachFromPanelEvent>(Detach);
            if (_root.panel != null)
            {
                _presenter = new ParticleSamplePresenter(_root);
            }
        }

        private void Attach(AttachToPanelEvent evt)
        {
            _presenter?.Dispose();
            _presenter = new ParticleSamplePresenter(_root);
        }

        private void Detach(DetachFromPanelEvent evt)
        {
            _presenter?.Dispose();
            _presenter = null;
        }

        private void OnDisable()
        {
            _presenter?.Dispose();
            _presenter = null;
            _root.UnregisterCallback<AttachToPanelEvent>(Attach);
            _root.UnregisterCallback<DetachFromPanelEvent>(Detach);
        }
    }
}
