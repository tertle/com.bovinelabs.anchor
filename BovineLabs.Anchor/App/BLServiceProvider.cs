// <copyright file="BLServiceProvider.cs" company="BovineLabs">
// Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;
    using Unity.AppUI.MVVM;

    /// <summary>
    /// The default IServiceProvider.
    /// </summary>
    internal sealed class BlServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceCollection services;
        private readonly ConcurrentDictionary<Type, Func<object>> realizedServices;
        private readonly ConcurrentDictionary<Type, object> singletons;

        /// <summary>Initializes a new instance of the <see cref="BlServiceProvider"/> class. </summary>
        /// <param name="serviceCollection"> The service collection to use. </param>
        /// <exception cref="ArgumentNullException"> Thrown when the service collection is null. </exception>
        public BlServiceProvider(IServiceCollection serviceCollection)
        {
            this.services = serviceCollection ?? throw new ArgumentNullException(nameof(serviceCollection));
            this.realizedServices = new ConcurrentDictionary<Type, Func<object>>();
            this.singletons = new ConcurrentDictionary<Type, object>();
        }

        /// <summary> Gets or sets a value indicating whether the service provider has been disposed. </summary>
        private bool Disposed { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.Disposed)
            {
                return;
            }

            this.realizedServices.Clear();
            this.singletons.Clear();
            this.Disposed = true;
        }

        /// <summary>
        /// Get a service from the service provider.
        /// </summary>
        /// <param name="serviceType"> The type of the service to get. </param>
        /// <returns> The service instance. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the requested service is not registered. </exception>
        public object GetService(Type serviceType)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException($"The {nameof(BlServiceProvider)} object has already been disposed.");
            }

            ServiceDescriptor desc = null;
            foreach (var d in this.services)
            {
                if (d.serviceType == serviceType)
                {
                    desc = d;

                    break;
                }
            }

            if (desc == null)
            {
                throw new InvalidOperationException($"Unable to find Service Descriptor for {serviceType.FullName}.");
            }

            var realizedService = this.realizedServices.GetOrAdd(serviceType, this.RealizeService);

            return realizedService?.Invoke();
        }

        public void AddServiceInstance<T, TI>(TI instance)
            where T : class
            where TI : class, T
        {
            this.services.AddSingleton<T, TI>();
            this.realizedServices.GetOrAdd(typeof(T), () => instance);
        }

        private Func<object> RealizeService(Type serviceType)
        {
            ServiceDescriptor desc = null;

            foreach (var d in this.services)
            {
                if (d.serviceType == serviceType)
                {
                    desc = d;

                    break;
                }
            }

            if (desc == null)
            {
                throw new InvalidOperationException($"No service registered for type {serviceType.FullName}.");
            }

            var constructors = desc.implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(
                    $"The type {desc.implementationType.FullName} doesn't contain any public constructor.");
            }

            ConstructorInfo bestConstructor = null;

            foreach (var constructor in constructors)
            {
                if (this.IsValidConstructor(constructor))
                {
                    bestConstructor = constructor;

                    break;
                }
            }

            if (bestConstructor == null)
            {
                throw new InvalidOperationException(
                    $"The type {desc.implementationType.FullName} doesn't contain any valid constructor.");
            }

            return () =>
            {
                if (desc.lifetime == ServiceLifetime.Singleton)
                {
                    if (!this.singletons.ContainsKey(serviceType))
                    {
                        this.singletons[serviceType] = this.ConstructAndInject(bestConstructor);
                    }

                    return this.singletons[serviceType];
                }

                return this.ConstructAndInject(bestConstructor);
            };
        }

        private object ConstructAndInject(ConstructorInfo bestConstructor)
        {
            var service = bestConstructor.Invoke(this.GetConstructorParameters(bestConstructor));
            var serviceType = service.GetType();

            foreach (var field in serviceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetCustomAttribute<ServiceAttribute>() != null)
                {
                    field.SetValue(service, this.GetService(field.FieldType));
                }
            }

            foreach (var property in serviceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute<ServiceAttribute>() != null)
                {
                    property.SetValue(service, this.GetService(property.PropertyType));
                }
            }

            if (service is IDependencyInjectionListener listener)
            {
                listener.OnDependenciesInjected();
            }

            return service;
        }

        private object[] GetConstructorParameters(ConstructorInfo info)
        {
            var parameters = info.GetParameters();
            var result = new object[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                result[i] = this.GetService(parameter.ParameterType);
            }

            return result;
        }

        private bool IsValidConstructor(ConstructorInfo info)
        {
            var res = true;

            foreach (var param in info.GetParameters())
            {
                res &= this.IsValidConstructorParameter(param);
            }

            return res;
        }

        private bool IsValidConstructorParameter(ParameterInfo param)
        {
            foreach (var service in this.services)
            {
                if (service.serviceType.IsAssignableFrom(param.ParameterType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
