// <copyright file="PhysicsToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DRAW || UNITY_PHYSICS
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.Debug.ViewModels;
    using Unity.Properties;
    using UnityEngine.UIElements;
    using Toggle = Unity.AppUI.UI.Toggle;

    public class PhysicsToolbarView : VisualElement, IView
    {
        public const string UssClassName = "bl-physics-tab";

        private readonly PhysicsToolbarViewModel viewModel;

        public PhysicsToolbarView()
        {
            this.AddToClassList(UssClassName);

            this.viewModel = new PhysicsToolbarViewModel();

            this.style.flexDirection = FlexDirection.Row;

            var left = new VisualElement();
            var right = new VisualElement();
            this.Add(left);
            this.Add(right);

            var colliders = new Toggle { label = "Colliders", dataSource = this.viewModel };
            colliders.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.ToSource,
                dataSource = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawColliderEdges)),
            });
            left.Add(colliders);

            var aabbs = new Toggle { label = "AABBs", dataSource = this.viewModel };
            aabbs.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.ToSource,
                dataSource = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawColliderAabbs)),
            });

            left.Add(aabbs);

#if BL_DRAW
            var terrain = new Toggle { label = "Terrain", dataSource = this.viewModel };

            terrain.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.ToSource,
                dataSource = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawTerrainColliderEdges)),
            });

            left.Add(terrain);

            var mesh = new Toggle { label = "Mesh", dataSource = this.viewModel };
            mesh.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.ToSource,
                dataSource = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawMeshColliderEdges)),
            });
            right.Add(mesh);
#endif

            var collisions = new Toggle { label = "Collisions", dataSource = this.viewModel };
            collisions.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.ToSource,
                dataSource = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawCollisionEvents)),
            });
            right.Add(collisions);

            var triggers = new Toggle { label = "Triggers", dataSource = this.viewModel };
            triggers.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.ToSource,
                dataSource = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawTriggerEvents)),
            });
            right.Add(triggers);
        }

        object IView.ViewModel => this.viewModel;
    }
}
#endif
