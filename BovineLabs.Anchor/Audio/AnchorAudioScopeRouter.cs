// <copyright file="AnchorAudioScopeRouter.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Audio
{
    using System;
    using System.Collections.Generic;
    using Unity.AppUI.UI;
    using UnityEngine;
    using UnityEngine.UIElements;
    using UIToolkitButton = UnityEngine.UIElements.Button;

    internal sealed class AnchorAudioScopeRouter : IDisposable
    {
        private readonly AnchorAudioFeedback feedback;
        private readonly Dictionary<int, WeakReference<VisualElement>> hoverTargets = new();
        private VisualElement scope;

        public AnchorAudioScopeRouter(AnchorAudioFeedback feedback)
        {
            this.feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        }

        internal VisualElement Scope => this.scope;

        internal int HoverPointerCount => this.hoverTargets.Count;

        public void RegisterScope(VisualElement newScope)
        {
            if (ReferenceEquals(this.scope, newScope))
            {
                return;
            }

            this.UnregisterScope();
            if (newScope == null)
            {
                return;
            }

            this.scope = newScope;
            this.scope.RegisterCallback<PointerOverEvent>(this.OnPointerOver, TrickleDown.TrickleDown);
            this.scope.RegisterCallback<PointerLeaveEvent>(this.OnPointerLeaveScope);
            this.scope.RegisterCallback<PointerCancelEvent>(this.OnPointerCancel, TrickleDown.TrickleDown);
            this.scope.RegisterCallback<PointerUpEvent>(this.OnPointerUp, TrickleDown.TrickleDown);
            this.scope.RegisterCallback<KeyUpEvent>(this.OnKeyUp, TrickleDown.TrickleDown);
            this.scope.RegisterCallback<ClickEvent>(this.OnClick, TrickleDown.TrickleDown);
        }

        public void UnregisterScope()
        {
            if (this.scope == null)
            {
                return;
            }

            this.scope.UnregisterCallback<PointerOverEvent>(this.OnPointerOver, TrickleDown.TrickleDown);
            this.scope.UnregisterCallback<PointerLeaveEvent>(this.OnPointerLeaveScope);
            this.scope.UnregisterCallback<PointerCancelEvent>(this.OnPointerCancel, TrickleDown.TrickleDown);
            this.scope.UnregisterCallback<PointerUpEvent>(this.OnPointerUp, TrickleDown.TrickleDown);
            this.scope.UnregisterCallback<KeyUpEvent>(this.OnKeyUp, TrickleDown.TrickleDown);
            this.scope.UnregisterCallback<ClickEvent>(this.OnClick, TrickleDown.TrickleDown);
            this.scope = null;
            this.hoverTargets.Clear();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.UnregisterScope();
        }

        internal VisualElement ResolveAudioTargetForTesting(VisualElement start)
        {
            return this.ResolveAudioTarget(start);
        }

        internal void HandlePointerOverForTesting(int pointerId, VisualElement start)
        {
            this.UpdateHoverTarget(pointerId, start);
        }

        internal void HandleClickForTesting(VisualElement start)
        {
            this.HandleClick(start);
        }

        private static bool HasExplicitAnchorAudio(VisualElement element)
        {
            return element is IAnchorAudioElement || AnchorAudio.TryGetOptions(element, out _);
        }

        private static bool IsHoverPointer(int pointerId)
        {
            if (pointerId == PointerId.mousePointerId)
            {
                return true;
            }

#if UNITY_6000_2_OR_NEWER
            return pointerId >= PointerId.trackedPointerIdBase;
#else
            return false;
#endif
        }

        private static bool IsSubmitKey(KeyCode keyCode)
        {
            return keyCode is KeyCode.Return or KeyCode.KeypadEnter or KeyCode.Space;
        }

        private static bool CanActivatePressablePointer(IPressable pressable, int pointerId, Vector2 panelPosition)
        {
            var pressableTarget = pressable.clickable?.target as VisualElement;
            if (pressableTarget == null || !pressable.clickable.active || !pressableTarget.enabledInHierarchy)
            {
                return false;
            }

            if (!pressableTarget.HasPointerCapture(pointerId))
            {
                return false;
            }

            return pressableTarget.ContainsPoint(pressableTarget.WorldToLocal(panelPosition));
        }

        private static bool CanActivatePressableKeyboard(IPressable pressable)
        {
            var pressableTarget = pressable.clickable?.target as VisualElement;
            return pressableTarget != null && pressable.clickable.active && pressableTarget.enabledInHierarchy;
        }

        private void OnPointerOver(PointerOverEvent evt)
        {
            this.UpdateHoverTarget(evt.pointerId, evt.target as VisualElement);
        }

        private void OnPointerLeaveScope(PointerLeaveEvent evt)
        {
            this.hoverTargets.Remove(evt.pointerId);
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            this.hoverTargets.Remove(evt.pointerId);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            var target = this.ResolveAudioTarget(evt.target as VisualElement);
            if (target is not IPressable pressable)
            {
                return;
            }

            if (CanActivatePressablePointer(pressable, evt.pointerId, evt.position))
            {
                this.feedback.Play(target, AnchorAudioCue.Activate);
            }
        }

        private void OnKeyUp(KeyUpEvent evt)
        {
            if (!IsSubmitKey(evt.keyCode))
            {
                return;
            }

            var target = this.ResolveAudioTarget(evt.target as VisualElement);
            if (target is not IPressable pressable)
            {
                return;
            }

            if (CanActivatePressableKeyboard(pressable))
            {
                this.feedback.Play(target, AnchorAudioCue.Activate);
            }
        }

        private void OnClick(ClickEvent evt)
        {
            this.HandleClick(evt.target as VisualElement);
        }

        private void HandleClick(VisualElement start)
        {
            var target = this.ResolveAudioTarget(start);
            if (target == null || target is IPressable)
            {
                return;
            }

            if ((target is UIToolkitButton || HasExplicitAnchorAudio(target)) && target.enabledInHierarchy)
            {
                this.feedback.Play(target, AnchorAudioCue.Activate);
            }
        }

        private void UpdateHoverTarget(int pointerId, VisualElement start)
        {
            if (!IsHoverPointer(pointerId))
            {
                return;
            }

            var target = this.ResolveAudioTarget(start);
            if (target == null || !target.enabledInHierarchy)
            {
                this.hoverTargets.Remove(pointerId);
                return;
            }

            if (this.IsCurrentHoverTarget(pointerId, target))
            {
                return;
            }

            this.hoverTargets[pointerId] = new WeakReference<VisualElement>(target);
            this.feedback.Play(target, AnchorAudioCue.Hover);
        }

        private bool IsCurrentHoverTarget(int pointerId, VisualElement target)
        {
            return this.hoverTargets.TryGetValue(pointerId, out var weakTarget) &&
                weakTarget.TryGetTarget(out var current) &&
                ReferenceEquals(current, target);
        }

        private VisualElement ResolveAudioTarget(VisualElement start)
        {
            VisualElement candidate = null;
            for (var current = start; current != null; current = current.parent)
            {
                if (ReferenceEquals(current, this.scope))
                {
                    return candidate;
                }

                if (candidate == null && (HasExplicitAnchorAudio(current) || current is IPressable || current is UIToolkitButton))
                {
                    candidate = current;
                }
            }

            return null;
        }
    }
}
