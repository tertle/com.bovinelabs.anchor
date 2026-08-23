// <copyright file="AnchorAppTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Tests.TestDoubles;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.UIElements;

    public class AnchorAppTests
    {
        [Test]
        public void ServiceProvider_SingletonInstanceAndAlias_ReturnsSameResolvedInstance()
        {
            var shared = new SharedSettings();
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(typeof(SharedSettings), shared);
            services.AddAlias<IFooSettings, SharedSettings>();
            services.AddAlias<IBarSettings, SharedSettings>();

            using var provider = services.BuildServiceProvider();

            Assert.AreSame(shared, provider.GetRequiredService<SharedSettings>());
            Assert.AreSame(shared, provider.GetRequiredService<IFooSettings>());
            Assert.AreSame(shared, provider.GetRequiredService<IBarSettings>());
        }

        [Test]
        public void ServiceProvider_TransientRegistration_ReturnsDistinctInstances()
        {
            var services = new AnchorServiceCollection();
            services.AddTransient(typeof(TransientDependency));

            using var provider = services.BuildServiceProvider();
            var first = provider.GetRequiredService<TransientDependency>();
            var second = provider.GetRequiredService<TransientDependency>();

            Assert.AreNotSame(first, second);
        }

        [Test]
        public void ServiceProvider_SelectsLongestResolvableConstructor()
        {
            var services = new AnchorServiceCollection();
            services.AddSingleton(typeof(ConstructorDependency));
            services.AddTransient(typeof(ConstructorSelectionTarget));

            using var provider = services.BuildServiceProvider();
            var resolved = provider.GetRequiredService<ConstructorSelectionTarget>();

            Assert.AreEqual(2, resolved.ConstructorParameterCount);
            Assert.AreSame(provider, resolved.Provider);
            Assert.IsNotNull(resolved.Dependency);
        }

        [Test]
        public void ServiceProvider_SelectsFallbackConstructor_WhenNestedDependencyGraphIsUnresolvable()
        {
            var services = new AnchorServiceCollection();
            services.AddTransient(typeof(FallbackConstructorSelectionTarget));
            services.AddTransient(typeof(UnresolvableNestedDependency));

            using var provider = services.BuildServiceProvider();
            var resolved = provider.GetRequiredService<FallbackConstructorSelectionTarget>();

            Assert.AreEqual(0, resolved.ConstructorParameterCount);
        }

        [Test]
        public void ServiceProvider_SelectsFallbackConstructor_WhenLongerConstructorHasCircularDependency()
        {
            var services = new AnchorServiceCollection();
            services.AddTransient(typeof(CircularConstructorSelectionTarget));
            services.AddTransient(typeof(CircularConstructorDependency));

            using var provider = services.BuildServiceProvider();
            var resolved = provider.GetRequiredService<CircularConstructorSelectionTarget>();

            Assert.AreEqual(0, resolved.ConstructorParameterCount);
        }

        [Test]
        public void ServiceProvider_SelectsFallbackConstructor_WhenLongerConstructorHasAliasCycle()
        {
            var services = new AnchorServiceCollection();
            services.AddTransient(typeof(AliasCycleConstructorSelectionTarget));
            services.AddAlias<IAliasCycleA, IAliasCycleB>();
            services.AddAlias<IAliasCycleB, IAliasCycleA>();

            using var provider = services.BuildServiceProvider();
            var resolved = provider.GetRequiredService<AliasCycleConstructorSelectionTarget>();

            Assert.AreEqual(0, resolved.ConstructorParameterCount);
        }

        [Test]
        public void ServiceProvider_CircularDependency_Throws()
        {
            var services = new AnchorServiceCollection();
            services.AddTransient(typeof(CircularDependencyA));
            services.AddTransient(typeof(CircularDependencyB));

            using var provider = services.BuildServiceProvider();
            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<CircularDependencyA>());

            StringAssert.Contains("Circular dependency detected", exception!.Message);
        }

        [Test]
        public void ServiceProvider_Dispose_DisposesDependentBeforeDependency()
        {
            var disposalOrder = new List<string>();
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(typeof(List<string>), disposalOrder);
            services.AddSingleton(typeof(DisposableDependency));
            services.AddSingleton(typeof(DisposableDependent));

            var provider = services.BuildServiceProvider();
            var dependent = provider.GetRequiredService<DisposableDependent>();

            Assert.IsNotNull(dependent.Dependency);

            provider.Dispose();

            CollectionAssert.AreEqual(new[] { "dependent", "dependency" }, disposalOrder);
        }

        [Test]
        public void ServiceProvider_Dispose_WhenServiceThrows_AttemptsRemainingServicesAndClearsState()
        {
            var disposalOrder = new List<string>();
            var expectedException = new InvalidOperationException("Expected dispose failure.");
            var recording = new RecordingDisposable(disposalOrder);
            var throwing = new ThrowingDisposable(disposalOrder, expectedException);
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(typeof(RecordingDisposable), recording);
            services.AddSingletonInstance(typeof(ThrowingDisposable), throwing);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<RecordingDisposable>();
            provider.GetRequiredService<ThrowingDisposable>();

            var actualException = Assert.Throws<InvalidOperationException>(() => provider.Dispose());

            Assert.AreSame(expectedException, actualException);
            CollectionAssert.AreEqual(new[] { "throwing", "recording" }, disposalOrder);
            Assert.Throws<ObjectDisposedException>(() => provider.GetRequiredService<RecordingDisposable>());
            Assert.DoesNotThrow(() => provider.Dispose());
        }

        [Test]
        public void ServiceProvider_Dispose_WhenMultipleServicesThrow_AggregatesFailures()
        {
            var disposalOrder = new List<string>();
            var firstException = new InvalidOperationException("First dispose failure.");
            var secondException = new InvalidOperationException("Second dispose failure.");
            var first = new ThrowingDisposable(disposalOrder, firstException, "first");
            var second = new ThrowingDisposable(disposalOrder, secondException, "second");
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(typeof(IFirstThrowingDisposable), first);
            services.AddSingletonInstance(typeof(ISecondThrowingDisposable), second);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IFirstThrowingDisposable>();
            provider.GetRequiredService<ISecondThrowingDisposable>();

            var aggregateException = Assert.Throws<AggregateException>(() => provider.Dispose());

            Assert.AreEqual(2, aggregateException!.InnerExceptions.Count);
            Assert.AreSame(secondException, aggregateException.InnerExceptions[0]);
            Assert.AreSame(firstException, aggregateException.InnerExceptions[1]);
            CollectionAssert.AreEqual(new[] { "second", "first" }, disposalOrder);
            Assert.DoesNotThrow(() => provider.Dispose());
        }

        [Test]
        public void ServiceProvider_GetRequiredService_WhenMissing_Throws()
        {
            using var provider = new AnchorServiceCollection().BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IMissingService>());
        }

        [Test]
        public void ServiceCollection_VisualElementsCannotBeRegistered()
        {
            var services = new AnchorServiceCollection();
            var instance = new VisualElementService();

            Assert.Throws<ArgumentException>(() => services.AddSingleton(typeof(VisualElementService)));
            Assert.Throws<ArgumentException>(() => services.AddSingleton(typeof(IVisualElementService), typeof(VisualElementService)));
            Assert.Throws<ArgumentException>(() => services.AddTransient(typeof(VisualElementService)));
            Assert.Throws<ArgumentException>(() => services.AddTransient(typeof(IVisualElementService), typeof(VisualElementService)));
            Assert.Throws<ArgumentException>(() => services.AddSingletonInstance(typeof(IVisualElementService), instance));
            Assert.Throws<ArgumentException>(() => services.AddAlias(typeof(IVisualElementService), typeof(VisualElementService)));
            Assert.AreEqual(0, services.Count);
        }

        [Test]
        public void UpdateScreenMetrics_WhenMetricsChange_RaisesScreenMetricsChanged()
        {
            using var scope = new TestAnchorAppScope();
            var app = scope.App;
            var fired = false;
            var received = default(AnchorScreenMetrics);

            void OnScreenMetricsChanged(AnchorScreenMetrics metrics)
            {
                fired = true;
                received = metrics;
            }

            app.ScreenMetricsChanged += OnScreenMetricsChanged;

            try
            {
                var expected = new AnchorScreenMetrics(1000, 500, new Rect(50f, 20f, 900f, 460f));

                app.UpdateScreenMetrics(expected);

                Assert.IsTrue(fired);
                Assert.AreEqual(expected, received);
            }
            finally
            {
                app.ScreenMetricsChanged -= OnScreenMetricsChanged;
            }
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

        private interface IMissingService
        {
        }

        private interface IVisualElementService
        {
        }

        private sealed class VisualElementService : VisualElement, IVisualElementService
        {
        }

        private sealed class TransientDependency
        {
        }

        private sealed class ConstructorDependency
        {
        }

        private sealed class ConstructorSelectionTarget
        {
            public ConstructorSelectionTarget()
            {
                this.ConstructorParameterCount = 0;
            }

            public ConstructorSelectionTarget(ConstructorDependency dependency, IServiceProvider provider)
            {
                this.Dependency = dependency;
                this.Provider = provider;
                this.ConstructorParameterCount = 2;
            }

            public ConstructorDependency Dependency { get; }

            public IServiceProvider Provider { get; }

            public int ConstructorParameterCount { get; }
        }

        private sealed class FallbackConstructorSelectionTarget
        {
            public FallbackConstructorSelectionTarget()
            {
                this.ConstructorParameterCount = 0;
            }

            public FallbackConstructorSelectionTarget(UnresolvableNestedDependency dependency)
            {
                this.ConstructorParameterCount = 1;
            }

            public int ConstructorParameterCount { get; }
        }

        private sealed class UnresolvableNestedDependency
        {
            public UnresolvableNestedDependency(MissingNestedDependency dependency)
            {
            }
        }

        private sealed class MissingNestedDependency
        {
        }

        private interface IAliasCycleA
        {
        }

        private interface IAliasCycleB
        {
        }

        private sealed class AliasCycleConstructorSelectionTarget
        {
            public AliasCycleConstructorSelectionTarget()
            {
                this.ConstructorParameterCount = 0;
            }

            public AliasCycleConstructorSelectionTarget(IAliasCycleA dependency)
            {
                this.ConstructorParameterCount = 1;
            }

            public int ConstructorParameterCount { get; }
        }

        private sealed class CircularConstructorSelectionTarget
        {
            public CircularConstructorSelectionTarget()
            {
                this.ConstructorParameterCount = 0;
            }

            public CircularConstructorSelectionTarget(CircularConstructorDependency dependency)
            {
                this.ConstructorParameterCount = 1;
            }

            public int ConstructorParameterCount { get; }
        }

        private sealed class CircularConstructorDependency
        {
            public CircularConstructorDependency(CircularConstructorSelectionTarget dependency)
            {
            }
        }

        private sealed class DisposableDependency : IDisposable
        {
            private readonly List<string> disposalOrder;

            public DisposableDependency(List<string> disposalOrder)
            {
                this.disposalOrder = disposalOrder;
            }

            public void Dispose()
            {
                this.disposalOrder.Add("dependency");
            }
        }

        private sealed class DisposableDependent : IDisposable
        {
            private readonly List<string> disposalOrder;

            public DisposableDependent(DisposableDependency dependency, List<string> disposalOrder)
            {
                this.Dependency = dependency;
                this.disposalOrder = disposalOrder;
            }

            public DisposableDependency Dependency { get; }

            public void Dispose()
            {
                this.disposalOrder.Add("dependent");
            }
        }

        private sealed class RecordingDisposable : IDisposable
        {
            private readonly List<string> disposalOrder;

            public RecordingDisposable(List<string> disposalOrder)
            {
                this.disposalOrder = disposalOrder;
            }

            public void Dispose()
            {
                this.disposalOrder.Add("recording");
            }
        }

        private interface IFirstThrowingDisposable : IDisposable
        {
        }

        private interface ISecondThrowingDisposable : IDisposable
        {
        }

        private sealed class ThrowingDisposable : IFirstThrowingDisposable, ISecondThrowingDisposable
        {
            private readonly List<string> disposalOrder;
            private readonly Exception exception;
            private readonly string name;

            public ThrowingDisposable(List<string> disposalOrder, Exception exception, string name = "throwing")
            {
                this.disposalOrder = disposalOrder;
                this.exception = exception;
                this.name = name;
            }

            public void Dispose()
            {
                this.disposalOrder.Add(this.name);
                throw this.exception;
            }
        }

        private sealed class CircularDependencyA
        {
            public CircularDependencyA(CircularDependencyB dependency)
            {
            }
        }

        private sealed class CircularDependencyB
        {
            public CircularDependencyB(CircularDependencyA dependency)
            {
            }
        }
    }
}
