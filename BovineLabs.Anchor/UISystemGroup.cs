namespace BovineLabs.Anchor
{
    using BovineLabs.Core;
    using Unity.Entities;

    /// <summary>
    /// Presentation system group used to run UI-related systems after the main simulation.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation | Worlds.Menu, WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UISystemGroup : ComponentSystemGroup
    {
    }
}
