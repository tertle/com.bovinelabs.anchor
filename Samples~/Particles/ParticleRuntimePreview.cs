#if UNITY_EDITOR
namespace BovineLabs.Anchor.Particles.Sample
{
    using System.IO;
    using UnityEditor;
    using UnityEngine.UIElements;

    public sealed class ParticleRuntimePreview : EditorWindow
    {
        private ParticleSamplePresenter _presenter;
        private IVisualElementScheduledItem _clock;
        private double _lastTime;

        [MenuItem("BovineLabs/Samples/Anchor Particles")]
        private static void Open() => GetWindow<ParticleRuntimePreview>("UI particles");

        private void CreateGUI()
        {
            Release();
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var folder = Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(folder + "/RuntimeParticles.uxml").CloneTree(rootVisualElement);
            _presenter = new ParticleSamplePresenter(rootVisualElement);
            _lastTime = EditorApplication.timeSinceStartup;
            _clock = rootVisualElement.schedule.Execute(() =>
            {
                var now = EditorApplication.timeSinceStartup;
                _presenter.Advance(now - _lastTime);
                _lastTime = now;
            }).Every(16);
        }

        private void OnDisable() => Release();

        private void Release()
        {
            _clock?.Pause();
            _clock = null;
            _presenter?.Dispose();
            _presenter = null;
            rootVisualElement.Clear();
        }
    }
}
#endif
