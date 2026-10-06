# Anchor Showcase

Import **Anchor Showcase** from Package Manager, then choose **BovineLabs > Samples > Anchor Play**. The imported sample generates `Generated/Scenes/Main.unity`; use **Anchor Generate** to rebuild only its generated assets. Save open scenes before rebuilding. No host-project Anchor settings are required. Generation reimports the UI after the sample assembly loads so declared view-model types resolve on a fresh import.

One scene and a persistent bottom button bar expose five examples:

| Page | Explore |
| --- | --- |
| Bindings | Observable properties, computed dependencies, two-way controls, command eligibility and class binding |
| Collections | Grid and accordion row templates, selection, item commands and live collection edits |
| Particles | Sparkle, clipped confetti, ambient dust and panel-space bursts; seed, speed, emission, visibility and counters |
| Animations | Fade and scale-fade navigation, back stack, animated popups and USS class transitions |
| Burst ECS | 512 continuously oscillating baked buffer cells, an individual-cell meter, an isolated world and Burst `SystemProperty` publication through `UIHelper`; pulse, reset, speed and pause controls |

Inspect `UI` for declarative layout/bindings and `Scripts` for state ownership. The sample uses Core's theme tokens and Anchor's App UI theme. Particle effect and texture metadata is preserved from the earlier particle sample.

The catalog app resolves view-model services from each UXML `data-source-type`. Managed examples retain their state while changing tabs. Effects and animation callbacks are released on tab changes. The ECS page loads its baked scene into a separate world and disposes that world when leaving; its system pairs `UIHelper.Bind` and `Unbind` and publishes only unmanaged values from its main-thread Burst update. UI commands write requests consumed on the next update.

Generated scenes and panel settings belong to the imported sample. The app creates four unsaved custom transition instances and destroys them at shutdown; only the inner navigation host registers their IDs for animated popup dismissal. Copying individual scripts without their UXML, templates, effect assets or assembly definitions is not a complete import.
