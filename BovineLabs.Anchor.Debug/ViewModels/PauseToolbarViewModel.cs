// <copyright file="PauseToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_CORE_EXTENSIONS && !BL_DISABLE_PAUSE
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.MVVM;
    using Unity.Properties;

    public class PauseToolbarViewModel : ObservableObject, IBindingObject<PauseToolbarViewModel.Data>
    {
        private Data data;

        public ref Data Value => ref this.data;

        [CreateProperty]
        public bool Pause
        {
            get => this.data.Pause;
            set => this.SetProperty(ref this.data.Pause, value);
        }

        public struct Data
        {
            public bool Pause;
        }
    }
}
#endif
