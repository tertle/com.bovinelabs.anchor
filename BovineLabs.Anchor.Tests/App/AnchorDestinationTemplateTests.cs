// <copyright file="AnchorDestinationTemplateTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using Unity.AppUI.Navigation;

    public class AnchorDestinationTemplateTests
    {
        [Test]
        public void Constructor_DisablesDefaultNavigationFlags()
        {
            var template = new TestableAnchorDestinationTemplate();
            template.SetTemplate("Invalid.Type, MissingAssembly");

            var screen = (NavigationScreen)template.CreateScreen(null);

            Assert.IsFalse(screen.showBottomNavBar);
            Assert.IsFalse(screen.showAppBar);
            Assert.IsFalse(screen.showBackButton);
            Assert.IsFalse(screen.showDrawer);
            Assert.IsFalse(screen.showNavigationRail);
        }

        [Test]
        public void InvalidTemplateType_FallsBackToDefaultScreen()
        {
            var template = new TestableAnchorDestinationTemplate();
            template.SetTemplate("Invalid.Type, MissingAssembly");

            var screen = template.CreateScreen(null);

            Assert.IsNotNull(screen);
            Assert.IsInstanceOf<NavigationScreen>(screen);
        }

        [Test]
        public void ValidTemplateType_ResolvesFromServices_AndAppliesFlags()
        {
            using var scope = new TestAnchorAppScope(static services =>
            {
                services.AddSingleton(typeof(TestDestinationScreen));
            });

            var template = new TestableAnchorDestinationTemplate();
            template.SetTemplate(typeof(TestDestinationScreen).AssemblyQualifiedName);
            template.SetVisibilityFlags(true);

            var screen = (NavigationScreen)template.CreateScreen(null);

            Assert.IsInstanceOf<TestDestinationScreen>(screen);
            Assert.IsTrue(screen.showBottomNavBar);
            Assert.IsTrue(screen.showAppBar);
            Assert.IsTrue(screen.showBackButton);
            Assert.IsTrue(screen.showDrawer);
            Assert.IsTrue(screen.showNavigationRail);
        }

        [Test]
        public void MissingServiceForValidType_ThrowsInvalidOperationException()
        {
            using var scope = new TestAnchorAppScope();

            var template = new TestableAnchorDestinationTemplate();
            template.SetTemplate(typeof(TestDestinationScreen).AssemblyQualifiedName);

            Assert.Throws<InvalidOperationException>(() => template.CreateScreen(null));
        }

        private sealed class TestableAnchorDestinationTemplate : AnchorDestinationTemplate
        {
            public void SetTemplate(string value)
            {
                this.template = value;
            }

            public void SetVisibilityFlags(bool value)
            {
                this.showBottomNavBar = value;
                this.showAppBar = value;
                this.showBackButton = value;
                this.showDrawer = value;
                this.showNavigationRail = value;
            }
        }
    }
}
