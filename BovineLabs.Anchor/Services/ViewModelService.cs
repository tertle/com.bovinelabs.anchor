// <copyright file="ViewModelService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using System.Collections.Generic;
    using Unity.AppUI.MVVM;

    internal record ViewModelService : IViewModelService
    {
        private readonly Dictionary<Type, object> loadedElements = new();

        public T Load<T>()
            where T : class
        {
            if (!this.loadedElements.TryGetValue(typeof(T), out var element))
            {
                element = this.loadedElements[typeof(T)] = App.current.services.GetService<T>();
            }

            return (T)element;
        }

        public void Unload<T>()
            where T : class
        {
            this.loadedElements.Remove(typeof(T));
        }

        public T Get<T>()
            where T : class
        {
            this.loadedElements.TryGetValue(typeof(T), out var element);
            return element as T;
        }
    }
}
