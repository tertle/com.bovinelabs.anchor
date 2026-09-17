namespace BovineLabs.Anchor.Services
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.MVVM;
    using UnityEngine.Scripting;

    [Preserve]
    internal record ViewModelService : IViewModelService
    {
        private readonly Dictionary<Type, object> loadedElements = new();

        public T Load<T>()
            where T : class
        {
            if (!this.loadedElements.TryGetValue(typeof(T), out var element))
            {
                element = this.loadedElements[typeof(T)] = AnchorApp.Current.Services.GetRequiredService<T>();
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

