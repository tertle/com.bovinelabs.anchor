// <copyright file="TestNavigationScreenReceiver.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

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
            this.EnterCount++;
            this.LastEnterArguments = args;
        }

        public void OnExit(AnchorNavArgument[] args)
        {
            this.ExitCount++;
            this.LastExitArguments = args;
        }
    }
}
