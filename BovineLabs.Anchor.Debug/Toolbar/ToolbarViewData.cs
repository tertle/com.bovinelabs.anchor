namespace BovineLabs.Anchor.Debug.Toolbar
{
    using Unity.Burst;
    using Unity.Collections;

    /// <summary>
    /// Kept separate from managed toolbar types so Burst does not compile their static variables.
    /// </summary>
    public static class ToolbarViewData
    {
        public static readonly SharedStatic<FixedString32Bytes> ActiveTab = SharedStatic<FixedString32Bytes>.GetOrCreate<ActiveTabVar>();

        private struct ActiveTabVar
        {
        }
    }
}
