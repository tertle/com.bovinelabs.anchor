namespace BovineLabs.Anchor.Nav
{
    using System;
    using BovineLabs.Core.Asset;
    using BovineLabs.Core.PropertyDrawers;
    using UnityEngine;
    using UnityEngine.UIElements;

    [AutoRef("AnchorSettings", "animations", nameof(AnchorNavAnimation), "UI/Animations")]
    public abstract class AnchorNavAnimation : ScriptableObject, IUID
    {
        [InspectorReadOnly]
        [SerializeField]
        private int id;

        [Min(0)]
        [Tooltip("Animation time in milliseconds")]
        [SerializeField]
        private int duration;

        int IUID.ID
        {
            get => this.ID;
            set => this.id = value;
        }

        public int ID => this.id;

        protected abstract Func<float, float> EasingFunction { get; }

        protected abstract Action<VisualElement, float> Callback { get; }

        protected virtual int DefaultDuration => 150;

        public AnimationDescription GetDescription()
        {
            return new AnimationDescription
            {
                Easing = this.EasingFunction,
                DurationMs = this.duration,
                Callback = this.Callback,
            };
        }

        protected virtual void Reset()
        {
            this.duration = this.DefaultDuration;
        }
    }
}
