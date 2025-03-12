// <copyright file="PauseToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_PAUSE
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using Unity.Properties;

    public class PauseToolbarViewModel : SystemObservableObject<PauseToolbarViewModel.Data>
    {
        [CreateProperty]
        public bool Pause
        {
            get => this.Value.Pause;
            set => this.SetProperty(ref this.Value.Pause, value);
        }

        public struct Data
        {
            public bool Pause;
        }
    }
}
#endif
