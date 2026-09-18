#if UNITY_EDITOR
namespace BovineLabs.Anchor.Particles.Sample
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEditor.Build.Profile;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.UIElements;

    public static class ParticleFixtureBuild
    {
        // Run from an imported sample. Generates its scene as an explicit authoring operation, never from tests.
        public static void Build()
        {
            var script = AssetDatabase.FindAssets("ParticleFixtureRunner t:MonoScript", new[] { "Assets" });
            if (script.Length != 1)
            {
                throw new InvalidOperationException("Import exactly one copy of the particle sample before building.");
            }

            var folder = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(script[0])).Replace('\\', '/');
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.04f, 0.05f, 0.08f, 1);
            camera.cullingMask = 0;
            var runner = new GameObject("Particle fixture").AddComponent<ParticleFixtureRunner>();
            var serialized = new SerializedObject(runner);
            serialized.FindProperty("fixture").objectReferenceValue = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(folder + "/Particles.uxml");
            serialized.FindProperty("theme").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(folder + "/Particles.tss");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var scenePath = folder + "/ParticleFixture.unity";
            EditorSceneManager.SaveScene(scene, scenePath);

            var oldAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            var oldAutomatic = PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64);
            var oldTiming = PlayerSettings.enableFrameTimingStats;
            var oldProfile = BuildProfile.GetActiveBuildProfile();
            var oldDevelopment = EditorUserBuildSettings.development;
            var addressableSettings = AssetDatabase.FindAssets("t:AddressableAssetSettings");
            SerializedObject addressables = null;
            SerializedProperty contentBuild = null;
            var oldContentBuild = 0;
            if (addressableSettings.Length == 1)
            {
                addressables = new SerializedObject(AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(addressableSettings[0])));
                contentBuild = addressables.FindProperty("m_BuildAddressablesWithPlayerBuild");
                oldContentBuild = contentBuild.enumValueIndex;
            }

            try
            {
                BuildProfile.SetActiveBuildProfile(null);
                EditorUserBuildSettings.development = false;
                if (contentBuild != null)
                {
                    // Addressables PlayerBuildOption.DoNotBuildWithPlayer; this scene has no addressable content.
                    contentBuild.enumValueIndex = Array.IndexOf(contentBuild.enumNames, "DoNotBuildWithPlayer");
                    addressables.ApplyModifiedPropertiesWithoutUndo();
                }

                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D12 });
                PlayerSettings.enableFrameTimingStats = true;
                AssetDatabase.SaveAssets();
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { scenePath },
                    locationPathName = "Temp/AnchorParticles/Player/AnchorParticles.exe",
                    target = BuildTarget.StandaloneWindows64,
                    subtarget = (int)StandaloneBuildSubtarget.Player,
                    options = BuildOptions.Development | BuildOptions.StrictMode,
                    extraScriptingDefines = new[] { "UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP" },
                });
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException($"Particle fixture build failed: {report.summary.result}");
                }
            }
            finally
            {
                if (contentBuild != null)
                {
                    contentBuild.enumValueIndex = oldContentBuild;
                    addressables.ApplyModifiedPropertiesWithoutUndo();
                    addressables.Dispose();
                }

                PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, oldAPIs);
                PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, oldAutomatic);
                PlayerSettings.enableFrameTimingStats = oldTiming;
                EditorUserBuildSettings.development = oldDevelopment;
                BuildProfile.SetActiveBuildProfile(oldProfile);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
#endif
