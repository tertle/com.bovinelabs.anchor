namespace BovineLabs.Anchor.Debug.ViewModels
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using BovineLabs.Anchor.Debug.Views;
    using BovineLabs.Anchor.MVVM;
    using Unity.Collections;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    [IsService]
    [AutoToolbar("FPS")]
    public partial class FPSToolbarViewModel : ObservableObject, IToolbarElement
    {
        private const int AvgFPSSamplesCapacity = 127;
        private const int TimeToResetMinMaxFPS = 10;

        private int _currentFPS;

        private int _averageFPS;

        private int _minFPS;

        private int _maxFPS;

        private FPSStatistics _fps;
        private float _timeToTriggerUpdatesPassed;

        public FPSToolbarViewModel()
        {
            var averageFPSSamples = default(FixedList512Bytes<float>);
            averageFPSSamples.Length = AvgFPSSamplesCapacity;
            _fps = new FPSStatistics { AverageFPSSamples = averageFPSSamples };
        }

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(CurrentFPS))]
        public float FrameTime => _currentFPS == 0 ? 0 : 1000f / _currentFPS;

        [CreateProperty]
        public int CurrentFPS
        {
            get => _currentFPS;
            set => SetProperty(ref _currentFPS, value);
        }

        [CreateProperty]
        public int AverageFPS
        {
            get => _averageFPS;
            set => SetProperty(ref _averageFPS, value);
        }

        [CreateProperty]
        public int MinFPS
        {
            get => _minFPS;
            set => SetProperty(ref _minFPS, value);
        }

        [CreateProperty]
        public int MaxFPS
        {
            get => _maxFPS;
            set => SetProperty(ref _maxFPS, value);
        }

        public VisualElement CreateElement()
        {
            return new FPSToolbarView(this);
        }

        public void Update()
        {
            var unscaledDeltaTime = Time.unscaledDeltaTime;
            _timeToTriggerUpdatesPassed += unscaledDeltaTime;

            CalculateStatistics(unscaledDeltaTime);

            if (_timeToTriggerUpdatesPassed < Toolbar.UpdateRateSeconds)
            {
                return;
            }

            _timeToTriggerUpdatesPassed = 0;

            CurrentFPS = (int)_fps.CurrentFPS;
            AverageFPS = (int)_fps.AvgFPS;
            MinFPS = (int)_fps.MinFPS;
            MaxFPS = (int)_fps.MaxFPS;
        }

        private void CalculateStatistics(float unscaledDeltaTime)
        {
            _fps.TimeToResetMinFPSPassed += unscaledDeltaTime;
            _fps.TimeToResetMaxFPSPassed += unscaledDeltaTime;

            // Build FPS and ms
            _fps.CurrentFPS = 1 / unscaledDeltaTime;

            // Build avg FPS
            _fps.AvgFPS = 0;
            _fps.AverageFPSSamples[_fps.IndexSample++] = _fps.CurrentFPS;

            if (_fps.IndexSample == AvgFPSSamplesCapacity)
            {
                _fps.IndexSample = 0;
            }

            if (_fps.AvgFPSSamplesCount < AvgFPSSamplesCapacity)
            {
                _fps.AvgFPSSamplesCount++;
            }

            for (var i = 0; i < _fps.AvgFPSSamplesCount; i++)
            {
                _fps.AvgFPS += _fps.AverageFPSSamples[i];
            }

            _fps.AvgFPS /= _fps.AvgFPSSamplesCount;

            // Checks to reset min and max FPS
            if (_fps.TimeToResetMinFPSPassed > TimeToResetMinMaxFPS)
            {
                _fps.MinFPS = 0;
                _fps.TimeToResetMinFPSPassed = 0;
            }

            if (_fps.TimeToResetMaxFPSPassed > TimeToResetMinMaxFPS)
            {
                _fps.MaxFPS = 0;
                _fps.TimeToResetMaxFPSPassed = 0;
            }

            // Build min FPS
            if (_fps.CurrentFPS < _fps.MinFPS || _fps.MinFPS <= 0)
            {
                _fps.MinFPS = _fps.CurrentFPS;

                _fps.TimeToResetMinFPSPassed = 0;
            }

            // Build max FPS
            if (_fps.CurrentFPS > _fps.MaxFPS || _fps.MaxFPS <= 0)
            {
                _fps.MaxFPS = _fps.CurrentFPS;

                _fps.TimeToResetMaxFPSPassed = 0;
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
