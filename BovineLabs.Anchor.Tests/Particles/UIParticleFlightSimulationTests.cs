namespace BovineLabs.Anchor.Tests.Particles
{
    using BovineLabs.Anchor.Particles;
    using NUnit.Framework;
    using Unity.Collections;
    using Unity.Mathematics;

    public class UIParticleFlightSimulationTests
    {
        private UIParticleCompiledData _data;
        private NativeArray<UIParticle> _particles;
        private NativeArray<UIParticleEmitterState> _states;
        private NativeArray<UIParticleEmissionStream> _streams;
        private NativeArray<int2> _flipbooks;
        private UIParticleFlights _flights;
        private float4x4 _transform;

        [SetUp]
        public void SetUp()
        {
            _data = new UIParticleCompiledData
            {
                Emitters = new NativeArray<UIParticleEmitter>(2, Allocator.Persistent),
                Bursts = new NativeArray<UIParticleBurst>(1, Allocator.Persistent),
                Flights = new NativeArray<UIParticleFlight>(2, Allocator.Persistent),
                Flipbooks = new NativeArray<UIParticleFlipbook>(2, Allocator.Persistent),
            };
            _data.Emitters[0] = new UIParticleEmitter
            {
                Capacity = 1,
                Duration = 1,
                Lifetime = new float2(2),
            };
            _data.Emitters[1] = new UIParticleEmitter
            {
                Offset = 1,
                Capacity = 3,
                Duration = 1,
                Lifetime = new float2(0.01f),
                Speed = new float2(8),
                Size = new float2(4),
                ColorMin = new float4(1),
                ColorMax = new float4(1),
            };
            _data.Flights[1] = new UIParticleFlight
            {
                Enabled = true,
                ScatterDuration = new float2(0.25f),
                FlightDuration = new float2(0.5f),
            };
            _particles = new NativeArray<UIParticle>(4, Allocator.Persistent);
            _states = new NativeArray<UIParticleEmitterState>(2, Allocator.Persistent);
            _streams = new NativeArray<UIParticleEmissionStream>(3, Allocator.Persistent);
            _flipbooks = new NativeArray<int2>(3, Allocator.Persistent);
            _flights = new UIParticleFlights
            {
                States = new NativeArray<UIParticleFlightState>(3, Allocator.Persistent),
                Target = new float2(112, 20),
            };
            _transform = float4x4.Translate(new float3(10, 20, 0));
        }

        [TearDown]
        public void TearDown()
        {
            _data.Emitters.Dispose();
            _data.Bursts.Dispose();
            _data.Flights.Dispose();
            _data.Flipbooks.Dispose();
            _particles.Dispose();
            _states.Dispose();
            _streams.Dispose();
            _flipbooks.Dispose();
            _flights.States.Dispose();
        }

        [Test]
        public void ScatterEndsAtItsExactBoundaryAndFlightOwnsFiniteLifetime()
        {
            Assert.That(Emit(1).Emitted, Is.EqualTo(1));
            Assert.That(_particles[1].Lifetime, Is.EqualTo(0.75f));
            Step(0.125);
            Assert.That(_particles[1].Position, Is.EqualTo(new float2(11, 20)));
            Assert.That(_flights.States[0].Started, Is.False);
            Step(0.125);
            Assert.That(_flights.States[0].Start, Is.EqualTo(new float2(12, 20)));
            Step(0.25);
            Assert.That(_particles[1].Position.x, Is.EqualTo(62).Within(0.0001));
            Step(0.25);
            Assert.That(_states[1].Count, Is.Zero);
            Assert.That(_flights.Arrivals, Is.EqualTo(1));
            Step(1);
            Assert.That(_flights.Arrivals, Is.EqualTo(1));
        }

        [Test]
        public void OvershootingBothPhasesRetiresOnceAndDoesNotReplayManualEmission()
        {
            Emit(2);
            Step(2);
            Assert.That(_states[1].Count, Is.Zero);
            Assert.That(_flights.Arrivals, Is.EqualTo(2));
            Step(2);
            Assert.That(_states[1].Emitted, Is.EqualTo(2));
            Assert.That(_flights.Arrivals, Is.EqualTo(2));
        }

        [Test]
        public void ArrivalRadiusDoesNotRetireScatterAndSnapshotCapturesAtFlightStart()
        {
            var flight = _data.Flights[1];
            flight.ArrivalRadius = 20;
            _data.Flights[1] = flight;
            _flights.Target = new float2(10, 20);
            Emit(1);
            Step(0.125);
            Assert.That(_states[1].Count, Is.EqualTo(1));
            _flights.Target = new float2(112, 20);
            Step(0.125);
            Assert.That(_flights.States[0].Destination, Is.EqualTo(new float2(112, 20)));
            _flights.Target = new float2(-100, 20);
            Step(0.125);
            Assert.That(_particles[1].Position.x, Is.EqualTo(37).Within(0.0001));
            Step(0.3125);
            Assert.That(_states[1].Count, Is.Zero);
            Assert.That(_flights.Arrivals, Is.EqualTo(1));
        }

        [Test]
        public void FollowUsesLatestEndpointAndCannotChaseBeyondItsDuration()
        {
            var flight = _data.Flights[1];
            flight.TargetMode = UIParticleTargetMode.Follow;
            _data.Flights[1] = flight;
            Emit(1);
            Step(0.25);
            _flights.Target = new float2(152, 60);
            Step(0.25);
            Assert.That(_particles[1].Position.x, Is.EqualTo(82).Within(0.0001));
            Assert.That(_particles[1].Position.y, Is.EqualTo(40).Within(0.0001));
            _flights.Target = new float2(10000, 10000);
            Step(0.25);
            Assert.That(_states[1].Count, Is.Zero);
            Assert.That(_flights.Arrivals, Is.EqualTo(1));
        }

        [Test]
        public void StableCompactionKeepsFlightAndFlipbookStateWithTheirBirths()
        {
            var flight = _data.Flights[1];
            flight.ScatterDuration = float2.zero;
            flight.FlightDuration = new float2(3);
            _data.Flights[1] = flight;
            _data.Flipbooks[1] = new UIParticleFlipbook
            {
                Enabled = true,
                StartFrame = new int2(0, 7),
                Rows = 1,
            };
            UIParticleSimulation.Emit(ref _data, ref _particles, ref _states, ref _flipbooks, ref _flights, 0, 1, 7, 1, ref _transform, out _);
            for (var i = 0; i < 3; i++)
            {
                _transform.c3.x = (i + 1) * 10;
                Emit(1);
                var phase = _flights.States[i];
                phase.FlightDuration = i + 1;
                _flights.States[i] = phase;
                var particle = _particles[i + 1];
                particle.Lifetime = i + 1;
                _particles[i + 1] = particle;
                _flipbooks[i] = new int2(i + 2, 0);
            }

            Step(1.5);
            Assert.That(_states[0].Count, Is.EqualTo(1));
            Assert.That(_states[1].Count, Is.EqualTo(2));
            Assert.That(_flights.States[0].Start.x, Is.EqualTo(20));
            Assert.That(_flights.States[1].Start.x, Is.EqualTo(30));
            Assert.That(_flights.States[0].FlightDuration, Is.EqualTo(2));
            Assert.That(_flights.States[1].FlightDuration, Is.EqualTo(3));
            Assert.That(_flipbooks[0].x, Is.EqualTo(3));
            Assert.That(_flipbooks[1].x, Is.EqualTo(4));
            Assert.That(_flights.Arrivals, Is.EqualTo(1));
        }

        [Test]
        public void JoinedSourcesKeepCapturedCoordinatesAndBoundCapacityWithoutDebt()
        {
            Emit(1);
            _transform.c3.x = 50;
            var result = Emit(5);
            Assert.That(result.Attempted, Is.EqualTo(5));
            Assert.That(result.Emitted, Is.EqualTo(2));
            Assert.That(result.Dropped, Is.EqualTo(3));
            Assert.That(_particles[1].Position.x, Is.EqualTo(10));
            Assert.That(_particles[2].Position.x, Is.EqualTo(50));
            Assert.That(_particles[3].Position.x, Is.EqualTo(50));
            Assert.That(_states[1].Attempted, Is.EqualTo(6));
            Assert.That(_states[1].Ordinal, Is.EqualTo(6));
            Step(1);
            Step(1);
            Assert.That(_states[1].Count, Is.Zero);
            Assert.That(_flights.Arrivals, Is.EqualTo(3));
        }

        [Test]
        public void ManualQualityCreditIsFractionalAndExtremeRequestsStayBounded()
        {
            Assert.That(Emit(1, 0.5f).Attempted, Is.Zero);
            Assert.That(Emit(1, 0.5f).Emitted, Is.EqualTo(1));
            var result = Emit(int.MaxValue, float.MaxValue);
            Assert.That(result.Emitted, Is.EqualTo(2));
            Assert.That(result.Attempted, Is.EqualTo(ulong.MaxValue));
            Assert.That(result.Dropped, Is.EqualTo(ulong.MaxValue));
            Assert.That(_states[1].Count, Is.EqualTo(3));
            Assert.That(_flights.Arrivals, Is.Zero);
        }

        [Test]
        public void AutomaticBirthsUseTheirWithinStepAgeAcrossThePhaseBoundary()
        {
            var emitter = _data.Emitters[1];
            emitter.BurstCount = 1;
            _data.Emitters[1] = emitter;
            _data.Bursts[0] = new UIParticleBurst
            {
                Time = 0.125f,
                Count = 1,
            };
            Step(0.5, true);
            Assert.That(_states[1].Count, Is.EqualTo(1));
            Assert.That(_particles[1].Age, Is.EqualTo(0.375f));
            Assert.That(_flights.States[0].Start.x, Is.EqualTo(12));
            Assert.That(_particles[1].Position.x, Is.EqualTo(37).Within(0.0001));
        }

        private ParticleEmissionResult Emit(int count, float scale = 1)
        {
            UIParticleSimulation.Emit(ref _data, ref _particles, ref _states, ref _flipbooks, ref _flights, 1, count, 7, scale, ref _transform, out var result);
            return result;
        }

        private void Step(double delta, bool emitting = false)
        {
            UIParticleSimulation.Step(ref _data, ref _particles, ref _states, ref _streams, ref _flipbooks, ref _flights, delta, 7, emitting, ref _transform);
        }
    }
}
