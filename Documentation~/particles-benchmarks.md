# Native quad rendering experiment (BL-299)

This is a rendering and measurement fixture, not the particle simulation or a released `AnchorParticles` control.
The sample's types are experimental. `ParticleQuad` and `ParticleQuadMesh` are the shared unmanaged input and mesh conversion.

## Rendering decision

Target: Unity **6000.7.0b1 (6f112f2bea37)**, Anchor **2.0.0-pre.4**. No Editor upgrade.
The installed Editor compiled these public calls:

```csharp
void MeshGenerationContext.AllocateTempMesh(int vertexCount, int indexCount,
    out NativeSlice<Vertex> vertices, out NativeSlice<ushort> indices);
void MeshGenerationContext.DrawMesh(NativeSlice<Vertex> vertices, NativeSlice<ushort> indices, Texture texture = null);
```

Each draw selects a texture. No material argument exists on this overload; custom materials belong to the element's style.
Four vertices and six triangle-list indices form a quad. The maximum is 65,535 **vertices**, not 65,535 indices.
Use 16,383 quads (65,532 vertices, 98,298 indices) per mesh. The 20,000-quad case submits two meshes in original order.
Counts are multiplied with checked arithmetic at allocation. Every chunk restarts its ushort indices at zero.
Fill requires one correctly sized chunk. `Draw` performs the splitting for arbitrary-length input.

Positions use element-local logical units, downward-positive Y, clockwise triangles and `Vertex.nearZ`.
Angles are radians; size is the full nonuniform extent. UV XY is the texture's bottom-left and ZW its top-right.
Top UI vertices receive the upper texture V coordinate. The chosen overload takes original texture coordinates.
Do not apply an old `MeshWriteData.uvRegion` transform: Unity's entry processor performs dynamic-atlas remapping after submission.
Vertex tint contains straight alpha. The helper does not multiply parent opacity; UI Toolkit applies hierarchy opacity.

`DrawMesh` records references, not an immediate copy. Fill UI-owned temporary slices inside `generateVisualContent`, submit them,
and never retain, dispose or overwrite them afterward. Input quad storage belongs to the control and is read synchronously.
It can be released on detach because the submitted vertices/indices belong to UI Toolkit.

Unity consumes entries after all dirty visual generators, registered mesh-generation dependencies and deferred callbacks finish.
The local Unity source (`MeshGenerationDeferrer.ProcessDeferredWork`) merges and **completes** registered jobs before dependent callbacks.
`AddMeshGenerationJob(JobHandle)` is a lifetime/fence contract, not proof of a no-stall pipeline.
The baseline calls Burst synchronously; quad counts are already known when allocating slices.
A future simulation must establish counts before allocation, or explicitly measure the completion needed to learn them.

References: [DrawMesh](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.MeshGenerationContext.DrawMesh.html),
[AddMeshGenerationJob](https://docs.unity3d.com/6000.6/Documentation/ScriptReference/UIElements.MeshGenerationContext.AddMeshGenerationJob.html).
The older documentation describes the ownership contract; compilation and Player evidence establish this checkout's support.

## Update and teardown decision

`ParticleFixtureClock` registers one scheduler on `panel.visualTree`, with an explicit `Time.frameCount` guard.
It requests mesh invalidation at most once per player frame, outside mesh callbacks. It does not simulate particles.
BL-300 can insert one unscaled-time sample and bounded simulation before invalidation in this hook, then let mesh generation consume it.
Panel scheduling occurs during UI update before the panel's visual generation; do not assume a particular ECS system order.
Manual Editor preview calls `PreviewStep(panel, frame)` with an explicit monotonically increasing preview frame; automatic play updates are disabled outside play.

The weak-key panel registry cannot keep discarded panels alive. Last unregister pauses the scheduled item and removes the entry.
Detach releases native input; reattach reconstructs the predefined fixture. Regeneration discards the old visual generation.
Empty, paused, zero-size and hidden controls skip repaint requests. Hidden ancestors are checked separately from the control's own style.
Clear invalidates once to remove the last mesh. Decorations ignore picking and focus.

## Reproduction

Import `Samples~/Particles` into the project's `Assets` as a single sample folder, preserving its metadata.
The isolated sample assembly requires Anchor, Collections and Mathematics. The existing Anchor test assembly stays Editor-only.
Unity Performance Testing **6.7.0** is installed in this Shattered checkout (confirmed by its live package manifest and run metadata);
only a test assembly reference was added. The transitive dependency request of 3.2.0 does not identify the installed built-in version.
No new runtime package dependency was added.

The sample's explicit authoring/build entry point is `BovineLabs.Anchor.Particles.Sample.ParticleFixtureBuild.Build`.
It creates the sample scene, builds a Windows x64 development Player at `Temp/AnchorParticles/Player/AnchorParticles.exe`,
temporarily enables frame timing and selects DX12, then restores project settings in `finally`.
It also restores the active build profile and the Editor development toggle. The scripted build explicitly selects
`StandaloneBuildSubtarget.Player` and `BuildOptions.Development | BuildOptions.StrictMode`; game build profiles must not supply
their scenes to this experiment. Addressables content generation is disabled only during this UI-only build and then restored.
`UNITY_DISABLE_AUTOMATIC_SYSTEM_BOOTSTRAP` isolates the UI experiment from automatic ECS worlds.
Use the repository's approved headless/connected workflow. Do not run alongside an Editor occupying the same project.

Launch with `-force-d3d12 -screen-fullscreen 0 -screen-width 1600 -screen-height 1000 --particle-results <absolute-directory>`.
Without the results argument the scene is a manual visual harness. Keep window state and resolution stable during measurement.
Use `python summarize.py <absolute-directory>` after `complete.txt` exists.

The fixed matrix includes empty UI, 100 idle/paused/hidden controls, 64/256/1,000/5,000/20,000 quads,
matched totals across 1/32/100 controls, large overlap, shared/multiple textures, masking and two panels.
The lifecycle workload clears and detaches/reattaches a batch of 100 controls on each measured frame; its marker is explicitly per batch.
The shared texture is a non-readable 2x2 point-filtered atlas candidate; the three additional 128x128 textures exceed the default 64-pixel atlas limit.
Small sprites use opaque tint; large 600x450 overlapping sprites use alpha 160/255. Positions and angles are deterministic, without random state.
Every Player workload uses five runs, each with 120 warm-up frames and 600 recorded frames.
The CPU fill microbenchmark uses 10 warm-ups and 30 measurements, with allocation and initial Burst compilation outside timing.

Raw `frames.csv` reports absolute frame times, FrameTimingManager CPU/GPU measurements, bounded mesh markers,
UI renderer processing, main/render thread time, engine-wide GC bytes, batches, engine memory and persistent quad input bytes.
Marker times are nanoseconds; vertex/index/input bytes and engine memory are bytes. `-1` means unavailable, never zero cost.
UI-owned retained allocator memory is included in engine memory, not falsely attributed to the control's input-byte counter.
The mesh submission marker excludes the renderer's later copy/processing; examine it alongside `RenderTreeManager.Process`.
The harness records data into preallocated arrays and writes CSV outside measured intervals.
The first lifecycle batch includes cold attachment/allocation effects; compare its spikes with later batches and later runs.

The reference build uses CoreCLR, Checked managed code with instrumentation, Burst compilation/optimisation enabled,
Burst AOT safety checks disabled, and x64 CPU target mask 72 from the checked-in Burst settings.
The build log is the authority for actual build configuration; the Player writes its hardware, graphics backend, resolution and cap settings.

## Evidence and gate

On 19 September 2026, the final headless EditMode run passed **12/12** tests with CLI exit **0** and a clean compact compilation log:
seven packing cases plus five Unity Performance Testing workloads. Scope: `^BovineLabs\.Anchor\.Tests\.Particles\.`.
Packing includes rotated nonuniform geometry, UVs, zero-alpha tint, winding and chunk boundaries.

The first timing pass used the Editor's disabled-Burst preference and was rejected as a native baseline.
The benchmark fixture now enables synchronous Burst compilation, asserts Burst is enabled, warms the function outside timing,
and restores both prior options in teardown. The final Burst-enabled CPU fill observations were:

| Quads | Median µs | p95 µs | Maximum µs | GC sample maximum |
| ---: | ---: | ---: | ---: | ---: |
| 64 | 9.40 | 12.20 | 12.60 | 0 |
| 256 | 12.25 | 17.20 | 44.50 | 0 |
| 1,000 | 21.45 | 31.60 | 31.60 | 0 |
| 5,000 | 76.60 | 121.60 | 143.40 | 0 |
| 20,000 | 294.55 | 502.00 | 749.20 | 0 |

Each row has 10 warm-ups and 30 measurements, one fill per measurement. Percentiles use the nearest rank.
GC values are the Performance Testing GC allocation sample, not an engine-wide byte measurement.
These are one Editor run's microbenchmarks, including native-slice dispatch/check overhead, **not** a Player capacity curve or five-run noise envelope.
There is no empty-panel/render/GPU baseline from this run and no baseline subtraction.

Hardware: AMD Ryzen 9 9950X3D (32 logical processors), NVIDIA RTX 4070, 97,907 MB reported system RAM, Windows 11 10.0.26200.
Run metadata: WindowsEditor/Mono, batch mode, DX12, linear color, 3440x1440 reported display, VSync 0, Unity 6000.7.0b1/6f112f2bea37.
Burst 2.0.0 and Performance Testing 6.7.0. Editor safety preferences were retained; Player AOT settings above do not establish Editor safety mode.
Fresh XML contains the complete raw samples and environment metadata. Handoff artifacts live in the assigned workspace at
`Temp/AnchorParticles/Evidence/ParticleFinal.xml`, `ParticleFinal.compact.txt`, `ParticleFinalLog.compact.txt`, and `ParticleBuild.log`.

The user explicitly deferred further builds during implementation. Player visual/performance validation is left unverified, not inferred from these tests.

The Windows/DX12 Player build reached Burst AOT and failed with **BC1045** under the current CoreCLR configuration.
The failing inputs are existing package containers of `System.ValueTuple`, including:

- Nerve targeting: `FixedList64Bytes<(Target, byte)>`.
- Core serialization: native lists/arrays of `(ulong, Serializer)` and `(ulong, Deserializer)`.
- Nerve subscene loading: native containers of `(Entity, NativeList<SubSceneBuffer>)`.

The compiler reports these as auto-layout structs. The final headless build exited 1; its Burst compiler ran about 128 seconds
before reporting the failures. These types are outside the particle helper. Disabling Burst or changing the scripting backend
would produce a different experiment, so neither was used to manufacture a passing result.

Reproduce after importing the sample:

```powershell
unity --non-interactive run <absolute-project-path> --timeout 1800 -- `
    -executeMethod BovineLabs.Anchor.Particles.Sample.ParticleFixtureBuild.Build -logFile <absolute-build-log>
```

No successful Player, Player timing table, screenshot, clipping/opacity pass, lifecycle pass or additive capability result is claimed.
The sample contains those checks and capture paths, but they require the build blocker to be resolved before acceptance.
The BL-299 core gate remains unverified. On 19 September 2026 the user explicitly authorized BL-300 implementation and Editor
validation while deferring Player testing and all builds. That override permits the runtime work; it does not supply the missing
Player evidence. See [the runtime measurements](particles-runtime-benchmarks.md) for BL-300's separate Editor results.
