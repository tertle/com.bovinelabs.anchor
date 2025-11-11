// <copyright file="IViewModelService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    /// <summary>
    /// Provides lifetime management for view model services used by Anchor UI.
    /// </summary>
    public interface IViewModelService
    {
        /// <summary>Loads or creates a view model instance of the requested type.</summary>
        /// <typeparam name="T">Type of view model to resolve.</typeparam>
        /// <returns>The loaded view model instance.</returns>
        T Load<T>()
            where T : class;

        /// <summary>Releases a view model instance of the requested type.</summary>
        void Unload<T>()
            where T : class;

        /// <summary>Gets an already-loaded view model instance if available.</summary>
        /// <typeparam name="T">Type of view model to retrieve.</typeparam>
        /// <returns>The cached view model, or null when not loaded.</returns>
        T Get<T>()
            where T : class;
    }
}
