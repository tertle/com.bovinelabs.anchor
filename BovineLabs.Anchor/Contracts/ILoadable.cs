namespace BovineLabs.Anchor
{
    /// <summary>
    /// Contract for view models that require an explicit Load and Unload.
    /// </summary>
    public interface ILoadable
    {
        void Load();

        void Unload();
    }
}
