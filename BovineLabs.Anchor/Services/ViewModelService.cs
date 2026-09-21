namespace BovineLabs.Anchor.Services
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.MVVM;
    using UnityEngine.Scripting;

    [Preserve]
    internal record ViewModelService : IViewModelService
    {
        private readonly Dictionary<Type, object> _loadedElements = new();

        public T Load<T>()
            where T : class
        {
            if (!_loadedElements.TryGetValue(typeof(T), out var element))
            {
                element = _loadedElements[typeof(T)] = AnchorApp.Current.Services.GetRequiredService<T>();
            }

            return (T)element;
        }

        public void Unload<T>()
            where T : class
        {
            _loadedElements.Remove(typeof(T));
        }

        public T Get<T>()
            where T : class
        {
            _loadedElements.TryGetValue(typeof(T), out var element);
            return element as T;
        }
    }
}

