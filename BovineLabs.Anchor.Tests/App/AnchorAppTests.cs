// <copyright file="AnchorAppTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Reflection;
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.DependencyInjection;
    using BovineLabs.Anchor.Services;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using BovineLabs.Anchor.Toolbar;
    using BovineLabs.Core.Utility;
    using NUnit.Framework;
    using UnityEngine.UIElements;

    public class AnchorAppTests
    {
        [Test]
        public void ServiceProvider_SingletonInstanceAndAlias_ReturnsSameResolvedInstance()
        {
            var shared = new SharedSettings();
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(typeof(SharedSettings), shared);
            services.AddAlias(typeof(IFooSettings), typeof(SharedSettings));
            services.AddAlias(typeof(IBarSettings), typeof(SharedSettings));

            using var provider = services.BuildServiceProvider();

            Assert.AreSame(shared, provider.GetRequiredService<SharedSettings>());
            Assert.AreSame(shared, provider.GetRequiredService<IFooSettings>());
            Assert.AreSame(shared, provider.GetRequiredService<IBarSettings>());
        }

        [Test]
        public void Initialize_CreatesNavHostAndResolvesContainers()
        {
            using var scope = new TestAnchorAppScope(static services =>
            {
                RegisterDefaultAnchorServices(services);
            });

            var app = scope.App;
            var previousStartDestination = GetField<string>(AnchorSettings.I, "startDestination");
            var popup = new VisualElement { name = "popup-container" };
            var notifications = new VisualElement { name = "notification-container" };
            var tooltip = new VisualElement { name = "tooltip-container" };
            app.RootVisualElement.Add(popup);
            app.RootVisualElement.Add(notifications);
            app.RootVisualElement.Add(tooltip);

            try
            {
                SetField(AnchorSettings.I, "startDestination", string.Empty);
                app.Initialize();
            }
            finally
            {
                SetField(AnchorSettings.I, "startDestination", previousStartDestination);
            }

            Assert.IsNotNull(app.NavHost);
            Assert.AreSame(popup, app.PopupContainer);
            Assert.AreSame(notifications, app.NotificationContainer);
            Assert.AreSame(tooltip, app.TooltipContainer);
        }

        [Test]
        public void Initialize_WithStartDestination_NavigatesToStart()
        {
            using var scope = new TestAnchorAppScope(static services =>
            {
                RegisterDefaultAnchorServices(services);
            });

            var app = scope.App;
            var previousStartDestination = GetField<string>(AnchorSettings.I, "startDestination");

            try
            {
                SetField(AnchorSettings.I, "startDestination", "start");
                Assert.AreSame(app, AnchorApp.Current);
                Assert.IsNotNull(app.Services.GetService<IUXMLService>());
                app.Initialize();
                Assert.AreEqual("start", app.NavHost.CurrentDestination);
            }
            finally
            {
                SetField(AnchorSettings.I, "startDestination", previousStartDestination);
            }
        }

        private static void RegisterDefaultAnchorServices(AnchorServiceCollection services)
        {
            services.AddSingleton(typeof(TestVisualElementFactory));
            services.AddSingleton(typeof(IUXMLService), typeof(TestUxmlService));
            services.AddSingleton(typeof(ILocalStorageService), typeof(TestLocalStorageService));
            services.AddSingleton(typeof(IViewModelService), typeof(ViewModelService));
            services.AddSingleton(typeof(ToolbarViewModel));
            services.AddSingleton(typeof(ToolbarView));

            foreach (var serviceType in ReflectionUtility.GetAllWithAttribute<IsServiceAttribute>())
            {
                if (serviceType.GetCustomAttribute<TransientAttribute>() != null)
                {
                    services.AddTransient(serviceType);
                }
                else
                {
                    services.AddSingleton(serviceType);
                }
            }
        }

        private static T GetField<T>(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(instance.GetType().FullName, fieldName);
            }

            return (T)field.GetValue(instance);
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

        private interface IFooSettings
        {
        }

        private interface IBarSettings
        {
        }

        private sealed class SharedSettings : IFooSettings, IBarSettings
        {
        }
    }
}

