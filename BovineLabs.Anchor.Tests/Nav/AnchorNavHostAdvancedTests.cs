// <copyright file="AnchorNavHostAdvancedTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Nav
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Nav;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.TestTools;
    using UnityEngine.UIElements;
    using Object = UnityEngine.Object;

    public class AnchorNavHostAdvancedTests
    {
        [Test]
        public void Navigate_ActionName_InvokesActionTriggeredAndMergesArguments()
        {
            var namedAction = ScriptableObject.CreateInstance<AnchorNamedAction>();

            try
            {
                SetField(namedAction, "actionName", "go-popup");
                SetField(
                    namedAction,
                    "action",
                        new AnchorNavAction(
                        "popup",
                        new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent },
                        new[]
                        {
                            AnchorNavArgument.String("default", "one"),
                            AnchorNavArgument.String("override", "before"),
                        }));

                using var context = new HostContext(new[] { namedAction }, null);
                context.RegisterScreen("base");
                var popupReceiver = context.RegisterScreen("popup");

                context.Host.Navigate("base");

                var actionTriggered = 0;
                context.Host.ActionTriggered += (_, _) => actionTriggered++;

                var navigated = context.Host.Navigate(
                    "go-popup",
                    AnchorNavArgument.String("override", "after"),
                    AnchorNavArgument.String("extra", "two"));

                Assert.IsTrue(navigated);
                Assert.AreEqual(1, actionTriggered);
                Assert.AreEqual("popup", context.Host.CurrentDestination);
                Assert.IsTrue(context.Host.HasActivePopups);
                Assert.AreEqual(1, popupReceiver.EnterCount);
                CollectionAssert.AreEquivalent(
                    new[]
                    {
                        AnchorNavArgument.String("default", "one"),
                        AnchorNavArgument.String("override", "after"),
                        AnchorNavArgument.String("extra", "two"),
                    },
                    popupReceiver.LastEnterArguments);
            }
            finally
            {
                Object.DestroyImmediate(namedAction);
            }
        }

        [Test]
        public void PopupExistingStrategy_CloseOtherPopups_ReplacesPopupLayer()
        {
            using var context = new HostContext();
            context.RegisterScreen("base");
            context.RegisterScreen("popup-a");
            context.RegisterScreen("popup-b");

            context.Host.Navigate("base");
            context.Host.Navigate("popup-a", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            context.Host.Navigate(
                "popup-b",
                new AnchorNavOptions
                {
                    PopupStrategy = AnchorPopupStrategy.PopupOnCurrent,
                    PopupExistingStrategy = AnchorPopupExistingStrategy.CloseOtherPopups,
                });

            Assert.AreEqual("popup-b", context.Host.CurrentDestination);
            Assert.IsTrue(context.Host.HasActivePopups);
            Assert.IsTrue(context.Host.ClosePopup("popup-b"));
            Assert.AreEqual("base", context.Host.CurrentDestination);
            Assert.IsFalse(context.Host.HasActivePopups);
        }

        [Test]
        public void PopupExistingStrategy_PushNew_RestoresPreviousPopupSnapshot()
        {
            using var context = new HostContext();
            context.RegisterScreen("base");
            context.RegisterScreen("popup-a");
            context.RegisterScreen("popup-b");

            context.Host.Navigate("base");
            context.Host.Navigate("popup-a", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            context.Host.Navigate(
                "popup-b",
                new AnchorNavOptions
                {
                    PopupStrategy = AnchorPopupStrategy.PopupOnCurrent,
                    PopupExistingStrategy = AnchorPopupExistingStrategy.PushNew,
                });

            Assert.AreEqual("popup-b", context.Host.CurrentDestination);

            Assert.IsTrue(context.Host.PopBackStack());
            Assert.AreEqual("popup-a", context.Host.CurrentDestination);
            Assert.IsTrue(context.Host.HasActivePopups);
        }

        [Test]
        public void EnsureBaseAndPopup_EmptyBaseDestination_ReturnsFalse()
        {
            using var context = new HostContext();
            context.RegisterScreen("popup");
            LogAssert.Expect(LogType.Error, new Regex("requires a base destination"));

            var result = context.Host.Navigate(
                "popup",
                new AnchorNavOptions
                {
                    PopupStrategy = AnchorPopupStrategy.EnsureBaseAndPopup,
                    PopupBaseDestination = string.Empty,
                });

            Assert.IsFalse(result);
            Assert.IsNull(context.Host.CurrentDestination);
        }

        [Test]
        public void EnsureBaseAndPopup_MisalignedBase_NavigatesToBaseThenPopup()
        {
            using var context = new HostContext();
            context.RegisterScreen("base-a");
            context.RegisterScreen("base-b");
            context.RegisterScreen("popup");

            context.Host.Navigate("base-a");

            var result = context.Host.Navigate(
                "popup",
                new AnchorNavOptions
                {
                    PopupStrategy = AnchorPopupStrategy.EnsureBaseAndPopup,
                    PopupBaseDestination = "base-b",
                    PopupBaseArguments = new[] { AnchorNavArgument.String("seed", "1") },
                },
                AnchorNavArgument.String("popup", "2"));

            Assert.IsTrue(result);
            Assert.AreEqual("popup", context.Host.CurrentDestination);
            Assert.IsTrue(context.Host.ClosePopup("popup"));
            Assert.AreEqual("base-b", context.Host.CurrentDestination);
        }

        [Test]
        public void Navigate_SamePopupDestination_UpdatesArgumentsWithoutDuplicatePopupLayer()
        {
            using var context = new HostContext();
            context.RegisterScreen("base");
            var popupReceiver = context.RegisterScreen("popup");

            context.Host.Navigate("base");
            context.Host.Navigate(
                "popup",
                new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent },
                AnchorNavArgument.String("a", "1"));

            context.Host.Navigate(
                "popup",
                new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent },
                AnchorNavArgument.String("a", "2"));

            Assert.AreEqual(2, popupReceiver.EnterCount);
            Assert.AreEqual(AnchorNavArgument.String("a", "2"), popupReceiver.LastEnterArguments[0]);
            Assert.IsTrue(context.Host.ClosePopup("popup"));
            Assert.IsFalse(context.Host.ClosePopup("popup"));
            Assert.AreEqual("base", context.Host.CurrentDestination);
        }

        [Test]
        public void PopBackStack_RestoresSnapshotWithPopups()
        {
            using var context = new HostContext();
            context.RegisterScreen("base");
            context.RegisterScreen("popup");
            context.RegisterScreen("panel");

            context.Host.Navigate("base");
            context.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });
            context.Host.Navigate("panel");

            Assert.IsTrue(context.Host.PopBackStack());
            Assert.AreEqual("popup", context.Host.CurrentDestination);
            Assert.IsTrue(context.Host.HasActivePopups);
        }

        [Test]
        public void PopBackStackToPanel_StripsPopupLayersFromPoppedSnapshot()
        {
            using var context = new HostContext();
            context.RegisterScreen("base");
            context.RegisterScreen("popup");
            context.RegisterScreen("panel");

            context.Host.Navigate("base");
            context.Host.Navigate("popup", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });
            context.Host.Navigate("panel");

            Assert.IsTrue(context.Host.PopBackStackToPanel());
            Assert.AreEqual("popup", context.Host.CurrentDestination);
            Assert.IsFalse(context.Host.HasActivePopups);
        }

        [Test]
        public void ClearNavigation_ValidAnimationId_DoesNotThrow()
        {
            var animation = ScriptableObject.CreateInstance<TestAnchorNavAnimation>();

            try
            {
                ((BovineLabs.Core.ObjectManagement.IUID)animation).ID = 73;

                using var context = new HostContext(null, new[] { animation });
                context.RegisterScreen("base");

                context.Host.Navigate("base");

                Assert.DoesNotThrow(() => context.Host.ClearNavigation(73));
                Assert.IsNull(context.Host.CurrentDestination);
                Assert.IsFalse(context.Host.CanGoBack);
            }
            finally
            {
                Object.DestroyImmediate(animation);
            }
        }

        [Test]
        public void CloseAllPopups_TrimsEquivalentBackStackSnapshots()
        {
            using var context = new HostContext();
            context.RegisterScreen("base");
            context.RegisterScreen("popup-a");
            context.RegisterScreen("popup-b");

            context.Host.Navigate("base");
            context.Host.Navigate("popup-a", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });
            context.Host.Navigate(
                "popup-b",
                new AnchorNavOptions
                {
                    PopupStrategy = AnchorPopupStrategy.PopupOnCurrent,
                    PopupExistingStrategy = AnchorPopupExistingStrategy.CloseOtherPopups,
                });

            Assert.IsTrue(context.Host.CanGoBack);
            Assert.IsTrue(context.Host.CloseAllPopups());
            Assert.AreEqual("base", context.Host.CurrentDestination);
            Assert.IsFalse(context.Host.HasActivePopups);
            Assert.IsFalse(context.Host.CanGoBack);
        }

        [Test]
        public void ClosePopup_OnlySearchesPopupSegment()
        {
            using var context = new HostContext();
            context.RegisterScreen("base");
            context.RegisterScreen("popup-base");
            context.RegisterScreen("panel");
            context.RegisterScreen("popup-top");

            context.Host.Navigate("base");
            context.Host.Navigate("popup-base", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });
            context.Host.Navigate("panel");
            context.Host.Navigate("popup-top", new AnchorNavOptions { PopupStrategy = AnchorPopupStrategy.PopupOnCurrent });

            Assert.IsFalse(context.Host.ClosePopup("popup-base"));
            Assert.AreEqual("popup-top", context.Host.CurrentDestination);

            Assert.IsTrue(context.Host.ClosePopup("popup-top"));
            Assert.AreEqual("panel", context.Host.CurrentDestination);
            Assert.IsFalse(context.Host.HasActivePopups);
        }

        [Test]
        public void SharedPrefixArgumentsUpdate_ReentersCurrentDestination()
        {
            using var context = new HostContext();
            var receiver = context.RegisterScreen("panel");

            context.Host.Navigate("panel", new AnchorNavOptions(), AnchorNavArgument.String("x", "1"));
            context.Host.Navigate("panel", new AnchorNavOptions(), AnchorNavArgument.String("x", "2"));

            Assert.AreEqual(2, receiver.EnterCount);
            Assert.AreEqual(0, receiver.ExitCount);
            Assert.AreEqual(AnchorNavArgument.String("x", "2"), receiver.LastEnterArguments[0]);
        }

        private static void SetField(object instance, string fieldName, object value)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, fieldName);
            }

            field.SetValue(instance, value);
        }

        private sealed class TestAnchorNavAnimation : AnchorNavAnimation
        {
            protected override Func<float, float> EasingFunction => static f => f;

            protected override Action<VisualElement, float> Callback => null;
        }

        private sealed class HostContext : IDisposable
        {
            private readonly TestAnchorAppScope scope;

            public HostContext(IEnumerable<AnchorNamedAction> actions = null, IEnumerable<AnchorNavAnimation> animations = null)
            {
                this.scope = new TestAnchorAppScope(static services =>
                {
                    services.AddSingleton(typeof(TestVisualElementFactory));
                    services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
                });

                this.Factory = this.scope.ServiceProvider.GetRequiredService<TestVisualElementFactory>();
                this.Host = new AnchorNavHost(actions, animations);
                this.scope.App.NavHost = this.Host;
            }

            public AnchorNavHost Host { get; }

            private TestVisualElementFactory Factory { get; }

            public TestNavigationScreenReceiver RegisterScreen(string destination)
            {
                var receiver = new TestNavigationScreenReceiver();

                this.Factory.Register(destination, () =>
                {
                    var root = new VisualElement { name = destination };
                    root.Add(new VisualElement { dataSource = receiver });
                    return root;
                });

                return receiver;
            }

            public void Dispose()
            {
                this.scope.Dispose();
            }
        }
    }
}
