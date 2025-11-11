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
        T Load<T>()
            where T : class;

        /// <summary>Releases a view model instance of the requested type.</summary>
        void Unload<T>()
            where T : class;

        /// <summary>Gets an already-loaded view model instance if available.</summary>
        T Get<T>()
            where T : class;
    }
}
