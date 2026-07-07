// <copyright file="PauseToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_NERVE
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor.Debug.ViewModels;
    using JetBrains.Annotations;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using Toggle = Unity.AppUI.UI.Toggle;
    using UnityEngine.Scripting;

    [Preserve]
    [Transient]
    [UsedImplicitly]
    public class PauseToolbarView : View<PauseToolbarViewModel>
    {
        [Preserve]
        public PauseToolbarView()
            : base(new PauseToolbarViewModel())
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
    }
}
#endif
