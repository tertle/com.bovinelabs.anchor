namespace BovineLabs.Anchor.Particles.Sample
{
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.UIElements;

    [UxmlElement]
    public partial class ParticleFixtureElement : VisualElement
    {
        private NativeArray<ParticleQuad> quads;
        private int count = 4;
        private ParticleFixtureClock clock;

        public ParticleFixtureElement()
        {
            this.pickingMode = PickingMode.Ignore;
            this.focusable = false;
            this.generateVisualContent += this.Draw;
            this.RegisterCallback<AttachToPanelEvent>(this.Attach);
            this.RegisterCallback<DetachFromPanelEvent>(this.Detach);
            this.RegisterCallback<GeometryChangedEvent>(_ => this.Populate());
        }

        [UxmlAttribute]
        public int Count
        {
            get => this.count;
            set
            {
                this.count = value;
                if (this.panel != null)
                {
                    this.Populate();
                    this.MarkDirtyRepaint();
                }
            }
        }

        [UxmlAttribute] public float QuadSize { get; set; } = 32;
        [UxmlAttribute] public bool Overlap { get; set; }
        [UxmlAttribute] public bool ZeroAlpha { get; set; }
        public bool Opaque { get; set; }
        public Texture Texture { get; set; }
        public bool Paused { get; set; }
        public long TickCount { get; private set; }
        public long DrawCount { get; private set; }
        public long NativeBytes => this.quads.IsCreated ? this.quads.Length * (long)Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<ParticleQuad>() : 0;

        internal void Tick()
        {
            if (this.Paused || this.count == 0 || !this.visible || this.resolvedStyle.display == DisplayStyle.None ||
                this.contentRect.width <= 0 || this.contentRect.height <= 0)
            {
                return;
            }

            for (var ancestor = this.parent; ancestor != null; ancestor = ancestor.parent)
            {
                if (ancestor.resolvedStyle.display == DisplayStyle.None || ancestor.resolvedStyle.visibility == Visibility.Hidden)
                {
                    return;
                }
            }

            this.TickCount++;
            this.MarkDirtyRepaint();
        }

        private void Attach(AttachToPanelEvent evt)
        {
            this.Populate();
            this.clock = ParticleFixtureClock.Register(evt.destinationPanel, this);
        }

        private void Detach(DetachFromPanelEvent evt)
        {
            this.clock.Unregister(this);
            this.clock = null;
            if (this.quads.IsCreated)
            {
                this.quads.Dispose();
                this.quads = default;
            }
        }

        private void Populate()
        {
            if (this.panel == null)
            {
                return;
            }

            if (!this.quads.IsCreated || this.quads.Length < this.count)
            {
                if (this.quads.IsCreated)
                {
                    this.quads.Dispose();
                }

                this.quads = new NativeArray<ParticleQuad>(this.count, Allocator.Persistent);
            }

            var width = math.max(1, this.contentRect.width);
            var height = math.max(1, this.contentRect.height);
            for (var q = 0; q < this.count; q++)
            {
                this.quads[q] = new ParticleQuad
                {
                    Position = this.Overlap ? new float2(width / 2, height / 2) : new float2(16 + ((q * 37) % width), 16 + ((q * 23) % height)),
                    Size = new float2(this.QuadSize, this.QuadSize * 0.75f),
                    Angle = q == 0 ? 0 : q * 0.31f,
                    Tint = new Color32(255, 190, 100, this.ZeroAlpha ? (byte)0 : this.Opaque ? (byte)255 : (byte)160),
                    Uv = new float4(0, 0, 1, 1),
                };
            }
        }

        private void Draw(MeshGenerationContext context)
        {
            this.DrawCount++;
            if (this.count == 0 || this.contentRect.width <= 0 || this.contentRect.height <= 0)
            {
                return;
            }

            ParticleQuadMesh.Draw(context, this.quads.Slice(0, this.count), this.Texture);
        }
    }
}
