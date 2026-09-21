namespace BovineLabs.Anchor.Nav
{
    using System;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    internal sealed class AnchorNavAnimationHandle
    {
        private readonly AnimationDescription _description;
        private readonly Action _onCompleted;

        private bool _completed;

        public AnchorNavAnimationHandle(VisualElement element, AnimationDescription description, Action onCompleted)
        {
            Element = element;
            _description = description;
            _onCompleted = onCompleted;
        }

        public VisualElement Element { get; }

        public ValueAnimation<float> Handle { get; set; }

        public bool TryFinalizeFromAnimation()
        {
            if (_completed)
            {
                return false;
            }

            _completed = true;
            Handle = null;
            return true;
        }

        public void CompleteImmediately()
        {
            if (_completed)
            {
                return;
            }

            _completed = true;

            Handle?.Recycle();
            Handle = null;
            _description.Callback?.Invoke(Element, 1f);
            _onCompleted?.Invoke();
        }
    }
}
