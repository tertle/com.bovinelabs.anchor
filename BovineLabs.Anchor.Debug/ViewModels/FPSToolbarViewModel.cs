// <copyright file="FPSToolbarViewModel.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Toolbar;
    using Unity.Collections;
    using Unity.Properties;
    using UnityEngine;

    public class FPSToolbarViewModel : BLObservableObject
    {
        private const int AvgFPSSamplesCapacity = 127;
        private const int TimeToResetMinMaxFPS = 10;

        private int currentFPS;
        private float frameTime;
        private int averageFPS;
        private int minFPS;
        private int maxFPS;

        private FPSStatistics fps;
        private float timeToTriggerUpdatesPassed;

        public FPSToolbarViewModel()
        {
            var averageFPSSamples = default(FixedList512Bytes<float>);
            averageFPSSamples.Length = AvgFPSSamplesCapacity;
            this.fps = new FPSStatistics { AverageFPSSamples = averageFPSSamples };
        }

        [CreateProperty]
        public int CurrentFPS
        {
            get => this.currentFPS;
            set
            {
                if (this.SetProperty(ref this.currentFPS, value))
                {
                    this.FrameTime = this.currentFPS == 0 ? 0 : 1000f / this.currentFPS;
                }
            }
        }

        [CreateProperty]
        public int AverageFPS
        {
            get => this.averageFPS;
            set => this.SetProperty(ref this.averageFPS, value);
        }

        [CreateProperty]
        public int MinFPS
        {
            get => this.minFPS;
            set => this.SetProperty(ref this.minFPS, value);
        }

        [CreateProperty]
        public int MaxFPS
        {
            get => this.maxFPS;
            set => this.SetProperty(ref this.maxFPS, value);
        }

        [CreateProperty]
        public float FrameTime
        {
            get => this.frameTime;
            private set => this.SetProperty(ref this.frameTime, value);
        }

        public void Update()
        {
            var unscaledDeltaTime = Time.unscaledDeltaTime;
            this.timeToTriggerUpdatesPassed += unscaledDeltaTime;

            this.CalculateStatistics(unscaledDeltaTime);

            if (this.timeToTriggerUpdatesPassed < ToolbarView.DefaultUpdateRate)
            {
                return;
            }

            this.timeToTriggerUpdatesPassed = 0;

            this.CurrentFPS = (int)this.fps.CurrentFPS;
            this.AverageFPS = (int)this.fps.AvgFPS;
            this.MinFPS = (int)this.fps.MinFPS;
            this.MaxFPS = (int)this.fps.MaxFPS;
        }

        private void CalculateStatistics(float unscaledDeltaTime)
        {
            this.fps.TimeToResetMinFPSPassed += unscaledDeltaTime;
            this.fps.TimeToResetMaxFPSPassed += unscaledDeltaTime;

            // Build FPS and ms
            this.fps.CurrentFPS = 1 / unscaledDeltaTime;

            // Build avg FPS
            this.fps.AvgFPS = 0;
            this.fps.AverageFPSSamples[this.fps.IndexSample++] = this.fps.CurrentFPS;

            if (this.fps.IndexSample == AvgFPSSamplesCapacity)
            {
                this.fps.IndexSample = 0;
            }

            if (this.fps.AvgFPSSamplesCount < AvgFPSSamplesCapacity)
            {
                this.fps.AvgFPSSamplesCount++;
            }

            for (var i = 0; i < this.fps.AvgFPSSamplesCount; i++)
            {
                this.fps.AvgFPS += this.fps.AverageFPSSamples[i];
            }

            this.fps.AvgFPS /= this.fps.AvgFPSSamplesCount;

            // Checks to reset min and max FPS
            if (this.fps.TimeToResetMinFPSPassed > TimeToResetMinMaxFPS)
            {
                this.fps.MinFPS = 0;
                this.fps.TimeToResetMinFPSPassed = 0;
            }

            if (this.fps.TimeToResetMaxFPSPassed > TimeToResetMinMaxFPS)
            {
                this.fps.MaxFPS = 0;
                this.fps.TimeToResetMaxFPSPassed = 0;
            }

            // Build min FPS
            if ((this.fps.CurrentFPS < this.fps.MinFPS) || (this.fps.MinFPS <= 0))
            {
                this.fps.MinFPS = this.fps.CurrentFPS;

                this.fps.TimeToResetMinFPSPassed = 0;
            }

            // Build max FPS
            if ((this.fps.CurrentFPS > this.fps.MaxFPS) || (this.fps.MaxFPS <= 0))
            {
                this.fps.MaxFPS = this.fps.CurrentFPS;

                this.fps.TimeToResetMaxFPSPassed = 0;
            }
        }

        private struct FPSStatistics
        {
            public float CurrentFPS;
            public float AvgFPS;
            public float MinFPS;
            public float MaxFPS;

            public FixedList512Bytes<float> AverageFPSSamples; // used as an array
            public int AvgFPSSamplesCount;
            public int IndexSample;

            public float TimeToResetMinFPSPassed;
            public float TimeToResetMaxFPSPassed;
        }
    }
}
