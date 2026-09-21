#if BL_NERVE
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class PauseToolbarViewModel : SystemObservableObject<PauseToolbarViewModel.Data>, IToolbarElement
    {
        [CreateProperty]
        public bool Pause
        {
            get => Value.Pause;
            set => SetProperty(ref Value.Pause, value);
        }

        public VisualElement CreateElement()
        {
            return new PauseToolbarView(this);
        }

        public struct Data
        {
            public bool Pause;
        }
    }
}
#endif
