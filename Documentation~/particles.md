# Native UI particles

`UIParticleEffect` is an ordered, bounded alpha-particle asset. `AnchorParticles` is a reusable UI Toolkit control, independent of
an ECS world, camera or `AnchorApp`. Author placement in UXML/USS. Its `bl-anchor-particles` class is stable; particles ignore
picking and keyboard focus. The basic feature includes lifecycle, inspector preview and samples. See
[release evidence](particles-release-evidence.md) for the tested scope and outstanding release gates.

## Authoring and preview

Create **BovineLabs > Anchor > Particle Effect**. The default is a one-second, non-looping 16-particle burst with capacity 128.
The normal serialized inspector supports Undo and multi-edit. Emitters render in list order; validation reports invalid ranges
and combined capacity overflow. The inspector's independent preview starts only when Play is pressed. Pause freezes it, Restart
replays the chosen seed, and Clear releases preview storage. Choose a background to inspect alpha. Asset edits explicitly invalidate
the compiled revision and restart a running preview; other live instances retain their original immutable revision until replay.
Closing the inspector, recompiling or changing Play Mode stops and releases the preview. UI Builder does not automatically play effects.

| Emitter fields | Contract |
| --- | --- |
| Duration, StartDelay, Looping | Positive duration; one initial nonnegative delay per Play; looping repeats the duration. |
| Rate, Bursts | Nonnegative continuous particles/second; timed bursts in `[0, Duration)`, including time zero. |
| MaxParticles | Positive fixed capacity for this emitter. Excess births are dropped, counted and never replayed. |
| Shape, Offset, Dimensions | Point, horizontal line, rectangle interior or uniform-area circle. Full dimensions; circle uses X as diameter. |
| Direction, Spread | Emission direction and spread in degrees. UI positive Y points down. |
| Lifetime, Speed, Size, Rotation, AngularVelocity | Min/max ranges; lifetime positive; speed and size nonnegative; angles/spin in degrees. |
| StartColorMin/Max | Components sampled once per birth. |
| Acceleration, Drag | Constant acceleration in the simulation space and nonnegative linear drag. |
| SizeOverLifetime, ColorOverLifetime | Normalized-age appearance, sampled as described below. |
| Texture, Uv | Optional Texture2D and normalized rectangular UV region; white untextured quads are valid. |

Empty effects and unassigned controls are inert. Invalid configured inputs fail preparation; they are not silently repaired.
For runtime-authored changes call `effect.Invalidate()` after editing fields. The asset cache is identity + revision, with no deep
hashing or polling. It releases native tables when the final runtime using a revision releases it.

## Control and playback

| Property | Default / behavior |
| --- | --- |
| Effect | Asset reference; replacement cancels the old run and releases its storage. An active request restarts with the replacement. |
| PlayOnAttach | `true` in runtime panels; a fresh run waits for positive, resolved geometry. No automatic Editor playback. |
| SimulationSpace | `Local`; `Panel` preserves birth position, size and orientation across subsequent source movement. |
| TimeMode | `Unscaled`; `Scaled` follows game time. Manual preview supplies its own elapsed time. |
| PlaybackSpeed | `1`; finite and nonnegative. Zero freezes time and does not consume the initial burst. |
| Seed | `1`; explicit deterministic replay seed. No automatic reseeding. |
| Tint | White; multiplies start/lifetime tint, independently of inherited opacity. |
| HiddenBehaviour | `Pause`; alternatives are `Continue` and `StopAndClear`. |
| EffectsEnabled | `true`; false cancels/clears and suppresses playback. True alone does not replay missed effects. |

The properties support binding and UXML attributes. Setters notify only on actual changes. `IsPlaying`, `IsPaused`, `LiveCount`
and `DroppedCount` are read-only diagnostic snapshots; counts are not broadcast per particle or frame.

`Play()` restarts with Seed; `Play(seed)` also sets it. `StopEmitting()` lets survivors finish. `Pause()` is an explicit pause,
independent of visibility. `Resume()` resumes a paused run, not a cancelled or completed run. `Clear()` cancels, empties geometry,
and cancels a pending layout start. Reattachment never restores old particles or wall time. Seed, space and time-mode changes
restart active requests with cleared state; they are not seamless remapping operations. Tint and speed changes apply live.

`Completed` fires once after natural emission ends and its last survivor dies, including after StopEmitting. Clear, replacement,
detach and disabled effects cancel instead. Notifications run after simulation/iteration; a handler may safely restart or detach.
Detach releases completion subscriptions, source references and native ownership. Wire events once per visual generation and
release presenters when it ends. `AnchorApp.ReleaseVisualGeneration` and Dispose also release still-attached particles.

Hidden ancestors count, whether hidden by display or visibility. Hidden Pause performs a lightweight visibility watch but no
particle simulation; explicit Pause does not watch unless StopAndClear must detect a hidden ancestor. Showing a hidden-paused run discards hidden time. Continue ages
while hidden and skips mesh work. StopAndClear requires a new Play after showing. Clipping and offscreen position do not kill particles.
Idle panels pause their particle scheduler; the final detach removes the coordinator. Editor/domain/play-mode lifecycle hooks clear
registered ownership. Manually created `UIParticleRuntime` instances remain the caller's responsibility to Dispose.

## Coordinates, clipping and ordering

Local particles move and scale with their control. Panel births use the source's full ancestor transform in panel logical units;
positions are points and velocities are vectors. Each particle retains its birth basis, including nonuniform scale/shear/reflection,
so later source or renderer transforms cannot resize old panel particles. Acceleration remains in the selected simulation space.
The renderer converts corners back to local coordinates and repairs reflected triangle winding. Singular source/renderer transforms
suspend updates and rendering until invertible; pending layout never emits at an arbitrary origin.

`SetSourcePoint(source, localPoint)` supplies an origin for future births, preserving the source's linear basis. Source and renderer
must be attached to the same panel; otherwise the call throws. A source detached afterward suspends the request until replaced or
reattached. `ResetSourcePoint()` returns to the control's own origin. This is a coordinate input, not a target-flight API.

Place the control inside a card/ScrollView to inherit clipping. To escape clipping, explicitly author a separate overlay control
and supply a source point from the button/card. Controls never reparent themselves or move globally to the front. Painter order is
emitter order, then birth order. Only contiguous compatible textures merge. UI Toolkit applies inherited opacity once.

## Time, storage and rendering

Simulation consumes at most four substeps of at most 1/60 second, including a shorter final step. Excess wall time is discarded
without catch-up debt. Delay is consumed once per restart. Continuous emission retains fractional credit. Time-zero bursts run once;
zero-speed control updates consume nothing. Stable sorting of equal-time bursts preserves author order; simultaneous continuous
emission precedes bursts. Bounded admission happens after survivor compaction and does not reuse newborn deaths within the substep.
This is deterministic for the same seed, stepping and inputs, not a cross-platform lockstep or saturation partition-invariance promise.

Attempted/emitted/dropped counters saturate at `ulong.MaxValue`. RNG is local to emitter and spawn ordinal; aggregate rejected
births advance the ordinal without iterating rejected particles. Work is bounded by capacity and emitter/burst metadata.
Motion uses the analytic constant-acceleration/linear-drag solution, with a small-drag series near zero. Particles at lifetime die;
zero-size particles still age. Stable compaction preserves alpha ordering.

Size and blended color curves use **128 samples including both endpoints**, with linear interpolation. Fixed gradients retain exact
key boundaries and step values. This is sampled visual behavior, not exact managed curve parity. No managed curves run per particle.

Each runtime owns fixed native buffers, keeping reusable capacity while attached. Particle state is now 68 bytes including the birth
basis; quad scratch is 56 bytes. `ParticleStride`, `ParticleCapacityBytes`, `NativeBytes`, and `SharedCompiledBytes` report actual
payload sizes. Shared compiled data is counted once per revision. UI Toolkit owns submitted temporary vertex/index slices: Anchor
fills synchronously through Burst and never retains, overwrites or disposes them after submission. The final particle requests one
last repaint to clear old geometry. Draws split at 16,383 quads; normalized original-texture UVs use Unity's atlas remapping.

## Samples and scope

Import **Native UI particles** from Package Manager. Open `ParticleSample.unity`, enter Play mode and click **Play all**, or open **BovineLabs > Samples > Anchor Particles**. The sample includes
editable Sparkle, Confetti, Dust and Layered assets, original procedural soft-dot art, clipped and explicit-overlay placements,
pause/clear and effects-disabled controls. Effects start only on request. The presenter uses Anchor RelayCommands, resolves controls
once, and unregisters on disposal. In an Anchor app create it in OnVisualGenerationInitialized and dispose it in
OnVisualGenerationShuttingDown. For a standalone UIDocument use RuntimeParticles.uxml with ParticleSampleController.

The existing Basic UI sample remains available. This stage includes ordinary alpha quads only: no additive material, flipbook,
target-flight, GPU animation, scene collision, sub-emitter graph, ParticleSystem, VFX Graph or Deck integration.

## Validation and measurements

Tests live under `BovineLabs.Anchor.Tests.Particles`, including coordinate, lifecycle and shared-revision ownership contracts.
Select correctness fixtures separately from explicit Performance tests when the connected test runner cannot combine filters.
The sample fixture supports `--particle-runtime-results <directory>` for five repeated 1,000-particle / 32-emitter workloads:
active, hidden visibility watching, explicit pause, idle and detach/reattach churn. Record CPU/GPU and engine GC separately from
Anchor's focused allocation tests. See [BL-301 evidence](particles-release-evidence.md); older stage baselines are historical and
must not be treated as current measurements of the larger birth-basis state.
