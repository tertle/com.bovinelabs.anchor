namespace BovineLabs.Anchor.Tests.TestDoubles
{
    using BovineLabs.Anchor.Nav;

    internal sealed class TestNavigationScreenReceiver : IAnchorNavigationScreen
    {
        public int EnterCount { get; private set; }

        public int ExitCount { get; private set; }

        public AnchorNavArgument[] LastEnterArguments { get; private set; }

        public AnchorNavArgument[] LastExitArguments { get; private set; }

        public void OnEnter(AnchorNavArgument[] args)
        {
            EnterCount++;
            LastEnterArguments = args;
        }

        public void OnExit(AnchorNavArgument[] args)
        {
            ExitCount++;
            LastExitArguments = args;
        }
    }
}
