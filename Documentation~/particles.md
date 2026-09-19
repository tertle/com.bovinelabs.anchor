# Native UI particles — runtime foundation (BL-300)

`UIParticleEffect` composes ordered emitters. `AnchorParticles` is a thin UI Toolkit control; `UIParticleRuntime` is the same
simulation with an explicit clock for preview and benchmarks. This is the foundation for BL-301, not the supported basic release.
Player rendering/performance evidence and all builds are deferred by the implementation request. The BL-299 mesh contract is reused;
its outstanding visual gate is not represented as a pass.

## Asset and playback

Create an effect through **BovineLabs > Anchor > Particle Effect**. The default is a one-second, non-looping 16-particle burst
with capacity 128. Configure the ordered `Emitters` list, assign the effect to the control, then call `Play(seed)`.
An empty effect or an unassigned control is inert. Invalid configured ranges throw during preparation; capacity never grows.

`Play` restarts; `StopEmitting` lets survivors finish; `Pause` freezes timing; `Resume` continues a paused run; `Clear` stops and
removes old geometry. These are main-thread APIs. Changing the asset or coordinate space clears playback. Detach releases instance
buffers and stops playback; reattachment requires a new `Play`. A `Play` requested before initial attachment starts on attachment.
These baseline policies will be completed with binding, authoring preview and production visibility/ancestor handling in BL-301.
Currently hidden attached controls continue their clock; explicit pause avoids simulation work. Repainting never advances time.

Mutating authoring fields requires `effect.Invalidate()` before preparing new instances. Running instances retain their original
immutable data. `Play` on a control prepares the new revision when needed. The cache is per asset identity and explicit revision;
native tables are released when their last runtime owner releases them. Dispose manually owned `UIParticleRuntime` instances.
There is no asset hashing, polling, Editor invalidation hook or per-frame curve evaluation.

## Simulation contract

Each effect instance owns one contiguous AoS particle buffer partitioned by emitter capacity. Surviving particles are stably
compacted in birth order. Persistent state contains position, velocity, age/lifetime, initial size, rotation/spin and start tint.
Appearance is derived into a reusable quad scratch buffer during mesh generation, never stored back into particles.
`ParticleStride`, `ParticleCapacityBytes`, `NativeBytes` and `SharedCompiledBytes` report actual allocation sizes (shared bytes are
counted once per compiled revision, not once per instance). Mesh vertices and indices belong to UI Toolkit.

Each update consumes at most four substeps of at most 1/60 second, including a shorter final step. Excess wall time is discarded.
Delay is consumed once per restart. Continuous emission retains fractional credit. Bursts include time zero exactly once, even
with repeated zero-delta calls. Burst metadata is stably sorted at preparation; asset order is unchanged. Simultaneous continuous
emission precedes bursts, and equal-time bursts retain their authored order.

Counts across loops are computed analytically. Only the first available-capacity spawn attempts in each substep are materialized;
the remainder are dropped with no retry or catch-up debt. Admission happens after survivor compaction and does not reuse slots
from newborns that die inside the same substep. This deliberately does not promise partition-invariant saturation.
Work is bounded by four passes over particle capacity and emitter/burst metadata, independent of rejected spawn counts.
Attempted/emitted/dropped counters saturate at `ulong.MaxValue`. RNG is local to emitter index and 32-bit spawn ordinal; dropped
attempts advance that ordinal analytically, modulo 2^32. Same seed and stepping replay identically; no cross-platform lockstep promise.

Newborns are advanced only from their within-step birth time. Constant acceleration and linear drag use the analytic solution of
`dv/dt = acceleration - drag * velocity`, with a small-drag series to avoid cancellation and division by zero. Rotation advances
linearly. At or past lifetime particles are removed. Zero-size particles still age, and particles outside layout bounds are not killed.

The circle shape samples radius with a square root for uniform area; dimensions X is its diameter. Line is horizontal with length X;
rectangle uses full X/Y extents. Authoring angles are degrees; native geometry uses radians. Units are logical UI units.

## Appearance and rendering

Size and blended-color lifetime curves use **128 samples including both endpoints**, linearly interpolated at runtime. This is sampled
visual behavior, not exact managed curve parity. Fixed gradients additionally retain color/alpha key boundaries and step values,
including exact boundary values, so the table never blends across a fixed transition.

Quads preserve asset emitter order and particle birth order. Only contiguous emitters sharing a texture merge. Start color,
lifetime color and explicit control tint multiply once; UI Toolkit applies hierarchy opacity. Zero-size particles omit geometry.
Oversized draws use BL-299's 16,383-quad chunks and checked count arithmetic. UVs are normalized original-texture coordinates;
Unity handles dynamic-atlas remapping. No custom material, ParticleSystem, VFX Graph or Deck dependency is used.

Local-space particles stay in control coordinates. Panel-space births transform positions as points and velocities as vectors;
existing positions/velocities then stay in logical panel coordinates. Acceleration, size and rotation use the selected simulation
space. Panel-space geometry transforms each corner back to local coordinates, retaining the full inverse matrix rather than an
offset approximation. Full ancestor/layout/visibility behavior and authoring UX remain BL-301 scope.

The panel coordinator shares one unscaled frame-time sample and frame guard across controls. The mesh callback only reads runtime
state, fills UI-owned temporary mesh slices synchronously through Burst, and submits them. It never retains, overwrites or disposes
those slices. Clear and last-particle death request a final repaint. There are no outstanding consumer jobs during replacement/disposal.

## Validation

Plain NUnit tests and Performance Testing fixtures are under `BovineLabs.Anchor.Tests.Particles`. Use the shared namespace with
`!Performance` for correctness, then explicitly select `Performance`. Tests use only in-memory assets. Preparation is outside steady-state
timings. See `particles-benchmarks.md` for the inherited rendering baseline and `particles-runtime-benchmarks.md` for this stage's evidence.
The runtime sample and explicit stepping window are under `Sample~/Particles` alongside the preserved BL-299 fixture.
