namespace BovineLabs.Anchor.Particles.Sample
{
    using UnityEngine;
    using UnityEngine.UIElements;

    [RequireComponent(typeof(UIDocument))]
    public sealed class ParticleSampleController : MonoBehaviour
    {
        private ParticleSamplePresenter presenter;
        private VisualElement root;

        private void OnEnable()
        {
            this.root = this.GetComponent<UIDocument>().rootVisualElement;
            this.root.RegisterCallback<AttachToPanelEvent>(this.Attach);
            this.root.RegisterCallback<DetachFromPanelEvent>(this.Detach);
            if (this.root.panel != null)
            {
                this.presenter = new ParticleSamplePresenter(this.root);
            }
        }

        private void Attach(AttachToPanelEvent evt)
        {
            this.presenter?.Dispose();
            this.presenter = new ParticleSamplePresenter(this.root);
        }

        private void Detach(DetachFromPanelEvent evt)
        {
            this.presenter?.Dispose();
            this.presenter = null;
        }

        private void OnDisable()
        {
            this.presenter?.Dispose();
            this.presenter = null;
            this.root.UnregisterCallback<AttachToPanelEvent>(this.Attach);
            this.root.UnregisterCallback<DetachFromPanelEvent>(this.Detach);
        }
    }
}
