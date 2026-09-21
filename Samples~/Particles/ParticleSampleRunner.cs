namespace BovineLabs.Anchor.Particles.Sample
{
    using System;
    using System.Collections;
    using System.IO;
    using UnityEngine;
    using UnityEngine.UIElements;

    public sealed class ParticleSampleRunner : MonoBehaviour
    {
        [SerializeField] private VisualTreeAsset _visualTree;
        [SerializeField] private ThemeStyleSheet _theme;
        private ParticleSamplePresenter _presenter;
        private UIDocument _document;
        private PanelSettings _settings;

        private void Awake()
        {
            _settings = ScriptableObject.CreateInstance<PanelSettings>();
            _settings.themeStyleSheet = _theme;
            _settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            _settings.clearColor = true;
            _settings.colorClearValue = new Color(0.04f, 0.05f, 0.08f, 1);
            _document = gameObject.AddComponent<UIDocument>();
            _document.panelSettings = _settings;
            _document.visualTreeAsset = _visualTree;
            _presenter = new ParticleSamplePresenter(_document.rootVisualElement);
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

            var button = _document.rootVisualElement.Q<Button>("play");
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
            _presenter.Dispose();
            _presenter = null;
            yield return ParticleRuntimeMeasurements.Run(_document.rootVisualElement, output);
            Application.Quit();
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
            // UIDocument can already have detached its tree during GameObject destruction.
            if (_document != null)
            {
                _document.rootVisualElement?.Clear();
            }

            Destroy(_settings);
        }
    }
}
