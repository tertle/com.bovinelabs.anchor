namespace BovineLabs.Anchor
{
    using BovineLabs.Core;
    using Unity.Entities;

    [WorldSystemFilter(WorldSystemFilterFlags.Presentation | Worlds.Menu, WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UISystemGroup : ComponentSystemGroup
    {
    }
}
