namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Rendering")]
    public class RenderingToolbarViewModel : ObservableObject, IToolbarElement
    {
        private int _qualityValue;
        private int _wireframeValue;

        [Preserve]
        public RenderingToolbarViewModel()
        {
            QualityChoices = QualitySettings.names.ToList();
            Update();
        }

        [CreateProperty(ReadOnly = true)]
        public List<string> QualityChoices { get; }

        [CreateProperty]
        public int QualityValue
        {
            get => _qualityValue;
            set
            {
                if (SetProperty(ref _qualityValue, value))
                {
                    QualitySettings.SetQualityLevel(_qualityValue);
                }
            }
        }

        public List<string> WireframeChoices { get; } = new()
        {
            "Off",
            "Wireframe",
            "Solid Wireframe",
            "Shaded Wireframe",
        };

        [CreateProperty]
        public int WireframeValue
        {
            get => _wireframeValue;
            set
            {
                if (SetProperty(ref _wireframeValue, value))
                {
                    UniversalRenderPipelineDebugDisplaySettings.Instance.renderingSettings.wireframeMode = (DebugWireframeMode)value;
                }
            }
        }

        public VisualElement CreateElement()
        {
            return new RenderingToolbarView(this);
        }

        public void Update()
        {
            QualityValue = QualitySettings.GetQualityLevel();
            WireframeValue = (int)UniversalRenderPipelineDebugDisplaySettings.Instance.renderingSettings.wireframeMode;
        }
    }
}
