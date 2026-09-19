#if UNITY_EDITOR
namespace BovineLabs.Anchor.Particles.Sample
{
    using System.IO;
    using UnityEditor;
    using UnityEngine.UIElements;

    public sealed class ParticleFixturePreview : EditorWindow
    {
        private int frame;

        [MenuItem("Window/Anchor/Particle rendering fixture")]
        private static void Open() => GetWindow<ParticleFixturePreview>("Particle fixture");

        private void CreateGUI()
        {
            var path = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            var folder = Path.GetDirectoryName(path).Replace('\\', '/');
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(folder + "/Particles.uxml").CloneTree(this.rootVisualElement);
            this.rootVisualElement.Q(className: "toolbar").Query<Button>().ForEach(button =>
            {
                if (button.name != "preview-step")
                {
                    button.style.display = DisplayStyle.None;
                }
            });
            this.rootVisualElement.Q<Button>("preview-step").clicked += () =>
                ParticleFixtureClock.PreviewStep(this.rootVisualElement.panel, ++this.frame);
        }

        private void OnDisable() => this.rootVisualElement.Clear();
    }
}
#endif
