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
| TextureSource, Texture, Uv | Texture mode keeps the optional Texture2D and normalized rectangular UV region; white untextured quads remain valid. |
| Sprite | Sprite mode uses the sprite's texture, actual UV corners, visible rectangle and pivot. Rectangular/full-rect geometry only. |
| FlipX, FlipY | Mirror texture content, or mirror a sprite's geometry about its authored pivot. Defaults preserve existing assets. |
| Flipbook | Optional regular-grid animation for Texture mode. Sprite mode is static. |
| Flight | Optional scatter then cubic target flight. Requires Panel space; scatter + flight duration replaces Lifetime. |

Empty effects and unassigned controls are inert. Invalid configured inputs fail preparation; they are not silently repaired.
For runtime-authored changes call `effect.Invalidate()` after editing fields. The asset cache is identity + revision, with no deep
hashing or polling. It releases native tables when the final runtime using a revision releases it.

### Sprites and flipbooks

Sprite sources preserve aspect ratio: `Size` is the longer dimension of the original sprite rectangle in UI units. The visible
geometry and authored pivot are scaled relative to that rectangle, including rectangular packing rotation/reflection and trimming.
The compiler matches all four UV corners to their geometry; it does not assume an unrotated atlas rectangle. Tight-packed,
nonrectangular and non-quad sprite meshes, and split-alpha textures, fail preparation with a diagnostic. Import settings are never changed automatically.

Texture mode supports a regular grid inside `Uv`. Columns and rows are positive integers. Frames run left-to-right, top-to-bottom;
row zero is the top row. Whole Sheet uses every cell; Single Row selects one fixed row or samples a random row once per birth.
`StartFrame` is an inclusive integer range sampled once per birth, counted within the selected sheet or row. Its independent random
stream does not alter existing spawn positions, lifetimes, colors or motion.

Lifetime timing advances `Cycles` over normalized particle age. Frames Per Second uses absolute particle age. Zero cycles/rate freezes
the sampled start frame. `Loop` wraps within the sheet/row; when false, animation clamps at its last frame. Playback speed and pause
work through ordinary particle time. `Padding` insets each cell edge in source-texture pixels; it must leave a nonempty frame. Author
transparent/extruded gutters for filtered textures, and use appropriate filtering/mipmap settings to avoid neighbouring-frame bleed.
FlipX/FlipY mirror each selected frame without changing frame order; padding remains inside that cell.

Sprites are static sources. To animate a sheet, explicitly select Texture and author its compatible grid region; arbitrary SpriteAtlas
layouts are not flipbook grids. The inspector shows source-specific fields and the settings for the selected timing/row mode.

Source texture/sprite reimports and moves invalidate loaded effect descriptors for their next preparation. Runtime SpriteAtlas registration
invalidates a matching prepared sprite revision once. Existing runs retain their prepared texture references and geometry until replay,
as with other effect edits; restart them after changing source layout. Call `Invalidate()` yourself after procedural sprite geometry or texture-layout changes. No sprite arrays or asset
properties are queried per particle or frame.

## Control and playback

| Property | Default / behavior |
| --- | --- |
| Effect | Asset reference; an admitted replacement restarts an active request. A budget rejection preserves the old asset and run. |
| PlayOnAttach | `true` in runtime panels; a fresh run waits for positive, resolved geometry. No automatic Editor playback. |
| SimulationSpace | `Local`; `Panel` preserves birth position, size and orientation across subsequent source movement. |
| TimeMode | `Unscaled`; `Scaled` follows game time. Manual preview supplies its own elapsed time. |
| PlaybackSpeed | `1`; finite and nonnegative. Zero freezes time and does not consume the initial burst. |
| EmissionScale | `1`; finite and nonnegative. Scales new rate/burst emission with fractional credit; existing particles continue unchanged. |
| Seed | `1`; explicit deterministic replay seed. No automatic reseeding. |
| Tint | White; multiplies start/lifetime tint, independently of inherited opacity. |
| HiddenBehaviour | `Pause`; alternatives are `Continue` and `StopAndClear`. |
| EffectsEnabled | `true`; false cancels/clears and suppresses playback. True alone does not replay missed effects. |

The properties support binding and UXML attributes. Setters notify only on actual changes. `IsPlaying`, `IsPaused`, `LiveCount`
and `DroppedCount` are read-only diagnostic snapshots; counts are not broadcast per particle or frame. `Counters` exposes
attempted/emitted/dropped spawns for the current run, and `ReservedSlots` reports resident capacity.

`Play()` restarts with Seed; `Play(seed)` also sets it. `Play(effect, seed)` attempts a transactional asset replacement. Each returns
`ParticlePlayResult`: Started, Deferred (waiting for attachment/layout/visibility), Suppressed, NoEffect, BudgetExceeded or TargetUnavailable.
`LastPlayResult` also exposes the result of deferred startup and property-driven replacement. Rejected requests are not retried
automatically when capacity becomes available. `StopEmitting()` lets survivors finish. `Pause()` is an explicit pause,
independent of visibility. `Resume()` resumes a paused run, not a cancelled or completed run. `Clear()` cancels, empties geometry,
releases storage and its panel reservation, and cancels a pending layout start. Reattachment never restores old particles or wall time. Seed, space and time-mode changes
restart active requests with cleared state; they are not seamless remapping operations. Tint and speed changes apply live.

`Completed` fires once after natural emission ends and its last survivor dies, including after StopEmitting. Clear, replacement,
detach and disabled effects cancel instead. Notifications run after simulation/iteration; a handler may safely restart or detach.
Detach releases completion subscriptions, source references and native ownership. Wire events once per visual generation and
release presenters when it ends. `AnchorApp.ReleaseVisualGeneration` and Dispose also release still-attached particles.

Hidden ancestors count, whether hidden by display or visibility. Hidden Pause performs a lightweight visibility watch but no
particle simulation; explicit Pause does not watch unless StopAndClear must detect a hidden ancestor. Showing a hidden-paused run discards hidden time. Continue ages
while hidden and skips mesh work. StopAndClear requires a new Play after showing. Clipping and offscreen position do not kill particles.
Retained panel-space particles keep a lightweight renderer-transform watch even while paused: a changed renderer needs a fresh
inverse conversion because UI Toolkit otherwise transforms cached local vertices itself. An unchanged paused effect does not repaint.
Idle panels pause their particle scheduler; the final detach removes active coordinator ownership. Weak panel configuration survives
reattachment without retaining a discarded panel. Editor/domain/play-mode lifecycle hooks clear
registered ownership. Manually created `UIParticleRuntime` instances remain the caller's responsibility to Dispose.

## Capacity and content quality

Configure `UIParticleCoordinator.Get(panel).ParticleSlotBudget` before playback. The default is **16,384 resident particle slots per
panel**, a memory/admission limit rather than a performance guarantee. It may be raised for deliberate content or stress tests.
The budget counts allocated capacity, including paused live effects and completed effects retained for replay. It cannot be lowered
below current `ReservedSlots`. Call Clear at an explicit content lifecycle boundary to reclaim inactive storage; detach and visual
generation teardown also release it. There is no pool or automatic frame-time quality controller.

Admission reserves the complete new capacity before compiling or allocating the effect. A replacement needs headroom for both
the old and new allocations until it succeeds; a rejected replacement changes neither the old particles nor its seed/pause state.
Replaying the same prepared revision reuses its existing reservation. Per-emitter capacities retain drop-new overload behavior.
`LiveCount`, `ReservedSlots` and `RejectedPlays` on the coordinator are opt-in diagnostic reads, without per-frame binding notifications.
Standalone `UIParticleRuntime` allocations are caller-owned and are not part of a panel's admission budget.

EmissionScale does not resize storage. Continuous emission and each authored burst stream retain their own fractional credit across
steps/loop repetitions. Changing scale affects future emission only; zero consumes emission time without accumulating suppressed
births. Replay resets credit and counters. Existing lifetimes, motion and age are unaffected. Use EffectsEnabled for a full clear and
suppression switch, or Pause to freeze existing particles. Author tighter texture margins and smaller translucent quads when fill rate
dominates; more aggressive simulation batching cannot reduce their pixel cost.

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

Sprite and flipbook descriptors are optional compiled tables. Flipbook emitters allocate additional birth state: eight bytes per
reserved particle slot for the selected start frame and row. Stable compaction moves that state with its particle. Basic particle
and quad strides remain 68 and 56 bytes, and basic effects allocate no animation state. Sprites use the existing quad basis;
flipbooks update UV rectangles during mesh generation. Both retain adjacent-texture batching without new materials or vertex channels.

## Target flights and joined emission

Enable an emitter's `Flight` section to scatter using its existing shape, speed, acceleration and drag, then travel along a cubic
path. `ScatterDuration` is a nonnegative range; `FlightDuration` is a positive range. Their sum is the particle's finite lifetime,
so ordinary Lifetime is hidden and ignored. `PathBend` is a maximum perpendicular offset in panel UI units, sampled with a stable
independent seed per birth. `ArrivalRadius` allows early retirement only after scatter ends. Time along the curve is linear and
clamped; there is no indefinitely chasing particle or collision solver.

Flight effects require `SimulationSpace = Panel`; attempting to play or emit them in Local space throws an authoring error.
Assign `Target` and its explicit target-local `TargetPoint`, or set the bindable/UXML `TargetName` and call
`RefreshTarget(currentViewRoot)` once when the presenter creates that visual generation. A name miss is inert until an explicit
refresh. There is no per-frame name search. Rebind after recreating a view; do not reuse a previous generation's target.

The control samples the target's full world transform once per active update, including ancestor transforms and panel scaling.
Snapshot captures the destination **when scatter ends**. Follow uses the latest target sample and moves the endpoint-side control
point by the same delta, intentionally reshaping the remaining curve. A teleported target can therefore produce a position jump.
Snapshot still arrives at its captured point if the same target moves. Both modes cancel if that target detaches.

`Emit(emitterIndex, count, source, localPoint)` captures the source transform once and admits new particles immediately into the
asset's ordered emitter index. It joins existing particles without calling `Play()` or resetting their ages, sources or seeds.
An idle configured control starts a draining run with **no automatic rate or burst emission**. Joining a previously requested
Play retains that run's timeline. Requests can also join a paused run; their time and target sampling wait for resume. Source
elements are not retained by this API after the request, so their later movement/removal does not affect emitted particles.

`ParticleEmissionResult` returns a Status plus Attempted, Emitted and Dropped counts after quality scaling. Accepted may emit zero:
fractional `EmissionScale` credit is retained per emitter, while capacity exhaustion drops new births without emission debt.
Counters and fractional credit are retained with prepared storage until Clear or Play resets them. Suppressed, NoEffect,
SourceUnavailable, TargetUnavailable and BudgetExceeded are explicit non-admission results. Detached/unresolved source geometry
and an unconfigured/detached target are ordinary unavailable results; nonfinite points, invalid indexes/counts, and cross-panel
source/target references are programmer errors. A failed budget admission does not partially emit.

The runtime is synchronous: admission is bounded by emitter capacity and has no pending request queue. Even very large counts
do at most the available-capacity work, with saturating diagnostic counters. No managed object or callback is stored per request
or particle. Only enabled emitters allocate the 48-byte flight sidecar per reserved slot; compaction preserves birth order and
keeps flight/flipbook sidecars together. Ordinary particle and quad strides remain 68/56 bytes. Manual fractional credit adds one
double per emitter, independent of particle count. `NativeBytes` and `SharedCompiledBytes` include flight storage/descriptors.

`Arrived(int count)` is optional, aggregate **cosmetic** feedback, dispatched at most once after an update. The accumulator is
cleared before callbacks; a handler can emit again or clear/detach safely. Rejected or cancelled particles never arrive. Completed
fires once when the combined run drains; an arrival handler that joins more particles postpones completion until those drain too.
Never grant currency, inventory or experience from either notification. Change gameplay state in the initiating command, even when
effects are disabled or rejected; use arrival only for a visual pulse.

Changing Target cancels the old run and requires a new start. Moving the same target or changing its local point follows the
selected Snapshot/Follow behavior. Invalidating a prepared flight effect cancels it on its next update, including TargetMode edits;
the next Play/Emit prepares the new revision. Clear, disabled effects, target removal, renderer detach and visual-generation teardown
cancel without Completed. Hidden Pause freezes phase time and target sampling; reveal discards hidden elapsed time, and explicit
Pause continues to override visibility. Teardown clears target references and arrival subscriptions. The inspector preview has
draggable S/T handles and uses the same control/runtime with an explicit clock. **Emit 16** joins the first flight emitter (or
emitter zero for ordinary effects), including assets with no automatic rate or bursts.

## Samples and scope

Import **Anchor Showcase** from Package Manager, choose **BovineLabs > Samples > Anchor Play**, then select **Particles** from the button bar.
The page explores sparkle, clipped confetti, ambient dust, panel-space bursts, an eight-frame AnimatedSparkle and a rectangular SpriteSparkle
with an offset pivot. Playback, seed, speed, emission, visibility and counter controls apply to all six examples.
The authored effects and textures live under `Showcase/Effects`; the original four effects preserve their earlier metadata and appearance.
The separate reward card uses `TargetFlights.uxml`, two authored Snapshot/Follow overlays, three source buttons and one counter.
It demonstrates overlapping requests, moving/removing/restoring the target, pause/reveal and effects suppression. Its counter changes
immediately from each source command; arrivals only pulse the label. Switching mode cancels the previous visuals.
See the [Showcase README](../Samples~/Showcase/README.md) for generation and lifecycle ownership.

Particles use ordinary alpha quads only: no additive material,
GPU animation, scene collision, sub-emitter graph, ParticleSystem, VFX Graph or Deck integration.

## Validation and measurements

Data-only tests live under `BovineLabs.Anchor.Tests.Particles`, covering quad geometry, sprite corner/pivot mapping, flipbook timing,
validation, deterministic births and sidecar compaction. The earlier asset-dependent lifecycle/runtime fixtures were retired. Check
actual imports, atlas packing, inspector/preview, clipping, opacity and playback teardown in the assigned Editor or a focused Player.
Use the current test runner and explicitly include Performance when measuring the mesh and simulation/appearance fixtures.
Record CPU/GPU and engine GC separately from
Anchor's focused allocation tests. See [BL-301 evidence](particles-release-evidence.md) and the [BL-302 baseline](particles-performance-baseline.md).
These are historical captures from retired measurement harnesses; the Showcase is the current interactive sample and does not reproduce those capture commands.

### BL-303 validation — 17 October 2026

Unity 7000.0.0a9, Windows Editor, synchronous Burst. The data-only kernel fixture advances one populated control and samples its
appearance; it excludes UI submission, GPU uploads and frame time. Ten warmups and thirty one-iteration measurements were used.
The reference uses the original `29b3f1e` simulation/appearance kernels with unused argument adapters for the current fixture.

| Live particles | Basic reference median (µs) | Basic final median (µs) | Flipbook median (µs) |
| ---: | ---: | ---: | ---: |
| 64 | 12.80 | 13.00 | 13.25 |
| 1,000 | 23.45 | 22.95 | 38.40 |
| 5,000 | 70.20 | 66.55 | 145.45 |

All measured kernel GC sample groups were zero. The first candidate's higher disabled-feature timings prompted a separate ordinary
appearance loop; the final medians are within the reference's observed sample spread. These short paired CPU captures are not a
five-run noise calibration, a Player performance guarantee or a speedup claim. Flipbook cost is explicit and optional.

Editor checks covered the authored sample imports, a legacy effect without the new serialized fields, full-rect sprite aspect/pivot,
rectangular SpriteAtlas packing, source reimport invalidation, conditional inspector fields, frame rendering, clipping, opacity and
pause/clear. The imported atlas used no packing rotation; rotated/reflected corner mappings are covered by data-only tests. A 64-slot
basic or sprite runtime owned 8,100 native bytes; animation added exactly 512 bytes. Clearing the review controls returned all 704
reserved slots. No Player/GPU measurement or split-alpha support is claimed.
