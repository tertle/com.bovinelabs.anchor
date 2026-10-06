namespace BovineLabs.Anchor.Samples.Showcase.Editor
{
    using System;
    using System.IO;
    using BovineLabs.Anchor.Particles;
    using BovineLabs.Anchor.Samples.Showcase.Authoring;
    using BovineLabs.Core.Editor.Utility;
    using Unity.Scenes;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.UIElements;
    using Hash128 = Unity.Entities.Hash128;

    public static class AnchorShowcaseSampleBuilder
    {
        private static string SampleRoot
        {
            get
            {
                var scripts = AssetDatabase.FindAssets("AnchorShowcaseSampleBuilder t:MonoScript", new[] { "Assets" });
                if (scripts.Length != 1)
                {
                    throw new InvalidOperationException("Import exactly one Anchor Showcase sample before generating.");
                }

                var scriptPath = AssetDatabase.GUIDToAssetPath(scripts[0]);
                return scriptPath[..scriptPath.IndexOf("/Scripts/", StringComparison.Ordinal)];
            }
        }

        [MenuItem("BovineLabs/Samples/Anchor Generate")]
        public static void GenerateMenu()
        {
            if (SampleEditorUtility.SaveScenesForGeneration(out var discard))
            {
                Generate(discard);
            }
        }

        [MenuItem("BovineLabs/Samples/Anchor Generate", true)]
        [MenuItem("BovineLabs/Samples/Anchor Play", true)]
        private static bool ValidateMenu() => SampleEditorUtility.CanUseMenu;

        [MenuItem("BovineLabs/Samples/Anchor Play")]
        public static void Play() => SampleEditorUtility.Play($"{SampleRoot}/Generated/Scenes/Main.unity", () => Generate());

        public static void Generate() => Generate(false);

        private static void Generate(bool discardUnsavedScenes)
        {
            var root = SampleRoot;
            SampleEditorUtility.RebuildGeneratedAssets($"{root}/Generated", () => Build(root), discardUnsavedScenes);
        }

        [InitializeOnLoadMethod]
        private static void Initialize() => EditorApplication.delayCall += GenerateAfterImport;

        private static void GenerateAfterImport()
        {
            if (!SampleEditorUtility.CanGenerateAutomatically)
            {
                return;
            }

            var scripts = AssetDatabase.FindAssets("AnchorShowcaseSampleBuilder t:MonoScript", new[] { "Assets" });
            if (scripts.Length == 1 && !File.Exists($"{SampleRoot}/Generated/Scenes/Main.unity"))
            {
                Generate();
            }
        }

        private static void Build(string root)
        {
            var generated = $"{root}/Generated";
            Directory.CreateDirectory($"{generated}/Scenes/Main");
            Directory.CreateDirectory($"{generated}/Settings");
            AssetDatabase.Refresh();

            var panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
            panelSettings.themeStyleSheet = Load<ThemeStyleSheet>($"{root}/UI/Showcase.tss");
            panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            panelSettings.referenceResolution = new Vector2Int(1920, 1080);
            panelSettings.screenMatchMode = PanelScreenMatchMode.Expand;
            panelSettings.clearColor = true;
            panelSettings.colorClearValue = new Color(0.04f, 0.05f, 0.07f, 1);
            var textSettings = ScriptableObject.CreateInstance<PanelTextSettings>();
            AssetDatabase.CreateAsset(textSettings, $"{generated}/Settings/TextSettings.asset");
            panelSettings.textSettings = textSettings;
            AssetDatabase.CreateAsset(panelSettings, $"{generated}/Settings/PanelSettings.asset");

            var simulation = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("512 dashboard cells").AddComponent<EcsDashboardAuthoring>();
            var simulationPath = $"{generated}/Scenes/Main/Simulation.unity";
            EditorSceneManager.SaveScene(simulation, simulationPath);
            var simulationAsset = Load<SceneAsset>(simulationPath);

            var main = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Single-scene replacement can unload settings that were not yet referenced by a scene object.
            panelSettings = Load<PanelSettings>($"{generated}/Settings/PanelSettings.asset");
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = panelSettings.colorClearValue;
            camera.cullingMask = 0;
            var subScene = new GameObject("Dashboard data").AddComponent<SubScene>();
            subScene.SceneAsset = simulationAsset;
            subScene.AutoLoadScene = false;
            var sample = new GameObject("Anchor Showcase");
            sample.SetActive(false);
            var panel = sample.AddComponent<PanelRenderer>();
            panel.panelSettings = panelSettings;
            var catalog = Load<VisualTreeAsset>($"{root}/UI/Showcase.uxml");
            var serialized = new SerializedObject(panel);
            serialized.FindProperty("sourceAsset").objectReferenceValue = catalog;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var views = new VisualTreeAsset[8];
            var names = new[] { "Bindings", "Collections", "Particles", "Animations", "Ecs", "AnimationHome", "AnimationDetails", "AnimationPopup" };
            for (var index = 0; index < names.Length; index++)
            {
                views[index] = Load<VisualTreeAsset>($"{root}/UI/{names[index]}.uxml");
            }

            var effects = new UIParticleEffect[4];
            var effectNames = new[] { "Sparkle", "Confetti", "Dust", "Layered" };
            for (var index = 0; index < effects.Length; index++)
            {
                effects[index] = Load<UIParticleEffect>($"{root}/Effects/{effectNames[index]}.asset");
            }

            sample.AddComponent<AnchorShowcaseAppBuilder>().Configure(catalog, views, effects,
                new Hash128(AssetDatabase.AssetPathToGUID(simulationPath)));
            sample.SetActive(true);
            EditorSceneManager.SaveScene(main, $"{generated}/Scenes/Main.unity");
        }

        private static T Load<T>(string path)
            where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path) ?? throw new InvalidOperationException($"Missing sample asset '{path}'.");
        }
    }
}
