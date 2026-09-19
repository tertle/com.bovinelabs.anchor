# BL-300 Editor runtime baseline — 19 September 2026

The user explicitly deferred **all builds and Player testing**. These results establish the bounded runtime's CPU baseline only.
They do not establish Player rendering, GPU/overdraw, clipping/opacity correctness, submission/copy cost or end-to-end UI frame cost.
BL-301 still owns the supported basic release; BL-302 owns measured optimization. No speedup or hardware capacity limit is claimed.

## Environment and method

- Unity 6000.7.0b1 / 6f112f2bea37, WindowsEditor/Mono, DX12, batch mode, linear color, VSync 0.
- AMD Ryzen 9 9950X3D, 32 logical processors; RTX 4070; 97,907 MB reported RAM; Windows 11 10.0.26200.
- Reported display 3440x1440. These CPU tests do not render to that display or measure its GPU work.
- Burst 2.0.0; Collections 6.7.0; Mathematics 1.4.0; Unity Performance Testing 6.7.0.
- Burst enabled with synchronous compilation. The correctness probe compiles and executes both complete production native call paths.
- Ten warm-ups, thirty measurements, one batch per measurement; seed 7. Setup restores state before every timed batch.
- Compilation, preparation, allocations and setup are outside measured intervals. The fixture restores the previous Burst preferences.
- Timing values are microseconds per **whole batch of the listed controls**, not per particle or per control. P95 uses nearest rank.
- This is one Editor run, with other Editors present on the machine, not five controlled Player runs or a measured noise envelope.
  Separately measured groups are not additive; cache/scheduling variation can make a combined median lower than an isolated group.

`SimulationBatch` advances an already-populated effect by 1/60 second. `AppearanceAndMeshBatch` samples appearance and fills chunked
vertices/indices. `CombinedBatch` performs both. `SimulationWithHalfDeathsBatch` advances with every second particle expiring and
stably compacts survivors. All four measured paths recorded **zero GC allocation samples** across every workload. These are the
Performance Testing allocation-event samples for these CPU paths, not engine-wide managed bytes or proof about Player panel dispatch.

## Runtime measurements

All particles are live before the measured update; the half-death workload ends with half that count. The other workloads retain it.

| Live particles | Controls | Simulation median | Appearance + mesh median | Combined median / p95 | Half-death simulation median |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 64 | 1 | 15.70 | 18.50 | 25.25 / 32.10 | 14.85 |
| 256 | 1 | 16.35 | 22.70 | 29.50 / 35.80 | 15.45 |
| 1,000 | 1 | 18.80 | 38.35 | 55.45 / 67.20 | 18.05 |
| 5,000 | 1 | 32.80 | 156.25 | 195.15 / 251.50 | 27.15 |
| 20,000 | 1 | 80.40 | 667.05 | 651.95 / 831.50 | 69.15 |
| 3,200 | 1 | 27.40 | 115.35 | 133.85 / 151.90 | 24.15 |
| 3,200 | 32 | 61.80 | 136.55 | 161.75 / 204.10 | 51.80 |
| 3,200 | 100 | 112.35 | 162.35 | 222.05 / 267.30 | 102.50 |

The measured AoS **particle stride is 52 bytes**. Instance storage includes particle capacity, quad scratch capacity, per-emitter
state/counts and emission-stream scratch. Every workload shares one immutable compiled revision of 2,752 native bytes; do not
multiply that by the control count. Allocator bookkeeping and UI Toolkit's mesh memory are not included in these payload sizes.

| Capacity | Controls | Particle bytes | Total instance native bytes | Shared compiled bytes |
| ---: | ---: | ---: | ---: | ---: |
| 64 | 1 | 3,328 | 6,004 | 2,752 |
| 256 | 1 | 13,312 | 23,668 | 2,752 |
| 1,000 | 1 | 52,000 | 92,116 | 2,752 |
| 5,000 | 1 | 260,000 | 460,116 | 2,752 |
| 20,000 | 1 | 1,040,000 | 1,840,116 | 2,752 |
| 3,200 | 1 | 166,400 | 294,516 | 2,752 |
| 3,200 | 32 | 166,400 | 298,112 | 2,752 |
| 3,200 | 100 | 166,400 | 306,000 | 2,752 |

## Mesh baseline comparison

The inherited direct quad-fill fixture was rerun unchanged. Its local-space fill cost remains comparable to the BL-299 observation;
these separate single-run observations cannot establish a statistically meaningful improvement or regression. Renderer submission,
copying and GPU costs remain unmeasured under the Player deferral.

| Quads | BL-299 median | BL-300 median / p95 | GC sample maximum |
| ---: | ---: | ---: | ---: |
| 64 | 9.40 | 9.35 / 13.60 | 0 |
| 256 | 12.25 | 11.45 / 17.20 | 0 |
| 1,000 | 21.45 | 21.20 / 32.50 | 0 |
| 5,000 | 76.60 | 76.00 / 150.50 | 0 |
| 20,000 | 294.55 | 293.45 / 426.40 | 0 |

## Execution and evidence

The first 120-second correctness attempt timed out during cold Burst compilation and supplied no verdict. The subsequent narrow
correctness run passed 54/54 with exit 0 and no compilation errors. The final correctness run passed **58/58**, exit **0**, including
real-panel detach, replacement and transformed-corner coverage. The explicit Performance run passed **13/13**, exit **0**.
Both final logs contain no compilation errors. No performance-path behavior changed after its measurements.

Evidence is retained outside Unity's temporary directory, under the assigned workspace's `Artifacts/BL300`:

- `BL300Correctness.xml`, `BL300Correctness.compact.txt`, `BL300Correctness.log`, `BL300CorrectnessLog.compact.txt`.
- `BL300Performance.xml`, `BL300Performance.compact.txt`, `BL300Performance.log`, `BL300PerformanceLog.compact.txt`.
- `PerformanceSamples.json`: the raw Performance Testing sample groups extracted from the XML.

The Editor emits an existing two-preview-scene shutdown warning, also present in the pre-existing `ParticleBuild.log`. It is not a
particle native-container leak report. Particle tests create no project assets and explicitly dispose runtime/native ownership.

Reproduce using the repository Unity target-selection workflow, with the absolute assigned workspace as `$projectPath` and an
existing ignored artifact directory as `$evidencePath`:

```powershell
unity --non-interactive test $projectPath --mode EditMode --filter '^BovineLabs\.Anchor\.Tests\.Particles\.' `
    --output "$evidencePath/BL300Correctness.xml" --timeout 600 -- `
    -logFile "$evidencePath/BL300Correctness.log" -assemblyNames BovineLabs.Anchor.Tests -testCategory '!Performance' --burst-force-sync-compilation
```

For the explicit performance pass use `-testCategory Performance` and distinct `BL300Performance` output/log names. The longer
timeout covers cold project/Burst compilation; the selected scope stays limited to Anchor particles. Neither command builds a Player.
