namespace BovineLabs.Anchor.Particles.Sample
{
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using BovineLabs.Anchor.Elements;
    using Unity.Profiling;
    using UnityEngine;
    using UnityEngine.UIElements;

    public static class ParticleRuntimeMeasurements
    {
        public static IEnumerator Run(VisualElement surface, string directory)
        {
            Directory.CreateDirectory(directory);
            var turnover = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "--particle-turnover") >= 0;
            File.WriteAllText(Path.Combine(directory, "environment.txt"),
                $"Unity {Application.unityVersion}\n{SystemInfo.graphicsDeviceType}\n{SystemInfo.processorType}\n" +
                $"{SystemInfo.graphicsDeviceName}\n{Screen.width}x{Screen.height}\n" +
                $"seed 7; VSync {QualitySettings.vSyncCount}; cap {Application.targetFrameRate}\n");
            foreach (var workload in new[]
            {
                ("64", 64, 1), ("256", 256, 1), ("1000", 1000, 1), ("5000", 5000, 1), ("20000", 20000, 1),
                ("1000-on-32", 1000, 32), ("1000-on-100", 1000, 100), ("5000-on-32", 5000, 32), ("5000-on-100", 5000, 100),
                ("mostly-empty-100", 64, 100),
            })
            {
                if (!turnover || workload.Item1 == "1000" || workload.Item1 == "1000-on-32")
                {
                    yield return RunWorkload(surface, directory, workload.Item1, workload.Item2, workload.Item3, turnover);
                }
            }

            File.WriteAllText(Path.Combine(directory, "runtime-complete.txt"),
                turnover ? "Completed two turnover workloads, five runs each." : "Completed ten runtime workloads, five modes, five runs each.");
        }

        private static IEnumerator RunWorkload(VisualElement surface, string directory, string name, int total, int emitters, bool turnover)
        {
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            effect.Emitters.Clear();
            for (var i = 0; i < emitters; i++)
            {
                var count = (total / emitters) + (i < total % emitters ? 1 : 0);
                effect.Emitters.Add(new UIParticleEmitterSettings
                {
                    MaxParticles = System.Math.Max(1, count),
                    Offset = new Vector2(50 + ((i % 8) * 110), 50 + ((i / 8) * 100)),
                    Lifetime = turnover ? new Vector2(1f / 64, 1f / 64) : new Vector2(1000, 1000),
                    Duration = turnover ? 1f / 64 : 1,
                    Looping = turnover,
                    Speed = new Vector2(1, 4),
                    Size = new Vector2(3, 5),
                    Bursts = new()
                    {
                        new UIParticleBurst
                        {
                            Count = count,
                        },
                    },
                });
            }

            surface.Clear();
            UIParticleCoordinator.Get(surface.panel).ParticleSlotBudget = System.Math.Max(UIParticleCoordinator.DefaultParticleSlotBudget, total);
            var parent = new VisualElement();
            surface.Add(parent);
            var particles = new AnchorParticles
            {
                Effect = effect,
                PlayOnAttach = true,
                Seed = 7,
                TimeMode = turnover ? UIParticleTimeMode.Scaled : UIParticleTimeMode.Unscaled,
            };
            particles.style.width = 1000;
            particles.style.height = 500;
            parent.Add(particles);
            var names = new[]
            {
                "Anchor.Particles.Simulation", "Anchor.Particles.Appearance", "Anchor.Particles.MeshFill",
                "Anchor.Particles.MeshAllocate", "Anchor.Particles.MeshSubmit", "RenderTreeManager.Process", "GC Allocated In Frame",
                "Main Thread", "Render Thread", "Batches Count", "Total Used Memory",
            };
            var recorders = new ProfilerRecorder[names.Length];
            var frames = new double[600, names.Length + 4];
            var timing = new FrameTiming[1];
            using var csv = new StreamWriter(Path.Combine(directory, name + "-frames.csv"));
            csv.WriteLine("mode,run,frame,frameMs,cpuMs,gpuMs,live," + string.Join(",", names));
            var previousTimeScale = Time.timeScale;
            try
            {
                if (turnover)
                {
                    // Automatic scaled updates become zero-delta; explicit exact steps expire and refill the entire population each frame.
                    Time.timeScale = 0;
                }

                foreach (var mode in turnover ? new[] { "turnover" } : new[] { "active", "hidden-watch", "paused", "idle", "detach-reattach" })
                {
                    for (var run = 0; run < 5; run++)
                    {
                        parent.style.display = DisplayStyle.Flex;
                        particles.Play();
                        // Resolve deferred layout and births before pausing/hiding; every retained case has the same live population.
                        yield return null;
                        yield return null;
                        if (particles.LiveCount != total)
                        {
                            throw new System.InvalidOperationException("Runtime workload did not reach its exact live count.");
                        }
                        if (mode == "hidden-watch")
                        {
                            parent.style.display = DisplayStyle.None;
                        }
                        else if (mode == "paused")
                        {
                            particles.Pause();
                        }
                        else if (mode == "idle")
                        {
                            particles.Clear();
                        }

                        for (var i = 0; i < 120; i++)
                        {
                            if (turnover)
                            {
                                particles.Advance(1d / 64);
                            }

                            yield return null;
                        }

                        for (var i = 0; i < names.Length; i++)
                        {
                            recorders[i] = new ProfilerRecorder(names[i], 1,
                                ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.Default);
                        }

                        var previous = Time.realtimeSinceStartupAsDouble;
                        for (var frame = 0; frame < 600; frame++)
                        {
                            if (turnover)
                            {
                                particles.Advance(1d / 64);
                                if (particles.LiveCount != total)
                                {
                                    throw new System.InvalidOperationException("Turnover did not retain the matched live population.");
                                }
                            }

                            if (mode == "detach-reattach")
                            {
                                particles.RemoveFromHierarchy();
                                if (particles.LiveCount != 0)
                                {
                                    throw new System.InvalidOperationException("Detached particles retained live state.");
                                }

                                parent.Add(particles);
                            }

                            FrameTimingManager.CaptureFrameTimings();
                            yield return null;
                            var now = Time.realtimeSinceStartupAsDouble;
                            frames[frame, 0] = (now - previous) * 1000;
                            previous = now;
                            var available = FrameTimingManager.GetLatestTimings(1, timing) != 0;
                            frames[frame, 1] = available && timing[0].cpuFrameTime > 0 ? timing[0].cpuFrameTime : -1;
                            frames[frame, 2] = available && timing[0].gpuFrameTime > 0 ? timing[0].gpuFrameTime : -1;
                            frames[frame, 3] = particles.LiveCount;
                            for (var i = 0; i < names.Length; i++)
                            {
                                frames[frame, i + 4] = recorders[i].Valid ? recorders[i].LastValue : -1;
                            }
                        }

                        for (var i = 0; i < names.Length; i++)
                        {
                            recorders[i].Dispose();
                        }

                        for (var frame = 0; frame < 600; frame++)
                        {
                            csv.Write($"{mode},{run},{frame}");
                            for (var i = 0; i < names.Length + 4; i++)
                            {
                                csv.Write("," + frames[frame, i].ToString("R", CultureInfo.InvariantCulture));
                            }

                            csv.WriteLine();
                        }
                    }
                }
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                foreach (var recorder in recorders)
                {
                    recorder.Dispose();
                }

                particles.RemoveFromHierarchy();
                Object.Destroy(effect);
            }
        }
    }
}
