namespace BovineLabs.Anchor
{
    using BovineLabs.Core;
    using Unity.Entities;

    /// <summary>
    /// Updates ECS-driven user interfaces during presentation.
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Presentation | Worlds.Menu, WorldSystemFilterFlags.Presentation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class UISystemGroup : ComponentSystemGroup
    {
    }
}
