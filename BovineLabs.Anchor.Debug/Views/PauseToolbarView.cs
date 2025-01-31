// <copyright file="PauseToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_PAUSE
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using JetBrains.Annotations;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using Toggle = Unity.AppUI.UI.Toggle;

    [Transient]
    [UsedImplicitly]
    public class PauseToolbarView : VisualElement, IView<PauseToolbarViewModel>
    {
        public PauseToolbarView()
        {
            var pauseToggle = new Toggle
            {
                label = "Pause",
                dataSource = this.ViewModel,
            };

            pauseToggle.SetBinding(nameof(Toggle.value), new DataBinding
            {
                dataSourcePath = new PropertyPath(nameof(PauseToolbarViewModel.Pause)),
            });

            this.Add(pauseToggle);
        }

        public PauseToolbarViewModel ViewModel { get; } = new();
    }
}
#endif
