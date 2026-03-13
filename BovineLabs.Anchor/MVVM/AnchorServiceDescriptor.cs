// <copyright file="AnchorServiceDescriptor.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;

    public enum AnchorServiceLifetime
    {
        Singleton,
        Transient,
    }

    public sealed class AnchorServiceDescriptor
    {
        private AnchorServiceDescriptor(
            Type serviceType,
            Type implementationType,
            AnchorServiceLifetime lifetime,
            object implementationInstance,
            Type aliasType)
        {
            this.ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            this.ImplementationType = implementationType;
            this.Lifetime = lifetime;
            this.ImplementationInstance = implementationInstance;
            this.AliasType = aliasType;
        }

        public Type ServiceType { get; }

        public Type ImplementationType { get; }

        public AnchorServiceLifetime Lifetime { get; }

        public object ImplementationInstance { get; }

        public Type AliasType { get; }

        public bool IsAlias => this.AliasType != null;

        public bool IsInstance => this.ImplementationInstance != null;

        public static AnchorServiceDescriptor Singleton(Type serviceType, Type implementationType)
        {
            ValidateImplementation(serviceType, implementationType);
            return new AnchorServiceDescriptor(serviceType, implementationType, AnchorServiceLifetime.Singleton, null, null);
        }

        public static AnchorServiceDescriptor Transient(Type serviceType, Type implementationType)
        {
            ValidateImplementation(serviceType, implementationType);
            return new AnchorServiceDescriptor(serviceType, implementationType, AnchorServiceLifetime.Transient, null, null);
        }

        public static AnchorServiceDescriptor SingletonInstance(Type serviceType, object implementationInstance)
        {
            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            ValidateServiceCompatibility(serviceType, implementationInstance.GetType());
            return new AnchorServiceDescriptor(serviceType, implementationInstance.GetType(), AnchorServiceLifetime.Singleton, implementationInstance, null);
        }

        public static AnchorServiceDescriptor Alias(Type serviceType, Type existingServiceType)
        {
            if (existingServiceType == null)
            {
                throw new ArgumentNullException(nameof(existingServiceType));
            }

            return new AnchorServiceDescriptor(serviceType, null, AnchorServiceLifetime.Singleton, null, existingServiceType);
        }

        private static void ValidateImplementation(Type serviceType, Type implementationType)
        {
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            ValidateServiceCompatibility(serviceType, implementationType);
        }

        private static void ValidateServiceCompatibility(Type serviceType, Type implementationType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (!serviceType.IsAssignableFrom(implementationType))
            {
                throw new ArgumentException($"{implementationType} is not assignable to {serviceType}.", nameof(implementationType));
            }
        }
    }
}
