// <copyright file="AnchorNavAnimationHandle.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Nav
{
    using System;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;
    using UnityEngine.UIElements.Experimental;

    internal sealed class AnchorNavAnimationHandle
    {
        private readonly AnimationDescription description;
        private readonly Action onCompleted;

        private bool completed;

        public AnchorNavAnimationHandle(
            VisualElement element,
            AnimationDescription description,
            Action onCompleted)
        {
            this.Element = element;
            this.description = description;
            this.onCompleted = onCompleted;
        }

        public VisualElement Element { get; }

        public ValueAnimation<float> Handle { get; set; }

        public bool TryFinalizeFromAnimation()
        {
            if (this.completed)
            {
                return false;
            }

            this.completed = true;
            this.Handle = null;
            return true;
        }

        public void CompleteImmediately()
        {
            if (this.completed)
            {
                return;
            }

            this.completed = true;

            this.Handle?.Recycle();
            this.Handle = null;
            this.description.callback?.Invoke(this.Element, 1f);
            this.onCompleted?.Invoke();
        }
    }
}
