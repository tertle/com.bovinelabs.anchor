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
It explicitly regenerates this sample's scene and produces `Temp/AnchorParticles/Player/AnchorParticles.exe`.
Use IL2CPP while the project's CoreCLR build support is broken.

Run the Player visibly with `-force-d3d12 --particle-runtime-results <absolute-output-directory>` to capture the sample and record
five repeated workloads: 1,000 particles / 32 emitters, hidden visibility watching, explicit pause, idle and detach/reattach.
Wait for `runtime-complete.txt` before inspecting `runtime-frames.csv`, `runtime-samples.png` and the Player log.
Counter value `-1` means unavailable. Hidden windows may suppress rendering and cannot provide valid visual measurements.

The old static-quad diagnostic scene and its rendering-fixture menu have been removed. When updating an existing import, replace its old imported folder so retired scripts and assets do not remain.
