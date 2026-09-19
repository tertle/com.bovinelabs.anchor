#if UNITY_EDITOR
namespace BovineLabs.Anchor.Particles.Sample
{
    using System.IO;
    using BovineLabs.Anchor.Elements;
    using UnityEditor;
    using UnityEngine.UIElements;

    public sealed class ParticleRuntimePreview : EditorWindow
    {
        private UIParticleEffect effect;

        [MenuItem("Window/Anchor/Particle runtime example")]
        private static void Open() => GetWindow<ParticleRuntimePreview>("Particle runtime");

        private void CreateGUI()
        {
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var folder = Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(folder + "/RuntimeParticles.uxml").CloneTree(this.rootVisualElement);
            this.effect = ParticleRuntimeExample.CreateEffect();
            var particles = this.rootVisualElement.Q<AnchorParticles>("effect");
            particles.Effect = this.effect;
            this.rootVisualElement.Q<Button>("play").clicked += () => particles.Play(7);
            this.rootVisualElement.Q<Button>("stop").clicked += particles.StopEmitting;
            this.rootVisualElement.Q<Button>("pause").clicked += particles.Pause;
            this.rootVisualElement.Q<Button>("resume").clicked += particles.Resume;
            this.rootVisualElement.Q<Button>("clear").clicked += particles.Clear;
            this.rootVisualElement.Q<Button>("step").clicked += () => particles.Advance(1d / 60);
            particles.Play(7);
        }

        private void OnDisable()
        {
            this.rootVisualElement.Clear();
            if (this.effect != null)
            {
                DestroyImmediate(this.effect);
                this.effect = null;
            }
        }
    }
}
#endif
