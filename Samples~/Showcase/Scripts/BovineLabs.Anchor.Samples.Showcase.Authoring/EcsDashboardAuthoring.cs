namespace BovineLabs.Anchor.Samples.Showcase.Authoring
{
    using Unity.Entities;
    using Unity.Mathematics;
    using UnityEngine;

    public sealed class EcsDashboardAuthoring : MonoBehaviour
    {
        [SerializeField, Min(1)]
        private int _cellCount = 512;

        private sealed class DashboardBaker : Baker<EcsDashboardAuthoring>
        {
            public override void Bake(EcsDashboardAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<DashboardState>(entity);
                var cells = AddBuffer<DashboardCell>(entity);
                for (var index = 0; index < authoring._cellCount; index++)
                {
                    var phase = index * 0.013f;
                    cells.Add(new DashboardCell
                    {
                        Phase = phase,
                        Charge = (math.sin(phase) + 1) * 0.5f,
                    });
                }
            }
        }
    }
}
