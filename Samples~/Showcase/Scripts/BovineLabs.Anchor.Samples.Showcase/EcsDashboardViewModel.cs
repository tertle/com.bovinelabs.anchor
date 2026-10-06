namespace BovineLabs.Anchor.Samples.Showcase
{
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine.Scripting;

    [Preserve]
    public partial class EcsDashboardViewModel : SystemObservableObject<EcsDashboardViewModel.Data>, ILoadable
    {
        [CreateProperty(ReadOnly = true)]
        public int EntityCount => Value.EntityCount;

        [CreateProperty(ReadOnly = true)]
        public int ChargedCount => Value.ChargedCount;

        [CreateProperty(ReadOnly = true)]
        public float AverageCharge => Value.AverageCharge;

        [CreateProperty(ReadOnly = true)]
        public float MinimumCharge => Value.MinimumCharge;

        [CreateProperty(ReadOnly = true)]
        public float MaximumCharge => Value.MaximumCharge;

        [CreateProperty(ReadOnly = true)]
        public float ProbeCharge => Value.ProbeCharge;

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(ProbeCharge))]
        public string ProbeChargeText => $"Cell 01 · {ProbeCharge:P0} charge";

        [CreateProperty(ReadOnly = true)]
        public int Tick => Value.Tick;

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Tick))]
        public string TickText => $"Burst updates: {Value.Tick:N0}";

        [CreateProperty]
        public float Speed
        {
            get => Value.Speed;
            set => SetProperty(ref Value.Speed, value);
        }

        [CreateProperty]
        public bool Paused
        {
            get => Value.Paused;
            set => SetProperty(ref Value.Paused, value);
        }

        [ICommand]
        private void Reset() => Value.ResetRequested = true;

        [ICommand]
        private void Pulse() => Value.PulseRequested = true;

        public void Load()
        {
            Speed = 1;
            Paused = false;
            Value.ResetRequested = false;
            Value.PulseRequested = false;
        }

        public void Unload() { }

        public partial struct Data
        {
            [SystemProperty]
            private int _entityCount;

            [SystemProperty]
            private int _chargedCount;

            [SystemProperty]
            private float _averageCharge;

            [SystemProperty]
            private float _minimumCharge;

            [SystemProperty]
            private float _maximumCharge;

            [SystemProperty]
            private float _probeCharge;

            [SystemProperty]
            private int _tick;

            public float Speed;
            public bool Paused;
            public Changed<bool> ResetRequested;
            public Changed<bool> PulseRequested;
        }
    }
}
