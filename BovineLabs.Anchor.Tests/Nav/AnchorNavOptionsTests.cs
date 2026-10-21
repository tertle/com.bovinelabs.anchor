namespace BovineLabs.Anchor.Tests.Nav
{
    using System.Collections.Generic;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Nav.Animations;
    using NUnit.Framework;
    using UnityEngine;

    public class AnchorNavOptionsTests
    {

        [Test]
        public void PopupBaseArguments_SetterCopiesInputCollection()
        {
            var arguments = new List<AnchorNavArgument>
            {
                AnchorNavArgument.String("x", "1"),
            };

            var options = new AnchorNavOptions
            {
                PopupBaseArguments = arguments,
            };

            arguments.Add(AnchorNavArgument.String("y", "2"));

            Assert.AreEqual(1, options.PopupBaseArguments.Count);
            Assert.AreEqual("x", options.PopupBaseArguments[0].Name);
        }

    }
}
