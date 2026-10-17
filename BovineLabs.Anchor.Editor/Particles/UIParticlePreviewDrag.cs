namespace BovineLabs.Anchor.Editor.Particles
{
    using UnityEngine;
    using UnityEngine.UIElements;

    internal sealed class UIParticlePreviewDrag : PointerManipulator
    {
        private int _pointer = -1;
        private Vector2 _offset;

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(Down);
            target.RegisterCallback<PointerMoveEvent>(Move);
            target.RegisterCallback<PointerUpEvent>(Up);
            target.RegisterCallback<PointerCaptureOutEvent>(CaptureOut);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(Down);
            target.UnregisterCallback<PointerMoveEvent>(Move);
            target.UnregisterCallback<PointerUpEvent>(Up);
            target.UnregisterCallback<PointerCaptureOutEvent>(CaptureOut);
        }

        private void Down(PointerDownEvent evt)
        {
            if (evt.button != 0)
            {
                return;
            }

            _pointer = evt.pointerId;
            _offset = target.parent.WorldToLocal(evt.position) - target.layout.position;
            target.CapturePointer(_pointer);
            evt.StopPropagation();
        }

        private void Move(PointerMoveEvent evt)
        {
            if (_pointer != evt.pointerId || !target.HasPointerCapture(_pointer))
            {
                return;
            }

            var point = target.parent.WorldToLocal(evt.position) - _offset;
            target.style.left = Mathf.Clamp(point.x, 0, Mathf.Max(0, target.parent.contentRect.width - target.layout.width));
            target.style.top = Mathf.Clamp(point.y, 0, Mathf.Max(0, target.parent.contentRect.height - target.layout.height));
            evt.StopPropagation();
        }

        private void Up(PointerUpEvent evt)
        {
            if (_pointer == evt.pointerId)
            {
                target.ReleasePointer(_pointer);
                _pointer = -1;
            }
        }

        private void CaptureOut(PointerCaptureOutEvent evt) => _pointer = -1;
    }
}
