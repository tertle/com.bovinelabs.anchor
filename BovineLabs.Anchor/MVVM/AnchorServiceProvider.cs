namespace BovineLabs.Anchor.MVVM
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.ExceptionServices;

    public sealed class AnchorServiceProvider : IServiceProvider, IDisposable
    {
        private readonly AnchorServiceCollection _services;
        private readonly Dictionary<Type, object> _singletonCache = new();
        private readonly List<object> _singletonCreationOrder = new();
        private readonly HashSet<Type> _resolving = new();
        private bool _disposed;

        public AnchorServiceProvider(AnchorServiceCollection services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AnchorServiceProvider));
            }

            if (serviceType == typeof(IServiceProvider) || serviceType == typeof(AnchorServiceProvider))
            {
                return this;
            }

            if (!_resolving.Add(serviceType))
            {
                throw new InvalidOperationException($"Circular dependency detected while resolving '{serviceType.FullName}'.");
            }

            try
            {
                var descriptor = FindDescriptor(serviceType);
                if (descriptor == null)
                {
                    return null;
                }

                if (descriptor.IsAlias)
                {
                    var aliased = GetService(descriptor.AliasType);
                    if (aliased != null && !descriptor.ServiceType.IsInstanceOfType(aliased))
                    {
                        throw new InvalidOperationException(
                            $"Aliased service '{descriptor.AliasType.FullName}' is not assignable to '{descriptor.ServiceType.FullName}'.");
                    }

                    return aliased;
                }

                if (descriptor.Lifetime == AnchorServiceLifetime.Singleton)
                {
                    if (_singletonCache.TryGetValue(serviceType, out var existing))
                    {
                        return existing;
                    }

                    var singleton = CreateService(descriptor);
                    _singletonCache.Add(serviceType, singleton);
                    _singletonCreationOrder.Add(singleton);
                    return singleton;
                }

                return CreateService(descriptor);
            }
            finally
            {
                _resolving.Remove(serviceType);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            var disposedInstances = new List<object>();
            List<Exception> exceptions = null;

            for (var i = _singletonCreationOrder.Count - 1; i >= 0; i--)
            {
                var instance = _singletonCreationOrder[i];
                if (instance is not IDisposable disposable || ContainsReference(disposedInstances, instance))
                {
                    continue;
                }

                disposedInstances.Add(instance);

                try
                {
                    disposable.Dispose();
                }
                catch (Exception exception)
                {
                    exceptions ??= new List<Exception>();
                    exceptions.Add(exception);
                }
            }

            _singletonCache.Clear();
            _singletonCreationOrder.Clear();

            if (exceptions == null)
            {
                return;
            }

            if (exceptions.Count == 1)
            {
                ExceptionDispatchInfo.Capture(exceptions[0]).Throw();
            }

            throw new AggregateException("One or more Anchor services failed to dispose.", exceptions);
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
            for (var i = _services.Count - 1; i >= 0; i--)
            {
                var descriptor = _services[i];
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

            return CreateInstance(descriptor.ImplementationType);
        }

        private object CreateInstance(Type implementationType)
        {
            if (implementationType.IsAbstract || implementationType.IsInterface)
            {
                throw new InvalidOperationException($"Service type '{implementationType.FullName}' cannot be instantiated.");
            }

            var constructor = SelectConstructor(implementationType);
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
                    parameterValue = GetService(parameterType);
                }

                arguments[i] = parameterValue ?? throw new InvalidOperationException(
                    $"Unable to resolve constructor parameter '{parameters[i].Name}' of type '{parameterType.FullName}' for '{implementationType.FullName}'.");
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
            ConstructorInfo circularFallback = null;
            var circularFallbackParameterCount = -1;

            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                if (!CanResolve(parameters, out var circularDependency))
                {
                    if (circularDependency && parameters.Length > circularFallbackParameterCount)
                    {
                        circularFallback = constructor;
                        circularFallbackParameterCount = parameters.Length;
                    }

                    continue;
                }

                if (parameters.Length <= selectedParameterCount)
                {
                    continue;
                }

                selected = constructor;
                selectedParameterCount = parameters.Length;
            }

            return selected ?? circularFallback;
        }

        private bool CanResolve(ParameterInfo[] parameters, out bool circularDependency)
        {
            return CanResolve(parameters, new HashSet<Type>(_resolving), out circularDependency);
        }

        private bool CanResolve(ParameterInfo[] parameters, HashSet<Type> resolutionPath, out bool circularDependency)
        {
            circularDependency = false;

            foreach (var parameter in parameters)
            {
                var parameterType = parameter.ParameterType;
                if (parameterType == typeof(IServiceProvider) || parameterType == typeof(AnchorServiceProvider))
                {
                    continue;
                }

                if (CanResolve(parameterType, resolutionPath, out var parameterIsCircular))
                {
                    continue;
                }

                if (!parameterIsCircular)
                {
                    circularDependency = false;
                    return false;
                }

                circularDependency = true;
            }

            return !circularDependency;
        }

        private bool CanResolve(Type serviceType, HashSet<Type> resolutionPath, out bool circularDependency)
        {
            circularDependency = false;

            if (serviceType == typeof(IServiceProvider) || serviceType == typeof(AnchorServiceProvider))
            {
                return true;
            }

            if (_singletonCache.ContainsKey(serviceType))
            {
                return true;
            }

            if (!resolutionPath.Add(serviceType))
            {
                circularDependency = true;
                return false;
            }

            try
            {
                var descriptor = FindDescriptor(serviceType);
                if (descriptor == null)
                {
                    return false;
                }

                if (descriptor.IsAlias)
                {
                    return CanResolve(descriptor.AliasType, resolutionPath, out circularDependency);
                }

                if (descriptor.IsInstance)
                {
                    return true;
                }

                var implementationType = descriptor.ImplementationType;
                if (implementationType == null || implementationType.IsAbstract || implementationType.IsInterface)
                {
                    return false;
                }

                var constructors = implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                foreach (var constructor in constructors)
                {
                    if (CanResolve(constructor.GetParameters(), resolutionPath, out var constructorIsCircular))
                    {
                        return true;
                    }

                    circularDependency |= constructorIsCircular;
                }

                return false;
            }
            finally
            {
                resolutionPath.Remove(serviceType);
            }
        }
    }
}
