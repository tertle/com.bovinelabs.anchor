# Native UI particles

Import **Native UI particles** from Package Manager. Open `ParticleSample.unity`, enter Play mode, and click **Play all**.
The scene shows animated button sparkle, clipped confetti, ambient dust and an explicitly placed layered overlay.
Use Stop emitting, Pause, Resume, Clear, Effects enabled and Hide cards to explore playback and visibility.

For an independent Editor preview, open **BovineLabs > Samples > Anchor Particles** and click **Play all**.
Select Sparkle, Confetti, Dust or Layered to edit the effect and use its inspector preview. Previews start only on request.
`RuntimeParticles.uxml` and `RuntimeParticles.uss` author the layout; `ParticleSamplePresenter` wires the controls and commands.
A separate `ParticleSampleController` supports using the same UXML with an existing UIDocument.

## Runtime measurements

The optional build entry point is `BovineLabs.Anchor.Particles.Sample.ParticleSampleBuild.Build`.
It explicitly regenerates this sample's scene and produces `Artifacts/AnchorParticles/Player/AnchorParticles.exe`, outside Unity's temporary directory.
Use IL2CPP while the project's CoreCLR build support is broken.

Run the Player visibly with `-force-d3d12 -screen-fullscreen 0 -screen-width 1600 -screen-height 1000 --particle-runtime-results <absolute-output-directory>`.
It captures the sample and measures ten population/emitter combinations, each active, hidden, paused, idle and detach/reattach.
Each case has five repeats, 120 warmup frames and 600 measured frames. Retained inactive effects have the same live count as active effects.
Wait for `runtime-complete.txt` before inspecting `*-frames.csv`, `runtime-samples.png` and the Player log.
Run `python summarize-runtime.py <before-directory> [after-directory]` to validate complete captures, report repeat-run noise bands,
and compare matched metrics. The noise band is the full range of the five per-run medians (or p95 values for the p95 gate).
Profiler recorders wrap their capacity-one buffers so every frame reads a fresh completed sample.
Counter value `-1` means unavailable. Hidden windows may suppress rendering and cannot provide valid visual measurements.
Add `--particle-turnover` to capture only sustained 1,000-particle death/respawn across one and 32 emitters.
This mode freezes automatic scaled time and explicitly advances by 1/64 second per frame, matching the authored lifetime and loop period.
It verifies that every captured frame retains exactly 1,000 live particles; use separate output directories for the two capture modes.

Default examples fit within the panel's 16,384-slot admission budget. The measurement harness explicitly raises it for the 20,000-particle stress case.
Configure `UIParticleCoordinator.Get(panel).ParticleSlotBudget`
before deliberate larger workloads. Bind `emission-scale` to content quality without changing live particle lifetimes or allocating
new buffers; Clear releases unused reservations. Neither the budget nor these desktop measurements are a universal supported-count promise.

The old static-quad diagnostic scene and its rendering-fixture menu have been removed. When updating an existing import, replace its old imported folder so retired scripts and assets do not remain.
