#if UNITY_EDITOR
namespace BovineLabs.Anchor.Particles.Sample
{
    using System.IO;
    using UnityEditor;
    using UnityEngine.UIElements;

    public sealed class ParticleRuntimePreview : EditorWindow
    {
        private ParticleSamplePresenter presenter;
        private IVisualElementScheduledItem clock;
        private double lastTime;

        [MenuItem("Window/Anchor/Particle runtime example")]
        private static void Open() => GetWindow<ParticleRuntimePreview>("UI particles");

        private void CreateGUI()
        {
            this.Release();
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var folder = Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(folder + "/RuntimeParticles.uxml").CloneTree(this.rootVisualElement);
            this.presenter = new ParticleSamplePresenter(this.rootVisualElement);
            this.lastTime = EditorApplication.timeSinceStartup;
            this.clock = this.rootVisualElement.schedule.Execute(() =>
            {
                var now = EditorApplication.timeSinceStartup;
                this.presenter.Advance(now - this.lastTime);
                this.lastTime = now;
            }).Every(16);
        }

        private void OnDisable() => this.Release();

        private void Release()
        {
            this.clock?.Pause();
            this.clock = null;
            this.presenter?.Dispose();
            this.presenter = null;
            this.rootVisualElement.Clear();
        }
    }
}
#endif
