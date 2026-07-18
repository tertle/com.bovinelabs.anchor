// <copyright file="EntitiesToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

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
        public int Entities => this.Value.Entities;

        [CreateProperty(ReadOnly = true)]
        public int Archetypes => this.Value.Archetypes;

        [CreateProperty(ReadOnly = true)]
        public int Chunks => this.Value.Chunks;

        /// <inheritdoc />
        public VisualElement CreateElement()
        {
            return new EntitiesToolbarView(this);
        }

        public partial struct Data
        {
            [SystemProperty]
            private int entities;

            [SystemProperty]
            private int archetypes;

            [SystemProperty]
            private int chunks;
        }
    }
}
