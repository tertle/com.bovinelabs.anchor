namespace BovineLabs.Anchor.Nav
{
    using System;
    using Unity.Scripting.LifecycleManagement;
    using UnityEngine.UIElements;

    public struct AnimationDescription
    {
        [NoAutoStaticsCleanup]
        public static readonly AnimationDescription None = new()
        {
            Easing = UnityEngine.UIElements.Experimental.Easing.Linear,
            DurationMs = 0,
            Callback = null,
        };

        /// <summary>
        /// Duration in milliseconds.
        /// </summary>
        public int DurationMs;

        public Func<float, float> Easing;

        public Action<VisualElement, float> Callback;
    }
}
