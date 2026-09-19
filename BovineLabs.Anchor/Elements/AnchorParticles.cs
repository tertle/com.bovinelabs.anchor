namespace BovineLabs.Anchor.Elements
{
    using BovineLabs.Anchor.Particles;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class AnchorParticles : VisualElement
    {
        private UIParticleEffect effect;
        private UIParticleRuntime runtime;
        private UIParticleCoordinator coordinator;
        private UIParticleSpace space;
        private Color tint = Color.white;
        private bool playOnAttach;
        private uint seed = 1;

        public AnchorParticles()
        {
            this.pickingMode = PickingMode.Ignore;
            this.focusable = false;
            this.generateVisualContent += this.Draw;
            this.RegisterCallback<AttachToPanelEvent>(this.Attach);
            this.RegisterCallback<DetachFromPanelEvent>(this.Detach);
        }

        [UxmlAttribute]
        public UIParticleEffect Effect
        {
            get => this.effect;
            set
            {
                if (this.effect == value)
                {
                    return;
                }

                this.Release();
                this.effect = value;
                this.MarkDirtyRepaint();
            }
        }

        [UxmlAttribute]
        public UIParticleSpace Space
        {
            get => this.space;
            set
            {
                if (this.space == value)
                {
                    return;
                }

                this.Clear();
                this.space = value;
            }
        }

        [UxmlAttribute]
        public Color Tint
        {
            get => this.tint;
            set
            {
                this.tint = value;
                this.MarkDirtyRepaint();
            }
        }

        public bool IsPlaying => this.runtime?.IsPlaying ?? false;
        public bool IsPaused => this.runtime?.IsPaused ?? false;
        public int LiveCount => this.runtime?.LiveCount ?? 0;

        public void Play(uint seed = 1)
        {
            this.seed = seed;
            this.playOnAttach = true;
            if (this.panel == null || this.effect == null)
            {
                return;
            }

            if (this.runtime == null || this.runtime.Revision != this.effect.Revision)
            {
                this.runtime?.Dispose();
                this.runtime = null;
                this.MarkDirtyRepaint();
                this.runtime = new UIParticleRuntime(this.effect);
            }

            this.runtime.Play(seed);
            this.Advance(0);
            this.MarkDirtyRepaint();
        }

        public void StopEmitting()
        {
            this.playOnAttach = false;
            this.runtime?.StopEmitting();
        }

        public void Pause() => this.runtime?.Pause();
        public void Resume() => this.runtime?.Resume();

        public new void Clear()
        {
            this.playOnAttach = false;
            this.runtime?.Clear();
            this.MarkDirtyRepaint();
        }

        // Manual preview shares the production clock path. Do not call alongside automatic Player updates.
        public void Advance(double elapsed)
        {
            if (this.runtime == null || !this.runtime.IsPlaying || this.runtime.IsPaused)
            {
                return;
            }

            var transform = this.space == UIParticleSpace.Panel ? (float4x4)this.worldTransform : float4x4.identity;
            var hadParticles = this.runtime.LiveCount != 0;
            if (this.runtime.Advance(elapsed, ref transform) && (hadParticles || this.runtime.LiveCount != 0))
            {
                this.MarkDirtyRepaint();
            }
        }

        private void Attach(AttachToPanelEvent evt)
        {
            this.coordinator = UIParticleCoordinator.Register(evt.destinationPanel, this);
            if (this.playOnAttach)
            {
                this.Play(this.seed);
            }
        }

        private void Detach(DetachFromPanelEvent evt)
        {
            this.coordinator?.Unregister(this);
            this.coordinator = null;
            this.Release();
        }

        private void Release()
        {
            this.runtime?.Dispose();
            this.runtime = null;
            this.playOnAttach = false;
        }

        private void Draw(MeshGenerationContext context)
        {
            if (this.runtime == null)
            {
                return;
            }

            this.runtime.PrepareMesh(new float4(this.tint.r, this.tint.g, this.tint.b, this.tint.a));
            var transform = this.space == UIParticleSpace.Panel ? (float4x4)this.worldTransform.inverse : float4x4.identity;
            this.runtime.Draw(context, ref transform, this.space == UIParticleSpace.Panel);
        }
    }
}
