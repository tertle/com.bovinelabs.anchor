namespace BovineLabs.Anchor.Particles.Sample
{
    using System;
    using System.Collections;
    using System.IO;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class ParticleSampleRunner : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset visualTree;
        [SerializeField] private ThemeStyleSheet theme;
        private ParticleSamplePresenter presenter;
        private UIDocument document;
        private PanelSettings settings;

        private void Awake()
        {
            this.settings = ScriptableObject.CreateInstance<PanelSettings>();
            this.settings.themeStyleSheet = this.theme;
            this.settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            this.settings.clearColor = true;
            this.settings.colorClearValue = new Color(0.04f, 0.05f, 0.08f, 1);
            this.document = this.gameObject.AddComponent<UIDocument>();
            this.document.panelSettings = this.settings;
            this.document.visualTreeAsset = this.visualTree;
            this.presenter = new ParticleSamplePresenter(this.document.rootVisualElement);
        }

        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            var argument = Array.IndexOf(args, "--particle-runtime-results");
            if (argument < 0)
            {
                yield break;
            }

            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            Application.runInBackground = true;
            for (var frame = 0; frame < 30; frame++)
            {
                yield return null;
            }

            var button = this.document.rootVisualElement.Q<Button>("play");
            using (var submit = NavigationSubmitEvent.GetPooled())
            {
                submit.target = button;
                button.SendEvent(submit);
            }

            yield return new WaitForSecondsRealtime(0.35f);
            var output = Path.GetFullPath(args[argument + 1]);
            Directory.CreateDirectory(output);
            ScreenCapture.CaptureScreenshot(Path.Combine(output, "runtime-samples.png"));
            yield return null;
            yield return null;
            this.presenter.Dispose();
            this.presenter = null;
            yield return ParticleRuntimeMeasurements.Run(this.document.rootVisualElement, output);
            Application.Quit();
        }

        private void OnDestroy()
        {
            this.presenter?.Dispose();
            // UIDocument can already have detached its tree during GameObject destruction.
            if (this.document != null)
            {
                this.document.rootVisualElement?.Clear();
            }

            Destroy(this.settings);
        }
    }
}
