// <copyright file="AnchorServiceProvider.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public sealed class AnchorServiceProvider : IServiceProvider, IDisposable
    {
        private readonly AnchorServiceCollection services;
        private readonly Dictionary<Type, object> singletonCache = new();
        private readonly HashSet<Type> resolving = new();
        private bool disposed;

        public AnchorServiceProvider(AnchorServiceCollection services)
        {
            this.services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(AnchorServiceProvider));
            }

            if (serviceType == typeof(IServiceProvider) || serviceType == typeof(AnchorServiceProvider))
            {
                return this;
            }

            if (this.resolving.Contains(serviceType))
            {
                throw new InvalidOperationException($"Circular dependency detected while resolving '{serviceType.FullName}'.");
            }

            this.resolving.Add(serviceType);

            try
            {
                var descriptor = this.FindDescriptor(serviceType);
                if (descriptor == null)
                {
                    return null;
                }

                if (descriptor.IsAlias)
                {
                    var aliased = this.GetService(descriptor.AliasType);
                    if (aliased != null && !descriptor.ServiceType.IsInstanceOfType(aliased))
                    {
                        throw new InvalidOperationException(
                            $"Aliased service '{descriptor.AliasType.FullName}' is not assignable to '{descriptor.ServiceType.FullName}'.");
                    }

                    return aliased;
                }

                if (descriptor.Lifetime == AnchorServiceLifetime.Singleton)
                {
                    if (this.singletonCache.TryGetValue(serviceType, out var existing))
                    {
                        return existing;
                    }

                    var singleton = this.CreateService(descriptor);
                    this.singletonCache.Add(serviceType, singleton);
                    return singleton;
                }

                return this.CreateService(descriptor);
            }
            finally
            {
                this.resolving.Remove(serviceType);
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            var disposedInstances = new List<object>();
            foreach (var instance in this.singletonCache.Values)
            {
                if (instance is not IDisposable disposable || ContainsReference(disposedInstances, instance))
                {
                    continue;
                }

                disposable.Dispose();
                disposedInstances.Add(instance);
            }

            this.singletonCache.Clear();
            this.disposed = true;
        }

        private static bool ContainsReference(List<object> values, object target)
        {
            foreach (var value in values)
            {
                if (ReferenceEquals(value, target))
                {
                    return true;
                }
            }

            return false;
        }

        private AnchorServiceDescriptor FindDescriptor(Type serviceType)
        {
            for (var i = this.services.Count - 1; i >= 0; i--)
            {
                var descriptor = this.services[i];
                if (descriptor.ServiceType == serviceType)
                {
                    return descriptor;
                }
            }

            return null;
        }

        private object CreateService(AnchorServiceDescriptor descriptor)
        {
            if (descriptor.IsInstance)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationType == null)
            {
                return null;
            }

            return this.CreateInstance(descriptor.ImplementationType);
        }

        private object CreateInstance(Type implementationType)
        {
            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                throw new InvalidOperationException($"Service type '{implementationType.FullName}' cannot be instantiated.");
            }

            var constructor = this.SelectConstructor(implementationType);
            if (constructor == null)
            {
                throw new InvalidOperationException($"No valid public constructor found for '{implementationType.FullName}'.");
            }

            var parameters = constructor.GetParameters();
            var arguments = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                object parameterValue;

                if (parameterType == typeof(IServiceProvider) || parameterType == typeof(AnchorServiceProvider))
                {
                    parameterValue = this;
                }
                else
                {
                    parameterValue = this.GetService(parameterType);
                }

                if (parameterValue == null)
                {
                    throw new InvalidOperationException(
                        $"Unable to resolve constructor parameter '{parameters[i].Name}' of type '{parameterType.FullName}' for '{implementationType.FullName}'.");
                }

                arguments[i] = parameterValue;
            }

            return constructor.Invoke(arguments);
        }

        private ConstructorInfo SelectConstructor(Type implementationType)
        {
            var constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
            {
                return null;
            }

            ConstructorInfo selected = null;
            var selectedParameterCount = -1;

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                if (!this.CanResolve(parameters))
                {
                    continue;
                }

                if (parameters.Length <= selectedParameterCount)
                {
                    continue;
                }

                selected = constructor;
                selectedParameterCount = parameters.Length;
            }

            return selected;
        }

        private bool CanResolve(ParameterInfo[] parameters)
        {
            foreach (var parameter in parameters)
            {
                var parameterType = parameter.ParameterType;
                if (parameterType == typeof(IServiceProvider) || parameterType == typeof(AnchorServiceProvider))
                {
                    continue;
                }

                if (this.FindDescriptor(parameterType) == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
