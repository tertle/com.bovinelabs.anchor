namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class UIParticleCoordinateTests
    {
        [TestCase(1f)]
        [TestCase(-1f)]
        public void PanelBirthBasisSurvivesSourceAndRendererTransformChanges(float reflection)
        {
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            try
            {
                var settings = effect.Emitters[0];
                settings.Speed = Vector2.zero;
                settings.Offset = new Vector2(3, 5);
                settings.Size = new Vector2(10, 10);
                settings.Rotation = new Vector2(25, 25);
                settings.Lifetime = new Vector2(10, 10);
                settings.SizeOverLifetime = AnimationCurve.Constant(0, 1, 1);
                using var runtime = new UIParticleRuntime(effect);
                var ancestor = float4x4.TRS(new float3(90, 110, 0), quaternion.RotateZ(0.3f), new float3(reflection * 2, 0.7f, 1));
                var sourceTransform = math.mul(ancestor, float4x4.TRS(new float3(11, 23, 0), quaternion.RotateZ(-0.8f), new float3(1.2f, 2, 1)));
                runtime.Play();
                runtime.Advance(0, ref sourceTransform);
                var birth = runtime.Particles[0];
                var movedSource = float4x4.Translate(new float3(500, 600, 0));
                runtime.Advance(0.01, ref movedSource);
                Assert.That(math.distance(runtime.Particles[0].Position, birth.Position), Is.LessThan(0.00001));
                runtime.PrepareMesh(new float4(1));
                var vertices = new NativeArray<Vertex>(4, Allocator.Temp);
                var indices = new NativeArray<ushort>(6, Allocator.Temp);
                var quads = runtime.Quads.Slice(0, 1);
                var output = vertices.Slice();
                var triangles = indices.Slice();
                ParticleQuadMesh.Fill(ref quads, ref output, ref triangles);
                var panelCorner = vertices[0].position;
                math.sincos(math.radians(25), out var sin, out var cos);
                var localCorner = new float2(3, 5) - (new float2(cos, sin) * 5) - (new float2(-sin, cos) * 5);
                var expected = math.transform(sourceTransform, new float3(localCorner, 0));
                Assert.That(math.distance(((float3)panelCorner).xy, expected.xy), Is.LessThan(0.0001));
                var renderer = float4x4.TRS(new float3(-40, 17, 0), quaternion.RotateZ(0.9f), new float3(-0.4f, 3, 1));
                var inverse = math.inverse(renderer);
                ParticleQuadMesh.Transform(ref output, ref inverse);
                ParticleQuadMesh.FixWinding(ref output, ref triangles);
                var roundTrip = math.transform(renderer, new float3(vertices[0].position.x, vertices[0].position.y, 0));
                Assert.That(math.distance(roundTrip.xy, expected.xy), Is.LessThan(0.0001));
                var a = vertices[indices[1]].position - vertices[indices[0]].position;
                var b = vertices[indices[2]].position - vertices[indices[0]].position;
                Assert.That((a.x * b.y) - (a.y * b.x), Is.GreaterThan(0));
            }
            finally
            {
                Object.DestroyImmediate(effect);
            }
        }
    }
}
