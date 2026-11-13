namespace BovineLabs.Anchor.Tests.App
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Anchor.Tests.TestDoubles;
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
        public void ServiceProvider_SingletonFactory_IsLazyAndReceivesResolvingProvider()
        {
            var services = new AnchorServiceCollection();
            var calls = 0;
            IServiceProvider receivedProvider = null;
            services.AddSingleton(typeof(ConstructorDependency), provider =>
            {
                calls++;
                receivedProvider = provider;
                return new ConstructorDependency();
            });

            using var provider = services.BuildServiceProvider();
            Assert.AreEqual(0, calls);

            var first = provider.GetRequiredService<ConstructorDependency>();

            Assert.AreSame(first, provider.GetRequiredService<ConstructorDependency>());
            Assert.AreSame(provider, receivedProvider);
            Assert.AreEqual(1, calls);
        }

        [Test]
        public void ServiceProvider_TransientFactory_CreatesDistinctInstancesWithoutOwningDisposal()
        {
            var disposalOrder = new List<string>();
            var services = new AnchorServiceCollection();
            services.AddTransient<RecordingDisposable>(_ => new RecordingDisposable(disposalOrder));

            using var provider = services.BuildServiceProvider();
            var first = provider.GetRequiredService<RecordingDisposable>();
            var second = provider.GetRequiredService<RecordingDisposable>();

            Assert.AreNotSame(first, second);
            provider.Dispose();

            Assert.IsEmpty(disposalOrder);
            first.Dispose();
            second.Dispose();
        }

        [Test]
        public void ServiceProvider_ForwardingFactories_ShareSingletonAndDisposeItOnce()
        {
            var disposalOrder = new List<string>();
            var services = new AnchorServiceCollection();
            services.AddSingleton<RecordingDisposable>(_ => new RecordingDisposable(disposalOrder));
            services.AddSingleton<IDisposable>(provider => provider.GetRequiredService<RecordingDisposable>());
            services.AddSingleton<object>(provider => provider.GetRequiredService<RecordingDisposable>());

            using var provider = services.BuildServiceProvider();
            var instance = provider.GetRequiredService<RecordingDisposable>();

            Assert.AreSame(instance, provider.GetRequiredService<IDisposable>());
            Assert.AreSame(instance, provider.GetRequiredService<object>());

            provider.Dispose();

            CollectionAssert.AreEqual(new[] { "recording" }, disposalOrder);
        }

        [Test]
        public void ServiceProvider_FactoryDependency_ResolvesConstructorWithoutInvokingFactoryDuringSelection()
        {
            var services = new AnchorServiceCollection();
            var calls = 0;
            services.AddSingleton<ConstructorDependency>(_ =>
            {
                calls++;
                return new ConstructorDependency();
            });
            services.AddTransient(typeof(ConstructorSelectionTarget));

            using var provider = services.BuildServiceProvider();
            var resolved = provider.GetRequiredService<ConstructorSelectionTarget>();

            Assert.AreEqual(2, resolved.ConstructorParameterCount);
            Assert.AreSame(provider.GetRequiredService<ConstructorDependency>(), resolved.Dependency);
            Assert.AreEqual(1, calls);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ServiceProvider_FactoryReturningInvalidResult_Throws(bool returnsNull)
        {
            var services = new AnchorServiceCollection();
            services.AddTransient(typeof(ConstructorDependency), _ => returnsNull ? null : new object());

            using var provider = services.BuildServiceProvider();

            Assert.Throws<InvalidOperationException>(() => provider.GetService(typeof(ConstructorDependency)));
        }

        [Test]
        public void ServiceProvider_FactoryFailure_PropagatesAndAllowsSingletonRetry()
        {
            var services = new AnchorServiceCollection();
            var calls = 0;
            var expectedException = new FormatException("Expected factory failure.");
            services.AddSingleton<ConstructorDependency>(_ =>
            {
                if (++calls == 1)
                {
                    throw expectedException;
                }

                return new ConstructorDependency();
            });

            using var provider = services.BuildServiceProvider();
            var actualException = Assert.Throws<FormatException>(() => provider.GetRequiredService<ConstructorDependency>());
            Assert.AreSame(expectedException, actualException);

            var instance = provider.GetRequiredService<ConstructorDependency>();

            Assert.AreSame(instance, provider.GetRequiredService<ConstructorDependency>());
            Assert.AreEqual(2, calls);
        }

        [Test]
        public void ServiceProvider_FactoryCycle_ThrowsWithoutRecursingIndefinitely()
        {
            var services = new AnchorServiceCollection();
            services.AddSingleton<ConstructorDependency>(provider => provider.GetRequiredService<ConstructorDependency>());

            using var provider = services.BuildServiceProvider();
            var exception = Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ConstructorDependency>());

            StringAssert.Contains("Circular dependency detected", exception!.Message);
        }

        [Test]
        public void ServiceProvider_FactoryReturningVisualThroughObject_Throws()
        {
            var element = new VisualElement();
            try
            {
                var services = new AnchorServiceCollection();
                services.AddSingleton<object>(_ => element);
                using var provider = services.BuildServiceProvider();

                Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<object>());
            }
            finally
            {
                element.ReleaseResources();
            }
        }

        [Test]
        public void ServiceCollection_NullFactoryOrVisualServiceType_RejectsRegistration()
        {
            var services = new AnchorServiceCollection();

            Assert.Throws<ArgumentNullException>(() => services.AddSingleton<object>(null));
            Assert.Throws<ArgumentNullException>(() => services.AddTransient<object>(null));
            Assert.Throws<ArgumentNullException>(() =>
                new AnchorServiceDescriptor(typeof(object), null, AnchorServiceLifetime.Singleton));
            Assert.Throws<ArgumentException>(() => services.AddSingleton<VisualElement>(_ => null));
            Assert.AreEqual(0, services.Count);
        }

        [Test]
        public void ServiceProvider_GenericSingletonInstance_IsAvailableToFactory()
        {
            var instance = new SharedSettings();
            var services = new AnchorServiceCollection();
            services.AddSingletonInstance(instance);
            services.AddTransient<IFooSettings>(provider => provider.GetRequiredService<SharedSettings>());

            using var provider = services.BuildServiceProvider();

            Assert.AreSame(instance, provider.GetRequiredService<IFooSettings>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ServiceCollection_AfterProviderConstruction_RejectsAllRegistrationPaths(bool useBuilder)
        {
            var services = new AnchorServiceCollection();
            services.AddSingleton(typeof(ConstructorDependency));

            Assert.IsFalse(services.IsReadOnly);
            using var provider = useBuilder ? services.BuildServiceProvider() : new AnchorServiceProvider(services);

            Assert.IsTrue(services.IsReadOnly);
            Assert.Throws<InvalidOperationException>(() => services.AddSingleton(typeof(SharedSettings)));
            Assert.Throws<InvalidOperationException>(() => services.AddTransient(typeof(TransientDependency)));
            Assert.Throws<InvalidOperationException>(() => services.AddSingleton<SharedSettings>(_ => new SharedSettings()));
            Assert.Throws<InvalidOperationException>(() => services.AddTransient<SharedSettings>(_ => new SharedSettings()));
            Assert.Throws<InvalidOperationException>(() => services.AddSingletonInstance(new SharedSettings()));
            Assert.Throws<InvalidOperationException>(() => services.AddAlias<IFooSettings, SharedSettings>());
            Assert.AreEqual(1, services.Count);
            Assert.IsNotNull(provider.GetRequiredService<ConstructorDependency>());
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

        private interface IFooSettings
        {
        }

        private interface IBarSettings
        {
        }

        private sealed class SharedSettings : IFooSettings, IBarSettings
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
                ConstructorParameterCount = 0;
            }

            public ConstructorSelectionTarget(ConstructorDependency dependency, IServiceProvider provider)
            {
                Dependency = dependency;
                Provider = provider;
                ConstructorParameterCount = 2;
            }

            public ConstructorDependency Dependency { get; }

            public IServiceProvider Provider { get; }

            public int ConstructorParameterCount { get; }
        }

        private sealed class FallbackConstructorSelectionTarget
        {
            public FallbackConstructorSelectionTarget()
            {
                ConstructorParameterCount = 0;
            }

            public FallbackConstructorSelectionTarget(UnresolvableNestedDependency dependency)
            {
                ConstructorParameterCount = 1;
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
                ConstructorParameterCount = 0;
            }

            public AliasCycleConstructorSelectionTarget(IAliasCycleA dependency)
            {
                ConstructorParameterCount = 1;
            }

            public int ConstructorParameterCount { get; }
        }

        private sealed class CircularConstructorSelectionTarget
        {
            public CircularConstructorSelectionTarget()
            {
                ConstructorParameterCount = 0;
            }

            public CircularConstructorSelectionTarget(CircularConstructorDependency dependency)
            {
                ConstructorParameterCount = 1;
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
            private readonly List<string> _disposalOrder;

            public DisposableDependency(List<string> disposalOrder)
            {
                _disposalOrder = disposalOrder;
            }

            public void Dispose()
            {
                _disposalOrder.Add("dependency");
            }
        }

        private sealed class DisposableDependent : IDisposable
        {
            private readonly List<string> _disposalOrder;

            public DisposableDependent(DisposableDependency dependency, List<string> disposalOrder)
            {
                Dependency = dependency;
                _disposalOrder = disposalOrder;
            }

            public DisposableDependency Dependency { get; }

            public void Dispose()
            {
                _disposalOrder.Add("dependent");
            }
        }

        private sealed class RecordingDisposable : IDisposable
        {
            private readonly List<string> _disposalOrder;

            public RecordingDisposable(List<string> disposalOrder)
            {
                _disposalOrder = disposalOrder;
            }

            public void Dispose()
            {
                _disposalOrder.Add("recording");
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
            private readonly List<string> _disposalOrder;
            private readonly Exception _exception;
            private readonly string _name;

            public ThrowingDisposable(List<string> disposalOrder, Exception exception, string name = "throwing")
            {
                _disposalOrder = disposalOrder;
                _exception = exception;
                _name = name;
            }

            public void Dispose()
            {
                _disposalOrder.Add(_name);
                throw _exception;
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
