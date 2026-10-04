namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("Wireframe")]
    public class WireframeToolbarViewModel : ObservableObject, IToolbarElement
    {
        private int _wireframeValue;

        public WireframeToolbarViewModel()
        {
            WireframeValue = (int)UniversalRenderPipelineDebugDisplaySettings.Instance.renderingSettings.wireframeMode;
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
            return new WireframeToolbarView(this);
        }

        public void Update()
        {
            WireframeValue = (int)UniversalRenderPipelineDebugDisplaySettings.Instance.renderingSettings.wireframeMode;
        }
    }
}
