namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class WireframeToolbarView : VisualElement
    {
        public const string UssClassName = "bl-wireframe-tab";

        [Preserve]
        public WireframeToolbarView(WireframeToolbarViewModel viewModel)
        {
            dataSource = viewModel;
            AddToClassList(UssClassName);

            var wireframe = new Dropdown(viewModel.WireframeChoices,
                (item, i) => item.label = viewModel.WireframeChoices[i],
                defaultIndices: new[] { viewModel.WireframeValue })
            {
                name = "wireframe",
                tooltip = "Set URP's wireframe mode for all cameras, including Entities Graphics.",
            };

            wireframe.SetBindingTwoWay(nameof(Dropdown.selectedIndex), nameof(WireframeToolbarViewModel.WireframeValue));

#if !UNITY_ENABLE_CHECKS
            wireframe.SetEnabled(false);
            wireframe.tooltip = "URP wireframe requires a Checked or Debug Managed Code Variant and retained runtime debug shaders.";
#endif

            Add(wireframe);
            schedule.Execute(viewModel.Update).Every(1);
        }
    }
}
