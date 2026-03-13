// <copyright file="AnchorServiceCollection.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class AnchorServiceCollection : IEnumerable<AnchorServiceDescriptor>
    {
        private readonly List<AnchorServiceDescriptor> descriptors = new();

        public int Count => this.descriptors.Count;

        public AnchorServiceDescriptor this[int index] => this.descriptors[index];

        public IEnumerator<AnchorServiceDescriptor> GetEnumerator()
        {
            return this.descriptors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public AnchorServiceCollection AddSingleton(Type serviceType)
        {
            return this.Add(AnchorServiceDescriptor.Singleton(serviceType, serviceType));
        }

        public AnchorServiceCollection AddSingleton(Type serviceType, Type implementationType)
        {
            return this.Add(AnchorServiceDescriptor.Singleton(serviceType, implementationType));
        }

        public AnchorServiceCollection AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return this.AddSingleton(typeof(TService), typeof(TImplementation));
        }

        public AnchorServiceCollection AddTransient(Type serviceType)
        {
            return this.Add(AnchorServiceDescriptor.Transient(serviceType, serviceType));
        }

        public AnchorServiceCollection AddTransient(Type serviceType, Type implementationType)
        {
            return this.Add(AnchorServiceDescriptor.Transient(serviceType, implementationType));
        }

        public AnchorServiceCollection AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return this.AddTransient(typeof(TService), typeof(TImplementation));
        }

        public AnchorServiceCollection AddSingletonInstance(Type serviceType, object instance)
        {
            return this.Add(AnchorServiceDescriptor.SingletonInstance(serviceType, instance));
        }

        public AnchorServiceCollection AddAlias(Type serviceType, Type existingServiceType)
        {
            return this.Add(AnchorServiceDescriptor.Alias(serviceType, existingServiceType));
        }

        public AnchorServiceCollection AddAlias<TService, TExistingService>()
        {
            return this.AddAlias(typeof(TService), typeof(TExistingService));
        }

        public AnchorServiceProvider BuildServiceProvider()
        {
            return new AnchorServiceProvider(this);
        }

        private AnchorServiceCollection Add(AnchorServiceDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            this.descriptors.Add(descriptor);
            return this;
        }
    }
}
