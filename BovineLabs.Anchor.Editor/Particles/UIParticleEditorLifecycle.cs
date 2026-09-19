namespace BovineLabs.Anchor.Editor.Particles
{
    using BovineLabs.Anchor.Particles;
    using UnityEditor;

    [InitializeOnLoad]
    internal static class UIParticleEditorLifecycle
    {
        static UIParticleEditorLifecycle()
        {
            AssemblyReloadEvents.beforeAssemblyReload += UIParticleCoordinator.Reset;
            EditorApplication.quitting += UIParticleCoordinator.Reset;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
            {
                UIParticleCoordinator.Reset();
            }
        }
    }
}
