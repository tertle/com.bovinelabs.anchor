// <copyright file="NavSmallTypesTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using System.Reflection;
    using BovineLabs.Anchor.Nav;
    using NUnit.Framework;
    using UnityEngine;

    public class NavSmallTypesTests
    {
        [Test]
        public void AnchorNamedAction_Action_ReturnsDefaultWhenBackingFieldNull()
        {
            var namedAction = ScriptableObject.CreateInstance<AnchorNamedAction>();

            try
            {
                var field = typeof(AnchorNamedAction).GetField("action", BindingFlags.Instance | BindingFlags.NonPublic);
                field!.SetValue(namedAction, null);

                var action = namedAction.Action;

                Assert.IsNotNull(action);
            }
            finally
            {
                Object.DestroyImmediate(namedAction);
            }
        }

        [Test]
        public void AnchorNavActionAttribute_StoresName()
        {
            var attribute = new AnchorNavActionAttribute("action-name");

            Assert.AreEqual("action-name", attribute.Name);
        }

        [Test]
        public void AnchorNavActionAttribute_Whitespace_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => new AnchorNavActionAttribute(" "));
        }

        [Test]
        public void AnchorNavBackStackEntry_NullValues_Normalized()
        {
            var entry = new AnchorNavBackStackEntry("dest", null, null, null);

            Assert.IsNotNull(entry.Options);
            Assert.IsNotNull(entry.Arguments);
            Assert.AreEqual(0, entry.Arguments.Length);
            Assert.IsNotNull(entry.Snapshot);
            Assert.AreEqual(0, entry.Snapshot.Items.Count);
        }

        [Test]
        public void AnchorNavArgument_StringAndFromFactories_Match()
        {
            var fromString = AnchorNavArgument.String("name", "value");
            var fromFactory = AnchorNavArgument.From("name", "value");

            Assert.AreEqual(fromString, fromFactory);
            Assert.AreEqual("name", fromString.Name);
            Assert.AreEqual("value", fromString.Value);
        }

        [Test]
        public void AnchorNavArgument_Equality_UsesNameAndValue()
        {
            var first = new AnchorNavArgument("same", "value");
            var second = new AnchorNavArgument("same", "value");
            var differentName = new AnchorNavArgument("other", "value");
            var differentValue = new AnchorNavArgument("same", "other");

            Assert.IsTrue(first.Equals(second));
            Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
            Assert.IsFalse(first.Equals(differentName));
            Assert.IsFalse(first.Equals(differentValue));
            Assert.IsFalse(first.Equals(null));
        }

        [Test]
        public void AnimationDescription_None_UsesExpectedDefaults()
        {
            var none = AnimationDescription.None;

            Assert.AreEqual(0, none.DurationMs);
            Assert.IsNull(none.Callback);
            Assert.IsNotNull(none.Easing);
        }
    }
}
