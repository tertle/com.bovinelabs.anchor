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
            var namedAction = ScriptableObject.CreateInstance<AnchorAction>();

            try
            {
                var field = typeof(AnchorAction).GetField("action", BindingFlags.Instance | BindingFlags.NonPublic);
                field!.SetValue(namedAction, null);

                var action = namedAction.Action;

                Assert.IsNotNull(action);
            }
            finally
            {
                Object.DestroyImmediate(namedAction);
            }
        }

    }
}
