namespace BovineLabs.Anchor.Samples.Showcase
{
    using BovineLabs.Anchor.MVVM;
    using Unity.Properties;
    using UnityEngine;
    using UnityEngine.Scripting;

    [Preserve]
    [IsService]
    public partial class BindingSampleViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial float Charge
        {
            get;
            set;
        } = 0.45f;

        [ObservableProperty]
        public partial bool Armed
        {
            get;
            set;
        } = true;

        [ObservableProperty]
        public partial int Launches
        {
            get;
            set;
        }

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Charge))]
        public string ChargeText => $"{Charge:P0} charge";

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Charge))]
        public float BufferValue => Mathf.Min(1f, Charge + 0.2f);

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Charge), nameof(Armed))]
        public bool CanLaunch => Armed && Charge >= 0.25f;

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(CanLaunch), nameof(Armed))]
        public string ReadinessText => CanLaunch ? "READY TO LAUNCH" : Armed ? "RECHARGE TO LAUNCH" : "SYSTEM DISARMED";

        [CreateProperty(ReadOnly = true)]
        [DependsOn(nameof(Launches))]
        public string LaunchText => $"{Launches} successful launches";

        [ICommand(CanExecuteProperty = nameof(CanLaunch))]
        private void Launch()
        {
            Charge -= 0.25f;
            Launches++;
        }

        [ICommand]
        private void Recharge()
        {
            Charge = 1f;
        }

        [ICommand]
        private void Drain()
        {
            Charge = 0f;
        }

        [ICommand]
        private void Reset()
        {
            Charge = 0.45f;
            Armed = true;
            Launches = 0;
        }
    }
}
