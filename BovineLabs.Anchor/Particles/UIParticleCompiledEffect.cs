namespace BovineLabs.Anchor.Particles
{
    using System;
    using System.Collections.Generic;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Mathematics;
    using UnityEngine;

    internal sealed class UIParticleCompiledEffect
    {
        public const int SampleCount = 128;
        private int owners;

        public UIParticleCompiledEffect(UIParticleEffect effect)
        {
            var emitters = new List<UIParticleEmitter>();
            var bursts = new List<UIParticleBurst>();
            var samples = new List<UIParticleSample>();
            var steps = new List<UIParticleColorStep>();
            this.Textures = new Texture2D[effect.Emitters.Count];
            this.Revision = effect.Revision;

            foreach (var settings in effect.Emitters)
            {
                Validate(settings);
                var capacity = checked(this.Capacity + settings.MaxParticles);
                var emitter = new UIParticleEmitter
                {
                    Offset = this.Capacity,
                    Capacity = settings.MaxParticles,
                    BurstOffset = bursts.Count,
                    BurstCount = settings.Bursts.Count,
                    StepOffset = steps.Count,
                    Duration = settings.Duration,
                    Delay = settings.StartDelay,
                    Rate = settings.Rate,
                    Looping = settings.Looping,
                    Shape = settings.Shape,
                    Position = settings.Offset,
                    Dimensions = settings.Dimensions,
                    Direction = math.radians(settings.Direction),
                    Spread = math.radians(settings.Spread),
                    Lifetime = settings.Lifetime,
                    Speed = settings.Speed,
                    Size = settings.Size,
                    Rotation = math.radians((float2)settings.Rotation),
                    AngularVelocity = math.radians((float2)settings.AngularVelocity),
                    ColorMin = ToFloat(settings.StartColorMin),
                    ColorMax = ToFloat(settings.StartColorMax),
                    Acceleration = settings.Acceleration,
                    Drag = settings.Drag,
                    Uv = new float4(settings.Uv.xMin, settings.Uv.yMin, settings.Uv.xMax, settings.Uv.yMax),
                };

                // Stable ordering for equal-time bursts, without mutating authoring data.
                var sorted = new List<UIParticleBurst>(settings.Bursts);
                for (var i = 1; i < sorted.Count; i++)
                {
                    var value = sorted[i];
                    var j = i;
                    while (j > 0 && sorted[j - 1].Time > value.Time)
                    {
                        sorted[j] = sorted[j - 1];
                        j--;
                    }

                    sorted[j] = value;
                }

                bursts.AddRange(sorted);
                for (var i = 0; i < SampleCount; i++)
                {
                    var t = i / (float)(SampleCount - 1);
                    var size = settings.SizeOverLifetime.Evaluate(t);
                    Require(math.isfinite(size) && size >= 0, "Size curve must produce finite nonnegative samples.");
                    var color = ToFloat(settings.ColorOverLifetime.Evaluate(t));
                    Require(math.all(math.isfinite(color)), "Gradient must produce finite samples.");
                    samples.Add(new UIParticleSample
                    {
                        Size = size,
                        Color = color,
                    });
                }

                if (settings.ColorOverLifetime.mode == GradientMode.Fixed)
                {
                    var boundaries = new SortedSet<float> { 0, 1 };
                    foreach (var key in settings.ColorOverLifetime.colorKeys)
                    {
                        boundaries.Add(key.time);
                    }

                    foreach (var key in settings.ColorOverLifetime.alphaKeys)
                    {
                        boundaries.Add(key.time);
                    }

                    var times = new List<float>(boundaries);
                    for (var i = 0; i < times.Count; i++)
                    {
                        var t = times[i];
                        var after = i + 1 < times.Count ? (t + times[i + 1]) * 0.5f : t;
                        steps.Add(new UIParticleColorStep
                        {
                            Time = t,
                            At = ToFloat(settings.ColorOverLifetime.Evaluate(t)),
                            After = ToFloat(settings.ColorOverLifetime.Evaluate(after)),
                        });
                    }

                    emitter.StepCount = steps.Count - emitter.StepOffset;
                }

                this.Textures[emitters.Count] = settings.Texture;
                emitters.Add(emitter);
                this.Capacity = capacity;
            }

            // All authoring validation and checked capacity arithmetic precede native allocation.
            this.Data = new UIParticleCompiledData
            {
                Emitters = new NativeArray<UIParticleEmitter>(emitters.ToArray(), Allocator.Persistent),
                Bursts = new NativeArray<UIParticleBurst>(bursts.ToArray(), Allocator.Persistent),
                Samples = new NativeArray<UIParticleSample>(samples.ToArray(), Allocator.Persistent),
                Steps = new NativeArray<UIParticleColorStep>(steps.ToArray(), Allocator.Persistent),
            };
            this.NativeBytes = (long)emitters.Count * UnsafeUtility.SizeOf<UIParticleEmitter>() +
                ((long)bursts.Count * UnsafeUtility.SizeOf<UIParticleBurst>()) +
                ((long)samples.Count * UnsafeUtility.SizeOf<UIParticleSample>()) + ((long)steps.Count * UnsafeUtility.SizeOf<UIParticleColorStep>());
        }

        public UIParticleCompiledData Data;
        public Texture2D[] Textures { get; }
        public uint Revision { get; }
        public int Capacity { get; }
        public long NativeBytes { get; }
        public bool IsDisposed { get; private set; }

        public void Retain() => this.owners++;

        public void Release()
        {
            if (--this.owners != 0)
            {
                return;
            }

            this.Data.Emitters.Dispose();
            this.Data.Bursts.Dispose();
            this.Data.Samples.Dispose();
            this.Data.Steps.Dispose();
            this.IsDisposed = true;
        }

        private static void Validate(UIParticleEmitterSettings s)
        {
            Require(s != null, "Emitter settings are required.");
            Require(math.isfinite(s.Duration) && s.Duration > 0, "Duration must be finite and positive.");
            Require(math.isfinite(s.StartDelay) && s.StartDelay >= 0, "Delay must be finite and nonnegative.");
            Require(math.isfinite(s.Rate) && s.Rate >= 0, "Rate must be finite and nonnegative.");
            Require(s.MaxParticles > 0, "Capacity must be positive.");
            Require(s.Shape >= UIParticleShape.Point && s.Shape <= UIParticleShape.Circle, "Unknown shape.");
            Require(math.all(math.isfinite((float2)s.Offset)), "Offset must be finite.");
            Require(math.all(math.isfinite((float2)s.Dimensions)) && math.all((float2)s.Dimensions >= 0), "Dimensions must be nonnegative.");
            Require(math.isfinite(s.Direction) && math.isfinite(s.Spread) && s.Spread >= 0 && s.Spread <= 360, "Invalid direction or spread.");
            Range(s.Lifetime, true, true);
            Range(s.Speed, true, false);
            Range(s.Size, true, false);
            Range(s.Rotation, false, false);
            Range(s.AngularVelocity, false, false);
            Require(math.all(math.isfinite((float2)s.Acceleration)), "Acceleration must be finite.");
            Require(math.isfinite(s.Drag) && s.Drag >= 0, "Drag must be finite and nonnegative.");
            var min = ToFloat(s.StartColorMin);
            var max = ToFloat(s.StartColorMax);
            Require(math.all(math.isfinite(min)) && math.all(math.isfinite(max)) && math.all(min >= 0) && math.all(max >= min) &&
                math.all(max <= 1), "Start colors must have ordered channels in [0, 1].");
            var uv = new float4(s.Uv.x, s.Uv.y, s.Uv.width, s.Uv.height);
            Require(math.all(math.isfinite(uv)) && math.all(uv.xy >= 0) && math.all(uv.zw > 0) && math.all(uv.xy + uv.zw <= 1),
                "UV rectangle must be nonempty and contained in [0, 1].");
            Require(s.Bursts != null, "Burst list is required.");
            foreach (var burst in s.Bursts)
            {
                Require(math.isfinite(burst.Time) && burst.Time >= 0 && burst.Time < s.Duration && burst.Count >= 0, "Invalid burst.");
            }

            Require(s.SizeOverLifetime != null && s.SizeOverLifetime.length > 0, "Size curve is required.");
            foreach (var key in s.SizeOverLifetime.keys)
            {
                Require(math.isfinite(key.time) && math.isfinite(key.value) && key.value >= 0, "Invalid size curve key.");
            }

            Require(s.ColorOverLifetime != null, "Color gradient is required.");
            foreach (var key in s.ColorOverLifetime.colorKeys)
            {
                Require(math.isfinite(key.time) && key.time >= 0 && key.time <= 1 && math.all(math.isfinite(ToFloat(key.color))),
                    "Invalid gradient color key.");
            }

            foreach (var key in s.ColorOverLifetime.alphaKeys)
            {
                Require(math.isfinite(key.time) && key.time >= 0 && key.time <= 1 && math.isfinite(key.alpha), "Invalid gradient alpha key.");
            }
        }

        private static void Range(Vector2 range, bool nonnegative, bool positive)
        {
            Require(math.all(math.isfinite((float2)range)) && range.y >= range.x && (!nonnegative || range.x >= 0) && (!positive || range.x > 0),
                "Initial-value ranges must be finite, ordered and respect their minimum.");
        }

        private static float4 ToFloat(Color color) => new(color.r, color.g, color.b, color.a);

        private static void Require(bool valid, string message)
        {
            if (!valid)
            {
                throw new ArgumentException(message);
            }
        }
    }
}
