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
            File.WriteAllText(Path.Combine(directory, "environment.txt"),
                $"Unity {Application.unityVersion}\n{SystemInfo.graphicsDeviceType}\n{SystemInfo.processorType}\n" +
                $"{SystemInfo.graphicsDeviceName}\n{Screen.width}x{Screen.height}\n1000 particles / 32 emitters / seed 7\n");
            var effect = ScriptableObject.CreateInstance<UIParticleEffect>();
            effect.Emitters.Clear();
            for (var i = 0; i < 32; i++)
            {
                var count = i < 8 ? 32 : 31;
                effect.Emitters.Add(new UIParticleEmitterSettings
                {
                    MaxParticles = count,
                    Offset = new Vector2(50 + ((i % 8) * 110), 50 + ((i / 8) * 100)),
                    Lifetime = new Vector2(1000, 1000),
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
            var parent = new VisualElement();
            surface.Add(parent);
            var particles = new AnchorParticles
            {
                Effect = effect,
                PlayOnAttach = true,
                Seed = 7,
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
            using var csv = new StreamWriter(Path.Combine(directory, "runtime-frames.csv"));
            csv.WriteLine("mode,run,frame,frameMs,cpuMs,gpuMs,live," + string.Join(",", names));
            try
            {
                foreach (var mode in new[] { "active", "hidden-watch", "paused", "idle", "detach-reattach" })
                {
                    for (var run = 0; run < 5; run++)
                    {
                        parent.style.display = DisplayStyle.Flex;
                        particles.Play();
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
                            yield return null;
                        }

                        for (var i = 0; i < names.Length; i++)
                        {
                            recorders[i] = new ProfilerRecorder(names[i], 1,
                                ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.SumAllSamplesInFrame);
                        }

                        var previous = Time.realtimeSinceStartupAsDouble;
                        for (var frame = 0; frame < 600; frame++)
                        {
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
                            frames[frame, 1] = available ? timing[0].cpuFrameTime : -1;
                            frames[frame, 2] = available ? timing[0].gpuFrameTime : -1;
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
                foreach (var recorder in recorders)
                {
                    recorder.Dispose();
                }

                particles.RemoveFromHierarchy();
                Object.Destroy(effect);
            }

            File.WriteAllText(Path.Combine(directory, "runtime-complete.txt"), "Completed all five runtime workloads, five runs each.");
        }
    }
}
