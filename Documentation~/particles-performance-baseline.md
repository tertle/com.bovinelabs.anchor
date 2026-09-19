# BL-302 measured particle baseline

Measured 20 September 2026 on Windows 11, Ryzen 9 9950X3D, RTX 4070, Unity 6000.7.0b1. Player: Windows x64 IL2CPP, Development/Strict, DX12, 1600 x 1000, VSync 0, uncapped, seed 7. The same machine, authored workloads and Burst settings were used before and after. Another workspace Editor was open; this is a workstation operating envelope, not a hardware-independent guarantee.

Baseline root: `862f1b5689818d95ee41cf49408df3251a22f2fb`; Anchor: `a482af6bee1057becfe4f3bac3c98c07adc57938`. The before Player contains the completed BL-301 runtime with only measurement-harness changes. The after Player contains the BL-302 working changes. CoreCLR was not used because its unrelated package Burst build failures were already established in BL-301. The original project backend/settings were restored after building.

**Closeout:** the user accepted publication after reviewing the results and lowered the default panel budget to 16,384 slots. The captures below used the earlier 32,768-slot default; the 20,000-particle stress harness now explicitly raises the budget. Matched captures and repeats still exceed some original regression bands amid demonstrated session drift. No speculative optimisation or performance improvement is claimed, and closeout does not turn those recorded failures into passing results.

## Decisions and delivered behavior

- Retain direct synchronous Burst simulation and direct fill of UI-owned temporary mesh slices. A single coarse scheduled job with its completion wait lost every tested execution case; no size threshold or alternate runtime path was shipped.
- Add a configurable 16,384-resident-slot panel budget, transactional play/replacement results, explicit Clear/detach reservation release, and opt-in live/reserved/spawn/rejection counters. The budget is admission policy, not a supported particle-count claim.
- Add bindable EmissionScale with persistent fractional credit per continuous/burst stream. Only future births change; no per-frame capacity growth or suppressed-emission replay is introduced.
- Preserve cached paused meshes. Retained panel-space particles watch the renderer transform and repaint only on change, because Unity transforms cached local vertices without re-running the mesh generator. This fixes correctness while paused.
- No AoS/SoA conversion, index cache, buffer-copy alternative, pooling, visibility-culling framework or GPU animation was introduced. Coefficients, compiled curves, transforms and texture metadata already sit outside particle loops. The retained rendering path preserves order, clipping, chunk rebasing and Unity buffer ownership.

## Capture validity and interpretation

Each Player workload has five runs of 120 warmup and 600 measured frames. The original 19-case BL-299 static matrix was rerun from the existing imported historical harness, with its workloads unchanged. The current runtime harness adds ten matched live-count/emitter combinations, each active, hidden, paused, idle and detach/reattach: 50 cases, 150,000 captured frames per version. Including static cases, the matched before/after evidence contains 414,000 frames.

The historical harness had capacity-one ProfilerRecorders without wraparound, which froze marker values after the first sample. Those preliminary captures are excluded. Both accepted captures use StartImmediately | Default (including wraparound). Hidden/paused effects are populated before changing their state; every retained runtime case verifies its exact live count.

The noise band is the full range of the five per-run medians, or the full range of per-run p95 values for p95 checks. A target improvement must be at least 10% and exceed twice its noise band. Regression tolerance is max(5% of baseline, twice the relevant absolute noise band). No runtime speedup is claimed from the required admission/quality additions.

The uncapped Player frequently renders faster than the panel scheduler's 1 ms interval. A frame with no scheduled particle update legitimately has zero particle markers. Native-path p95 below sums disjoint simulation, appearance, mesh allocation/fill/submission scopes per frame; it excludes coordinator/visibility work and Unity's later UI processing. The focused control microbenchmark includes control dispatch. All selected simulation/mesh work is synchronous on the main thread. Scheduled experiment times include scheduling and Complete; they are wall time, not summed worker CPU.

Worker-only timing and actual GPU vertex/index upload traffic were not available. The render-thread recorder is unavailable; the batches counter reports no useful values. GPU samples with no timing result remain unavailable rather than zero cost. Static-harness capture-array allocation can appear in the first recorded frame; it is harness setup, not steady-state Anchor allocation. Runtime lifecycle/churn allocations are reported separately. Occasional engine GC samples must not be attributed to Anchor without a call stack.

## Matched runtime Player measurements

All frame times are whole-Player wall time. Native scopes are microseconds; no empty-frame baseline was subtracted.

| Live / emitters | Frame median before / after ms | Frame p95 before / after ms | Baseline p95 noise ms | Native path p95 before / after us |
| --- | ---: | ---: | ---: | ---: |
| 64 | 0.375 / 0.364 | 0.556 / 0.563 | 0.146 | 6.40 / 4.70 |
| 256 | 0.369 / 0.393 | 0.520 / 0.553 | 0.113 | 9.20 / 12.30 |
| 1000 | 0.379 / 0.674 | 0.515 / 1.645 | 0.033 | 21.20 / 69.30 |
| 5000 | 0.530 / 0.846 | 0.797 / 2.080 | 0.502 | 150.80 / 312.10 |
| 20000 | 1.345 / 1.504 | 2.027 / 2.500 | 0.837 | 714.60 / 836.30 |
| 1000-on-32 | 0.381 / 0.378 | 0.599 / 0.561 | 0.174 | 24.90 / 24.30 |
| 1000-on-100 | 0.375 / 0.427 | 0.613 / 1.009 | 0.115 | 25.70 / 45.50 |
| 5000-on-32 | 0.533 / 0.636 | 0.744 / 1.122 | 0.233 | 134.20 / 186.60 |
| 5000-on-100 | 0.535 / 0.671 | 0.759 / 1.078 | 0.202 | 139.80 / 195.30 |
| mostly-empty-100 | 0.369 / 0.412 | 0.535 / 0.862 | 0.392 | 8.70 / 10.10 |

Full per-run spreads, all 50 cases and all recorded metrics are retained in `runtime-summary.json`; `comparison.json` records the gate results without discarding outliers.

### Gate audit

- 1000/active: frameMs p95 exceeds its baseline tolerance.
- 1000/active: particleNativePathNs p95 exceeds its baseline tolerance.
- 1000/hidden-watch: frameMs p95 exceeds its baseline tolerance.
- 1000/paused: frameMs p95 exceeds its baseline tolerance.
- 1000/idle: frameMs p95 exceeds its baseline tolerance.
- 1000/detach-reattach: frameMs p95 exceeds its baseline tolerance.
- 1000/detach-reattach: particleNativePathNs p95 exceeds its baseline tolerance.
- 1000-on-100/active: frameMs p95 exceeds its baseline tolerance.
- 1000-on-100/active: particleNativePathNs p95 exceeds its baseline tolerance.
- 1000-on-100/hidden-watch: frameMs p95 exceeds its baseline tolerance.
- 1000-on-100/detach-reattach: frameMs p95 exceeds its baseline tolerance.
- 1000-on-32/hidden-watch: frameMs p95 exceeds its baseline tolerance.
- 1000-on-32/paused: frameMs p95 exceeds its baseline tolerance.
- 1000-on-32/detach-reattach: frameMs p95 exceeds its baseline tolerance.
- 1000-on-32/detach-reattach: particleNativePathNs p95 exceeds its baseline tolerance.
- 20000/detach-reattach: frameMs p95 exceeds its baseline tolerance.
- 20000/detach-reattach: particleNativePathNs p95 exceeds its baseline tolerance.
- 256/hidden-watch: frameMs p95 exceeds its baseline tolerance.
- 256/idle: frameMs p95 exceeds its baseline tolerance.
- 256/detach-reattach: frameMs p95 exceeds its baseline tolerance.
- 256/detach-reattach: particleNativePathNs p95 exceeds its baseline tolerance.
- 5000/active: frameMs p95 exceeds its baseline tolerance.
- 5000/hidden-watch: frameMs p95 exceeds its baseline tolerance.
- 5000/paused: frameMs p95 exceeds its baseline tolerance.
- 5000/idle: frameMs p95 exceeds its baseline tolerance.
- 5000/detach-reattach: particleNativePathNs p95 exceeds its baseline tolerance.
- 5000-on-100/paused: frameMs p95 exceeds its baseline tolerance.
- 5000-on-100/detach-reattach: frameMs p95 exceeds its baseline tolerance.
- 5000-on-100/detach-reattach: particleNativePathNs p95 exceeds its baseline tolerance.
- 5000-on-32/idle: frameMs p95 exceeds its baseline tolerance.
- 5000-on-32/detach-reattach: frameMs p95 exceeds its baseline tolerance.
- 5000-on-32/detach-reattach: particleNativePathNs p95 exceeds its baseline tolerance.
- mostly-empty-100/hidden-watch: frameMs p95 exceeds its baseline tolerance.
- mostly-empty-100/idle: frameMs p95 exceeds its baseline tolerance.

## Static rendering matrix

| Workload | Frame median before / after ms | Frame p95 before / after ms | GPU median before / after ms |
| --- | ---: | ---: | ---: |
| empty | 0.358 / 0.342 | 0.484 / 0.506 | 0.143 / 0.142 |
| idle-100 | 0.346 / 0.345 | 0.479 / 0.508 | 0.139 / 0.139 |
| paused-100 | 0.348 / 0.370 | 0.480 / 0.517 | 0.158 / 0.157 |
| hidden-100 | 0.343 / 0.334 | 0.490 / 0.456 | 0.139 / 0.139 |
| quads-64 | 0.353 / 0.349 | 0.485 / 0.480 | 0.141 / 0.140 |
| quads-256 | 0.339 / 0.342 | 0.483 / 0.495 | 0.141 / 0.141 |
| quads-1000 | 0.358 / 0.355 | 0.523 / 0.473 | 0.158 / 0.156 |
| quads-5000 | 0.479 / 0.470 | 0.704 / 0.554 | 0.220 / 0.219 |
| quads-20000 | 0.918 / 0.929 | 1.446 / 1.268 | 0.477 / 0.477 |
| 1000-on-32 | 0.398 / 0.417 | 0.547 / 0.560 | 0.159 / 0.158 |
| 1000-on-100 | 0.543 / 0.551 | 0.732 / 0.712 | 0.158 / 0.158 |
| 5000-on-32 | 0.532 / 0.551 | 0.707 / 0.689 | 0.222 / 0.221 |
| 5000-on-100 | 0.687 / 0.668 | 0.929 / 0.765 | 0.227 / 0.225 |
| large-overlap | 3.542 / 3.540 | 4.436 / 4.003 | 3.505 / 3.505 |
| atlas-32 | 0.389 / 0.469 | 0.520 / 0.628 | 0.160 / 0.160 |
| textures-32 | 0.402 / 0.450 | 0.581 / 0.601 | 0.161 / 0.160 |
| masked-32 | 0.424 / 0.494 | 0.573 / 0.792 | 0.176 / 0.177 |
| two-panels | 0.463 / 0.542 | 0.619 / 0.779 | 0.164 / 0.166 |
| lifecycle-100 | 1.049 / 1.174 | 1.560 / 1.779 | 0.208 / 0.232 |

The large-overlap workload is predominantly GPU/fill-rate bound: its GPU time closely approaches whole-frame time. This supports reducing translucent coverage in content, not claiming that fewer CPU jobs or a different particle layout fixes fill rate. The source retains UI-owned slices and ordered adjacent texture grouping; no extra copy or cached-index branch was justified.

### Outliers and memory

| Workload | Worst captured frame before / after ms | Peak total engine memory before / after MiB |
| --- | ---: | ---: |
| 64/active | 4.651 / 3.246 | 509.42 / 509.31 |
| 1000/active | 2.944 / 8.376 | 518.83 / 518.34 |
| 1000-on-100/active | 3.234 / 4.100 | 513.61 / 513.26 |
| 20000/active | 7.754 / 14.011 | 575.14 / 575.14 |
| 1000/detach-reattach | 3.591 / 17.817 | 513.17 / 513.71 |

Maxima are retained rather than averaged away. Engine memory includes the fixture, Unity, graphics and native allocators; it is not an Anchor-only retained-memory or process-wide leak claim. Native ownership and reservation recovery are checked independently.

## Execution experiment (Editor)

Same Burst Step kernel, seed 7, stable half-death compaction, one coarse job covering all emitter ranges. Five repeats per mode, ten warmups and thirty samples per repeat, with state reset outside timing. The scheduled path completes before reading/reusing its native buffers. Outputs and survivor order are compared. No additional frame of latency is introduced into the shipping path.

| Particles / emitters | Direct median us | Schedule + Complete median us | Decision |
| --- | ---: | ---: | --- |
| 64,1 | 5.45 | 36.75 | Reject scheduling |
| 1000,1 | 7.40 | 39.95 | Reject scheduling |
| 1000,32 | 7.65 | 39.95 | Reject scheduling |
| 1000,100 | 8.45 | 42.05 | Reject scheduling |
| 5000,1 | 17.55 | 54.50 | Reject scheduling |
| 20000,1 | 55.20 | 92.95 | Reject scheduling |

## Preparation, replay and memory

Preparation includes asset/table construction; release measures the final shared owner. These lifecycle samples are separate from steady state and are not five-run Player speedup claims. Burst is warmed before replay timing. Editor medians (microseconds):

| Capacity | Prepare before / after | Replay before / after | Release before / after | Instance bytes after |
| --- | ---: | ---: | ---: | ---: |
| 64 | 24.25 / 34.80 | 20.75 / 12.00 | 1.00 / 1.80 | 8,100 |
| 1000 | 24.30 / 28.30 | 57.50 / 55.55 | 1.00 / 1.30 | 124,164 |
| 20000 | 45.45 / 44.50 | 613.95 / 622.20 | 1.70 / 1.70 | 2,480,164 |

Particle stride remains 68 bytes and quad scratch 56 bytes. Per-emitter state is 48 bytes plus a four-byte quad count; emission-stream scratch is now 56 bytes per continuous or authored burst stream. A one-emitter, one-burst 1,000-slot runtime owns 124,164 native payload bytes, plus 2,752 shared compiled bytes. Fractional accounting therefore adds 48 bytes per such instance, with no per-particle increase. NativeBytes excludes allocator metadata and Unity-owned mesh storage. Resident capacity is reused by replay and released by Clear/detach; there is no pool. A paused live effect retains its reservation.

A full 1,000-quad repaint writes 256,000 vertex bytes and 12,000 index bytes; 20,000 quads write 5,120,000 and 240,000 bytes across two rebased chunks. These are produced slice sizes, not measured GPU upload traffic. Unity owns the submitted temporary slices and may retain allocator storage after a frame; Anchor never reuses or disposes them.

## Correctness and allocation evidence

- Initial unchanged correctness: 73/73. Implemented particle scope plus Anchor visual-reload smoke: 86/86. Final control/repaint fixture: 18/18, including two added cases; 88 distinct correctness/smoke cases overall.
- Final explicit particle Performance scope: 23/23. All GC sample groups are zero, including simulation, appearance/mesh, replay, control update, hidden watching, pause and the execution experiment. A separate 100-update control check reports zero allocated managed bytes.
- Admission tests cover exact-capacity admission, paused reservations, rejected replacement preserving asset/seed/live state/pause, same-revision replay, Clear/detach recovery, configuration retained across last detach, budget reduction rejection, disabled suppression and deferred attachment rejection without replay.
- Quality tests cover fractional looping bursts across step partitions, changes to zero and back without suppressed replay, bounded overload counters and invalid scales. Existing tests retain deterministic motion, transformed/reflected births, shared revision ownership, chunk limits and stable compaction coverage.
- Repaint tests cover unchanged pause, changed tint, moved paused panel-space renderer, current-call live counts, final-death invalidation and repeated empty Clear. Tests create no project assets.

## Reproduction and evidence

Local raw evidence is in the assigned workspace under `Artifacts/BL302`: `BeforeStaticCorrected`, `BeforeRuntimeCorrected`, `AfterStatic`, `AfterRuntime`, the Before/After XML and compact test logs, and BeforeBuildCorrected/AfterBuild logs. The original benchmark Players are retained locally (BeforePlayer and AfterPlayer). Preliminary frozen-recorder captures contain EXCLUDED.txt and are not comparison evidence.

Use the Unity target-selection procedure and the narrow EditMode particle namespace with !Performance or Performance explicitly. The imported historical static harness is evidence only and is not reintroduced as a second shipping sample. The current sample uses ParticleSampleBuild.Build, writes outside Temp, and runs visibly with `-force-d3d12 -screen-fullscreen 0 -screen-width 1600 -screen-height 1000 --particle-runtime-results <directory>`. After runtime-complete.txt, run `python summarize-runtime.py <before-directory> <after-directory>`. See the sample README and particles.md for capacity/quality policy.


## Supplemental repeats and acceptance limits

The original static comparison flagged masked-32 and two-panels. Repeating the unchanged baseline binary also slowed the empty case from 0.484 ms p95 to 2.597 ms. This establishes session drift; it does not assign every difference to noise. The paired static repeat has no full-frame p95 gate failures using its own five-run baseline. Original failures remain reported above/in StaticComparison.json rather than being erased or pooled into a wider noise band.

| Repeated runtime workload | Frame p95 before / after ms | Native path p95 before / after us | Baseline frame p95 spread ms |
| --- | ---: | ---: | ---: |
| 64/active | 1.383 / 0.671 | 11.00 / 6.00 | 0.441 |
| 1000/active | 0.540 / 0.917 | 24.50 / 41.90 | 0.061 |
| 1000-on-32/active | 0.593 / 1.204 | 37.60 / 49.60 | 0.064 |
| 1000-on-100/active | 0.726 / 0.773 | 44.00 / 41.40 | 0.442 |
| mostly-empty-100/active | 0.555 / 0.607 | 8.50 / 9.80 | 0.315 |
| 20000/active | 1.840 / 4.420 | 692.90 / 1367.00 | 0.461 |

Repeat gate flags: 1000/active: frameMs, 1000/hidden-watch: frameMs, 1000-on-32/active: frameMs, 1000-on-32/idle: frameMs, 20000/active: frameMs, 20000/active: particleNativePathNs, 5000/hidden-watch: frameMs, 5000/detach-reattach: frameMs, 5000/detach-reattach: particleNativePathNs, 5000-on-32/idle: frameMs.

These repeats add 414,000 measured frames. The workstation variation prevents a clean claim that the complete after-state passes the original tight Player regression bands. No speculative runtime optimisation is retained and no runtime speedup is claimed. Functional/allocation checks pass; stable Player performance acceptance remains qualified by the recorded drift and gate flags.

## Deterministic death/respawn turnover

Both builds use the same updated harness. It freezes automatic scaled time and manually advances 1/64 second per rendered frame; lifetime and loop duration are both 1/64 second. The entire 1,000-particle population expires and refills each step. Automatic coordinator zero-delta work, when scheduled, is included in both versions. Every captured frame asserts exactly 1,000 live particles. Five runs per case, 120 warmup and 600 measured frames add 12,000 matched frames across both versions.

| Workload | Frame p95 before / after ms | Native path p95 before / after us | Simulation median before / after us |
| --- | ---: | ---: | ---: |
| 1000/turnover | 0.726 / 0.742 | 49.10 / 52.00 | 28.30 / 30.00 |
| 1000-on-32/turnover | 0.679 / 0.724 | 50.30 / 53.40 | 29.60 / 30.15 |

Turnover raw captures and comparison.json are retained in BeforeTurnover/AfterTurnover; BeforeTurnoverBuild.log and AfterTurnoverBuild.log identify their separate builds. Both builds and Players exited successfully (0). Both full-frame p95 gates pass. The 32-emitter native-path p95 fails: 50.3 to 53.4 us, a 3.1 us increase against a 2.515 us tolerance (baseline spread 1.2 us). This remains an acceptance failure, despite the small absolute cost; it is not relabelled as an improvement or averaged away. All original and supplemental valid captures total 840,000 measured frames.
