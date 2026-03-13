// <copyright file="AnchorNavOptionsTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

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
        public void Clone_DeepCopiesPopupArgumentsAndAnimations()
        {
            var enterAnimation = ScriptableObject.CreateInstance<FadeInAnimation>();

            try
            {
                var options = new AnchorNavOptions
                {
                    PopupBaseDestination = "Base",
                    PopupBaseArguments = new List<AnchorNavArgument> { AnchorNavArgument.String("name", "value") },
                    Animations = new AnchorAnimations
                    {
                        EnterAnim = enterAnimation,
                    },
                };

                var clone = options.Clone();

                Assert.AreNotSame(options.PopupBaseArguments, clone.PopupBaseArguments);
                Assert.AreNotSame(options.Animations, clone.Animations);
                Assert.AreEqual(options.PopupBaseArguments.Count, clone.PopupBaseArguments.Count);
                Assert.AreEqual(options.PopupBaseArguments[0], clone.PopupBaseArguments[0]);
                Assert.AreSame(enterAnimation, clone.Animations.EnterAnim);
            }
            finally
            {
                Object.DestroyImmediate(enterAnimation);
            }
        }

        [Test]
        public void Clone_MutationsOnOriginal_DoNotAffectClone()
        {
            var enterAnimation = ScriptableObject.CreateInstance<FadeInAnimation>();
            var replacementEnterAnimation = ScriptableObject.CreateInstance<FadeOutAnimation>();

            try
            {
                var options = new AnchorNavOptions
                {
                    PopupBaseArguments = new List<AnchorNavArgument> { AnchorNavArgument.String("a", "1") },
                    Animations = new AnchorAnimations { EnterAnim = enterAnimation },
                };

                var clone = options.Clone();

                options.PopupBaseArguments.Add(AnchorNavArgument.String("b", "2"));
                options.Animations.EnterAnim = replacementEnterAnimation;

                Assert.AreEqual(1, clone.PopupBaseArguments.Count);
                Assert.AreEqual("a", clone.PopupBaseArguments[0].Name);
                Assert.AreSame(enterAnimation, clone.Animations.EnterAnim);
            }
            finally
            {
                Object.DestroyImmediate(enterAnimation);
                Object.DestroyImmediate(replacementEnterAnimation);
            }
        }

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
