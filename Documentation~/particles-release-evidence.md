# BL-301 release evidence

Historical measurement correction: BL-302 found that the Player harness's capacity-one profiler recorders omitted wraparound.
The profiler-marker and engine-GC values below therefore describe frozen initial samples, not complete per-frame distributions.
FrameTimingManager CPU/GPU timings, screenshots and automated tests are unaffected. Use the corrected
[BL-302 performance baseline](particles-performance-baseline.md) for current marker/allocation evidence.

Validation date: 2026-09-19. Unity 6000.7.0b1, Windows, Anchor basic alpha-particle feature.
These are development measurements on one workstation, not production budgets or cross-platform guarantees.

## Automated contracts

The connected Editor ran `BovineLabs.Anchor.Tests.Particles`: 87 distinct cases, all passing across the final runs.
This includes 73 correctness cases and 14 explicit Performance cases. Two runtime performance cases in the initial combined
run received unexpected error logs from diagnostic Pipeline requests while the main thread was occupied. The complete
8-case runtime performance fixture was rerun without diagnostic requests and passed 8/8.

Coverage includes control attach/detach, active asset replacement, visual-generation release and app disposal while attached,
deferred layout cancellation, hidden ancestors, explicit pause, zero-speed resume, effects-disabled suppression, completion
cancellation and handler restart, same-panel sources, singular transforms, transformed/reflected birth bases, compiled revision
ownership, simulation contracts and mesh clearing. Test assets are in memory.

Local raw evidence is retained under `Artifacts/BL301`: `FinalTestResults.json`, `ParticleTests.xml`,
`RuntimePerformanceTests.xml` and their Pipeline summaries. Those generated artifacts are not package content.

## Editor preview

A real `UIParticleEffectEditor` was constructed and bound to the imported Dust asset in an Editor window.
Its Play button started the production control, Pause retained 36 live particles, and closing the window cleared playback
and live state. No ECS world or AnchorApp was needed. A separate focused check verified that all four Editor mode-transition callbacks preserve new controls on entry and release ownership on exit. Imported sample UXML resolved all four authored effect references.

## Focused Editor measurements

The 1,000-particle / 32-emitter control fixture measured zero managed bytes across 100 warmed `Advance` calls.
All reported GC allocation sample groups were zero. These timings exclude rendering and do not imply that the entire Editor
or Player frame allocates nothing.

| Work | Median | Measurement unit |
| --- | ---: | --- |
| Active control advance, 1,000 particles / 32 emitters | 60.60 us | One call |
| Hidden-pause visibility watch | 87.25 us | Batch of 100 calls |
| Explicitly paused control | 7.65 us | Batch of 100 calls |

The Performance package's fixed-iteration path reports total elapsed time for the iteration batch. Hidden watching therefore
cost approximately 0.873 us per call in this run, and explicit pause approximately 0.077 us. Runtime idle coordinators suspend
their scheduler, so calling a paused control directly is a diagnostic upper layer cost, not scheduled idle simulation.

Particle state occupies 68 bytes and quad scratch 56 bytes. One 1,000-capacity, one-emitter instance reports 124,116 native
payload bytes plus 2,752 shared compiled bytes; emitter counts and authored tables change these totals. Temporary UI Toolkit
mesh slices belong to Unity and are outside those instance totals.

## Player / DX12

The Windows x64 Development/Strict fixture build passed with IL2CPP and DX12. The first attempt was cancelled during native
compilation; the cached retry completed in 114 seconds. The final corrected-sample build passed in 132 seconds. CoreCLR had failed earlier in unrelated Core/Nerve Burst tuple-layout
compilation. The project's original CoreCLR backend and build-generated Player settings were restored after validation.

The executable reported Direct3D12, AMD Ryzen 9 9950X3D, NVIDIA GeForce RTX 4070 and 1600 x 1000 resolution. It completed all five
runtime workloads (five runs of 600 measured frames after 120 warmup frames each), including 3,000 detach/reattach operations.
However, launching with a hidden window suppressed rendering: the capture was black and mesh markers were unavailable.
Those first-run timings under `Artifacts/BL301/Player` are excluded from the rendering/performance verdict.

The visible-window run completed 15,000 measured frames. The actual Player rendered the sample UI and particles; that capture
exposed a missing texture reference caused by the project's default cubemap import settings. The sample now explicitly imports
its original soft dot as Texture2D and assigns it to all four assets. The final visible DX12 capture verifies that authoring correction: sparkle, confetti, dust and the layered overlay render with the soft-dot texture.

| Workload | Median frame / CPU / GPU (ms) | Median simulation / mesh fill (us) | Engine GC bytes per frame |
| --- | --- | --- | --- |
| Active, 1,000 live particles / 32 emitters | 0.814 / 0.816 / 0.163 | 3.5 / 10.3 | 0 throughout |
| Hidden visibility watch | 0.777 / 0.779 / 0.144 | 0 / 0 | 0 throughout |
| Explicit pause | 0.789 / 0.792 / 0.161 | 0 / 0 | 0 throughout |
| Idle | 0.653 / 0.655 / 0.144 | 0 / 0 | 0 throughout |
| Detach/reattach each frame | 0.866 / 0.867 / 0.161 | 34.0 / 12.7 | 274,696 median |

Raw data and the machine-readable summary are in `Artifacts/BL301/PlayerFinal`. Each workload has five 600-frame runs with
120 warmup frames. Hidden/paused runs include pending-layout requests with zero particles as well as retained 1,000-particle
runs; they measure inactive control housekeeping, not a uniform live-particle population. Active and churn samples all report
1,000 live particles. Every churn detach asserts zero live state before reattachment; the ownership tests separately verify
release of the final compiled owner. Churn allocations are deliberately reported separately from steady-state simulation.

Inactive workloads have zero recorded simulation and mesh fill, and active steady state reports zero engine frame GC in this
fixture. Frame time includes the fixture/engine; it is not an isolated Anchor cost. Some frame-timing GPU samples are zero,
and unavailable render-thread/UI marker counters are excluded. The Player emits URP warnings about stripped post-processing
shaders unused by this UI fixture. Whole-engine shutdown memory diagnostics are not evidence of zero process-wide leaks.
