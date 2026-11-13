namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class AnchorServiceCollection : IEnumerable<AnchorServiceDescriptor>
    {
        private readonly List<AnchorServiceDescriptor> _descriptors = new();

        public int Count => _descriptors.Count;

        public bool IsReadOnly { get; private set; }

        public AnchorServiceDescriptor this[int index] => _descriptors[index];

        public IEnumerator<AnchorServiceDescriptor> GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public AnchorServiceCollection AddSingleton(Type serviceType)
        {
            return Add(AnchorServiceDescriptor.Singleton(serviceType, serviceType));
        }

        public AnchorServiceCollection AddSingleton(Type serviceType, Type implementationType)
        {
            return Add(AnchorServiceDescriptor.Singleton(serviceType, implementationType));
        }

        public AnchorServiceCollection AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return AddSingleton(typeof(TService), typeof(TImplementation));
        }

        public AnchorServiceCollection AddSingleton(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            return Add(AnchorServiceDescriptor.Singleton(serviceType, implementationFactory));
        }

        public AnchorServiceCollection AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return Add(AnchorServiceDescriptor.Singleton(implementationFactory));
        }

        public AnchorServiceCollection AddTransient(Type serviceType)
        {
            return Add(AnchorServiceDescriptor.Transient(serviceType, serviceType));
        }

        public AnchorServiceCollection AddTransient(Type serviceType, Type implementationType)
        {
            return Add(AnchorServiceDescriptor.Transient(serviceType, implementationType));
        }

        public AnchorServiceCollection AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return AddTransient(typeof(TService), typeof(TImplementation));
        }

        public AnchorServiceCollection AddTransient(Type serviceType, Func<IServiceProvider, object> implementationFactory)
        {
            return Add(AnchorServiceDescriptor.Transient(serviceType, implementationFactory));
        }

        public AnchorServiceCollection AddTransient<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            return Add(AnchorServiceDescriptor.Transient(implementationFactory));
        }

        public AnchorServiceCollection AddSingletonInstance(Type serviceType, object instance)
        {
            return Add(AnchorServiceDescriptor.SingletonInstance(serviceType, instance));
        }

        public AnchorServiceCollection AddSingletonInstance<TService>(TService instance)
            where TService : class
        {
            return Add(AnchorServiceDescriptor.SingletonInstance(instance));
        }

        public AnchorServiceCollection AddAlias(Type serviceType, Type existingServiceType)
        {
            return Add(AnchorServiceDescriptor.Alias(serviceType, existingServiceType));
        }

        public AnchorServiceCollection AddAlias<TService, TExistingService>()
        {
            return AddAlias(typeof(TService), typeof(TExistingService));
        }

        public AnchorServiceProvider BuildServiceProvider()
        {
            return new AnchorServiceProvider(this);
        }

        internal void MakeReadOnly()
        {
            IsReadOnly = true;
        }

        private AnchorServiceCollection Add(AnchorServiceDescriptor descriptor)
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("The service collection cannot be modified after a provider has been built.");
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            _descriptors.Add(descriptor);
            return this;
        }
    }
}
