#if BL_NERVE
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using JetBrains.Annotations;
    using Unity.Properties;
    using Toggle = Unity.AppUI.UI.Toggle;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [UsedImplicitly]
    public class PauseToolbarView : VisualElement
    {
        [Preserve]
        public PauseToolbarView(PauseToolbarViewModel viewModel)
        {
            dataSource = viewModel;

            var pauseToggle = new Toggle
            {
                label = "Pause",
            };

            pauseToggle.SetBinding(nameof(Toggle.value), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(PauseToolbarViewModel.Pause)),
            });

            Add(pauseToggle);
        }
    }
}
#endif
