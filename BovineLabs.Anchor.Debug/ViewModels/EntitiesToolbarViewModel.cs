namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public partial class EntitiesToolbarViewModel : SystemObservableObject<EntitiesToolbarViewModel.Data>, IToolbarElement
    {
        [CreateProperty(ReadOnly = true)]
        public int Entities => Value.Entities;

        [CreateProperty(ReadOnly = true)]
        public int Archetypes => Value.Archetypes;

        [CreateProperty(ReadOnly = true)]
        public int Chunks => Value.Chunks;

        public VisualElement CreateElement()
        {
            return new EntitiesToolbarView(this);
        }

        public partial struct Data
        {
            [SystemProperty]
            private int _entities;

            [SystemProperty]
            private int _archetypes;

            [SystemProperty]
            private int _chunks;
        }
    }
}
