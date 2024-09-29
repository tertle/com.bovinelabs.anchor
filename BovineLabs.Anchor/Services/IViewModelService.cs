// <copyright file="IViewModelService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    public interface IViewModelService
    {
        T Load<T>()
            where T : class;

        void Unload<T>()
            where T : class;

        T Get<T>()
            where T : class;
    }
}
