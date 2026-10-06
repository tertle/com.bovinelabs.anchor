namespace BovineLabs.Anchor.Samples.Showcase
{
    using Unity.Entities;

    [InternalBufferCapacity(0)]
    public struct DashboardCell : IBufferElementData
    {
        public float Phase;
        public float Charge;
    }
}
