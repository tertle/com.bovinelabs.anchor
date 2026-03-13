// <copyright file="AnchorServiceProviderExtensions.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.MVVM
{
    using System;

    public static class AnchorServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider)
            where T : class
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            return serviceProvider.GetService(typeof(T)) as T;
        }

        public static object GetRequiredService(this IServiceProvider serviceProvider, Type serviceType)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var service = serviceProvider.GetService(serviceType);
            if (service == null)
            {
                throw new InvalidOperationException($"No service registered for type '{serviceType.FullName}'.");
            }

            return service;
        }

        public static T GetRequiredService<T>(this IServiceProvider serviceProvider)
            where T : class
        {
            return (T)GetRequiredService(serviceProvider, typeof(T));
        }
    }
}
