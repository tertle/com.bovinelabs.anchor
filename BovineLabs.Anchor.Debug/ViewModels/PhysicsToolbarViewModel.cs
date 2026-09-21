#if UNITY_PHYSICS
namespace BovineLabs.Anchor.Debug.ViewModels
{
    using System;
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [Serializable]
    public partial class PhysicsToolbarViewModel : SystemObservableObject<PhysicsToolbarViewModel.Data>, IToolbarElement
    {
        [CreateProperty]
        public bool DrawColliderEdges
        {
            get => Value.DrawColliderEdges;
            set => Value.DrawColliderEdges = value;
        }

        [CreateProperty]
        public bool DrawColliderAabbs
        {
            get => Value.DrawColliderAabbs;
            set => Value.DrawColliderAabbs = value;
        }

        [CreateProperty]
        public bool DrawCollisionEvents
        {
            get => Value.DrawCollisionEvents;
            set => Value.DrawCollisionEvents = value;
        }

        [CreateProperty]
        public bool DrawTriggerEvents
        {
            get => Value.DrawTriggerEvents;
            set => Value.DrawTriggerEvents = value;
        }

        [CreateProperty]
        public bool DrawMeshColliderEdges
        {
            get => Value.DrawMeshColliderEdges;
            set => Value.DrawMeshColliderEdges = value;
        }

        [CreateProperty]
        public bool DrawTerrainColliderEdges
        {
            get => Value.DrawTerrainColliderEdges;
            set => Value.DrawTerrainColliderEdges = value;
        }

        public VisualElement CreateElement()
        {
            return new PhysicsToolbarView(this);
        }

        [Serializable]
        public partial struct Data
        {
            [SerializeField]
            [SystemProperty]
            private bool _drawColliderEdges;

            [SerializeField]
            [SystemProperty]
            private bool _drawColliderAabbs;

            [SerializeField]
            [SystemProperty]
            private bool _drawCollisionEvents;

            [SerializeField]
            [SystemProperty]
            private bool _drawTriggerEvents;

            [SerializeField]
            [SystemProperty]
            private bool _drawMeshColliderEdges;

            [SerializeField]
            [SystemProperty]
            private bool _drawTerrainColliderEdges;
        }
    }
}
#endif
