namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class RenderingToolbarView : VisualElement
    {
        public const string UssClassName = "bl-rendering-tab";

        [Preserve]
        public RenderingToolbarView(RenderingToolbarViewModel viewModel)
        {
            dataSource = viewModel;
            AddToClassList(UssClassName);

            var quality = new Dropdown
            {
                defaultMessage = string.Empty,
                bindItem = (item, i) => item.label = viewModel.QualityChoices[i],
            };

            quality.SetBindingToUI(nameof(Dropdown.sourceItems), nameof(RenderingToolbarViewModel.QualityChoices));
            quality.SetBindingTwoWay(nameof(Dropdown.selectedIndex), nameof(RenderingToolbarViewModel.QualityValue));

            var wireframe = new Dropdown(viewModel.WireframeChoices,
                (item, i) => item.label = viewModel.WireframeChoices[i],
                defaultIndices: new[] { viewModel.WireframeValue })
            {
                name = "wireframe",
                tooltip = "Set URP's wireframe mode for all cameras, including Entities Graphics.",
            };

            wireframe.SetBindingTwoWay(nameof(Dropdown.selectedIndex), nameof(RenderingToolbarViewModel.WireframeValue));

#if !UNITY_ENABLE_CHECKS
            wireframe.SetEnabled(false);
            wireframe.tooltip = "URP wireframe requires a Checked or Debug Managed Code Variant and retained runtime debug shaders.";
#endif

            Add(new ToolbarRow("Quality", quality));
            Add(new ToolbarRow("Wireframe", wireframe));

            schedule.Execute(viewModel.Update).Every(1); // Every frame
        }
    }
}
