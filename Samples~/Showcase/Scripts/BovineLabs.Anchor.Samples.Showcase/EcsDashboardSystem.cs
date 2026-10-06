namespace BovineLabs.Anchor.Samples.Showcase
{
    using Unity.Burst;
    using Unity.Entities;
    using Unity.Mathematics;

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct EcsDashboardSystem : ISystem, ISystemStartStop
    {
        private UIHelper<EcsDashboardViewModel, EcsDashboardViewModel.Data> _ui;

        public void OnCreate(ref SystemState state) => state.RequireForUpdate<DashboardState>();

        public void OnStartRunning(ref SystemState state) => _ui.Bind();

        public void OnStopRunning(ref SystemState state) => _ui.Unbind();

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref var data = ref _ui.Binding;
            var reset = data.ResetRequested.HasChanged && data.ResetRequested.Value;
            var pulse = data.PulseRequested.HasChanged && data.PulseRequested.Value;
            data.ResetRequested = default;
            data.PulseRequested = default;
            var delta = data.Paused ? 0 : SystemAPI.Time.DeltaTime * data.Speed;
            // UIHelper shares pinned view-model data with UI bindings; keep this small simulation and publication on the main thread.
            foreach (var (dashboard, sourceCells) in SystemAPI.Query<RefRW<DashboardState>, DynamicBuffer<DashboardCell>>())
            {
                var cells = sourceCells;
                var sum = 0f;
                var minimum = 1f;
                var maximum = 0f;
                var charged = 0;
                for (var index = 0; index < cells.Length; index++)
                {
                    var cell = cells[index];
                    if (reset)
                    {
                        cell.Phase = index * 0.013f;
                    }

                    cell.Phase += delta * (0.4f + index % 11 * 0.08f);
                    if (pulse)
                    {
                        cell.Phase = math.PI * 0.5f;
                    }

                    cell.Charge = (math.sin(cell.Phase) + 1) * 0.5f;
                    cells[index] = cell;
                    sum += cell.Charge;
                    minimum = math.min(minimum, cell.Charge);
                    maximum = math.max(maximum, cell.Charge);
                    charged += cell.Charge >= 0.75f ? 1 : 0;
                }

                if (!data.Paused || reset || pulse)
                {
                    dashboard.ValueRW.Tick++;
                }

                data.EntityCount = cells.Length;
                data.ChargedCount = charged;
                data.AverageCharge = sum / cells.Length;
                data.MinimumCharge = minimum;
                data.MaximumCharge = maximum;
                data.ProbeCharge = cells[0].Charge;
                data.Tick = dashboard.ValueRO.Tick;
            }
        }
    }
}
