namespace BovineLabs.Anchor.Nav
{
    using System;
    using BovineLabs.Core.Asset;
    using BovineLabs.Core.PropertyDrawers;
    using UnityEngine;
    using UnityEngine.UIElements;

    [AutoRef("AnchorSettings", "_animations", nameof(AnchorNavAnimation), "UI/Animations")]
    public abstract class AnchorNavAnimation : ScriptableObject, IUID
    {
        [InspectorReadOnly]
        [SerializeField]
        private int _id;

        [Min(0)]
        [Tooltip("Animation time in milliseconds")]
        [SerializeField]
        private int _duration;

        int IUID.ID
        {
            get => ID;
            set => _id = value;
        }

        public int ID => _id;

        protected abstract Func<float, float> EasingFunction { get; }

        protected abstract Action<VisualElement, float> Callback { get; }

        protected virtual int DefaultDuration => 150;

        public AnimationDescription GetDescription()
        {
            return new AnimationDescription
            {
                Easing = EasingFunction,
                DurationMs = _duration,
                Callback = Callback,
            };
        }

        protected virtual void Reset()
        {
            _duration = DefaultDuration;
        }
    }
}
