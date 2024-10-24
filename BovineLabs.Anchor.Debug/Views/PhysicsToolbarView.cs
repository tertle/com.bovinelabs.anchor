// <copyright file="PhysicsToolbarView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DRAW || UNITY_PHYSICS
namespace BovineLabs.Anchor.Debug.Views
{
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.Debug.ViewModels;
    using JetBrains.Annotations;
    using Unity.Properties;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;
    using Toggle = Unity.AppUI.UI.Toggle;

    [Transient]
    [UsedImplicitly]
    public class PhysicsToolbarView : VisualElement, IView<PhysicsToolbarViewModel>
    {
        public const string UssClassName = "bl-physics-tab";

        public PhysicsToolbarView()
        {
            this.AddToClassList(UssClassName);

            this.style.flexDirection = FlexDirection.Row;

            var left = new VisualElement();
            var right = new VisualElement();
            this.Add(left);
            this.Add(right);

            var colliders = new Toggle { label = "Colliders", dataSource = this.ViewModel };
            colliders.RegisterValueChangedCallback(evt => this.ViewModel.DrawColliderEdges = evt.newValue);
            left.Add(colliders);

            var aabbs = new Toggle { label = "AABBs", dataSource = this.ViewModel };
            aabbs.RegisterValueChangedCallback(evt => this.ViewModel.DrawColliderAabbs = evt.newValue);
            left.Add(aabbs);

#if BL_DRAW
            var terrain = new Toggle { label = "Terrain", dataSource = this.ViewModel };
            terrain.RegisterValueChangedCallback(evt => this.ViewModel.DrawTerrainColliderEdges = evt.newValue);
            left.Add(terrain);

            var mesh = new Toggle { label = "Mesh", dataSource = this.ViewModel };
            mesh.RegisterValueChangedCallback(evt => this.ViewModel.DrawMeshColliderEdges = evt.newValue);
            right.Add(mesh);
#endif

            var collisions = new Toggle { label = "Collisions", dataSource = this.ViewModel };
            collisions.RegisterValueChangedCallback(evt => this.ViewModel.DrawCollisionEvents = evt.newValue);
            right.Add(collisions);

            var triggers = new Toggle { label = "Triggers", dataSource = this.ViewModel };
            triggers.RegisterValueChangedCallback(evt => this.ViewModel.DrawTriggerEvents = evt.newValue);
            right.Add(triggers);
        }

        public PhysicsToolbarViewModel ViewModel { get; } = new();
    }
}
#endif
