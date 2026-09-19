# Native particle rendering fixture

BL-300 adds a separate **Window > Anchor > Particle runtime example** after importing this folder. It composes a flash, sparks,
and continuous ambient emitter in one in-memory effect. Restart, stop, pause, resume, clear and explicit 1/60-second stepping use
the production `AnchorParticles` control. The original BL-299 fixture below remains unchanged. This is a runtime demonstration,
not the asset inspector/preview planned for BL-301. Player visual/performance evidence remains deferred.

BL-299 experiment: predefined quads, not a particle simulation. See `Documentation~/particles-benchmarks.md` for the contract,
workload matrix and current build blocker. The visual and Player performance gate has **not passed**.

Import this folder into `Assets`, preserving `.meta` files. Open `ParticleFixture.unity` and play for the manual fixture.
Buttons exercise clear/last-quad removal, regeneration, detach/attach, panel scale, zero-size/resize and parent opacity.
The foreground button should receive input while the particle decorations ignore picking and focus.
The UV diagnostic should show red/green above blue/white, tinted by the quad's color.

`Window > Anchor > Particle rendering fixture` opens the same UXML for manual Editor stepping.
Only **Preview step** advances its explicit frame clock outside Play Mode; repainting never advances the fixture.

Use the approved Unity workflow to invoke `BovineLabs.Anchor.Particles.Sample.ParticleFixtureBuild.Build` for the isolated DX12 Player.
Its scene generation is an explicit authoring operation, separate from automated tests. This replaces this sample's scene only.
The resulting executable is `Temp/AnchorParticles/Player/AnchorParticles.exe`.

Launch with `-force-d3d12 -screen-fullscreen 0 -screen-width 1600 -screen-height 1000 --particle-results <absolute-output-directory>`.
Keep the machine otherwise idle. The runner captures visual states, checks basic lifecycle ownership, probes the default material,
then records 19 workloads with 120 warm-up frames and 600 measured frames per run, repeated five times.
Do not compare interrupted runs or treat an incomplete build's executable as usable.

Run `python summarize.py <output-directory>` only when `complete.txt` exists. Raw counters use `-1` for unavailable data.
Inspect the captures and logs as well as the timing table: fast but incorrectly rendered geometry does not pass this experiment.
