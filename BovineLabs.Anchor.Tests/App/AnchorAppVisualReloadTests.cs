// <copyright file="AnchorAppVisualReloadTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System.Reflection;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class AnchorAppVisualReloadTests
    {
        [Test]
        public void Initialize_WhenRestoringNavigationState_DoesNotEnterStartDestination()
        {
            const string startDestination = "start";
            var field = typeof(AnchorSettings).GetField("startDestination", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field);

            var settings = AnchorSettings.I;
            var originalStartDestination = (string)field!.GetValue(settings);
            field.SetValue(settings, startDestination);

            try
            {
                using var scope = new TestAnchorAppScope(static services =>
                {
                    services.AddSingleton(typeof(TestVisualElementFactory));
                    services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
                });

                var factory = scope.ServiceProvider.GetRequiredService<TestVisualElementFactory>();
                var receiver = new TestNavigationScreenReceiver();
                factory.Register(startDestination, () =>
                {
                    var root = new VisualElement();
                    root.Add(new VisualElement { dataSource = receiver });
                    return root;
                });

                scope.App.RestoringNavigationState = true;
                scope.App.Initialize();

                Assert.IsNull(scope.App.NavHost.CurrentDestination);
                Assert.AreEqual(0, receiver.EnterCount);
            }
            finally
            {
                field.SetValue(settings, originalStartDestination);
            }
        }
    }
}
