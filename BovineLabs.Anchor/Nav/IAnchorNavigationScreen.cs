namespace BovineLabs.Anchor.Nav
{
    public interface IAnchorNavigationScreen
    {
        void OnEnter(AnchorNavArgument[] args);

        void OnExit(AnchorNavArgument[] args);
    }
}
