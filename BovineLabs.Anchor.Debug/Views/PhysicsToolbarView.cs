// <copyright file="PhysicsToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_PHYSICS
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
    public class PhysicsToolbarView : VisualElement
    {
        public const string UssClassName = "bl-physics-tab";

        [Preserve]
        public PhysicsToolbarView(PhysicsToolbarViewModel viewModel)
        {
            this.dataSource = viewModel;
            this.AddToClassList(UssClassName);

            this.style.flexDirection = FlexDirection.Row;

            var left = new VisualElement();
            var right = new VisualElement();
            this.Add(left);
            this.Add(right);

            var colliders = new Toggle
            {
                label = "Colliders",
            };

            colliders.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.TwoWay,
                dataSourcePath = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawColliderEdges)),
            });

            left.Add(colliders);

            var aabbs = new Toggle
            {
                label = "AABBs",
            };

            aabbs.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.TwoWay,
                dataSourcePath = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawColliderAabbs)),
            });

            left.Add(aabbs);

#if BL_QUILL
            var terrain = new Toggle
            {
                label = "Terrain",
            };

            terrain.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.TwoWay,
                dataSourcePath = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawTerrainColliderEdges)),
            });

            left.Add(terrain);

            var mesh = new Toggle
            {
                label = "Mesh",
            };

            mesh.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.TwoWay,
                dataSourcePath = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawMeshColliderEdges)),
            });

            right.Add(mesh);
#endif

            var collisions = new Toggle
            {
                label = "Collisions",
            };

            collisions.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.TwoWay,
                dataSourcePath = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawCollisionEvents)),
            });

            right.Add(collisions);

            var triggers = new Toggle
            {
                label = "Triggers",
            };

            triggers.SetBinding(nameof(Toggle.value), new DataBinding
            {
                bindingMode = BindingMode.TwoWay,
                dataSourcePath = new PropertyPath(nameof(PhysicsToolbarViewModel.DrawTriggerEvents)),
            });

            right.Add(triggers);
        }
    }
}
#endif
