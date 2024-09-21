// <copyright file="ViewService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using Unity.Burst;
    using Unity.Collections;
    using UnityEngine.UIElements;

    public interface IViewService
    {
        T LoadView<T>()
            where T : VisualElement;

        void UnloadView<T>()
            where T : VisualElement;

        T GetView<T>()
            where T : VisualElement;
    }

    public class ViewService : IViewService
    {
        private readonly Dictionary<Type, VisualElement> loadedElements = new();


        public T LoadView<T>()
            where T : VisualElement
        {
            if (!this.loadedElements.TryGetValue(typeof(T), out var element))
            {
                element = this.loadedElements[typeof(T)] = App.current.services.GetService<T>();
            }

            return (T)element;
        }

        public void UnloadView<T>()
            where T : VisualElement
        {
            this.loadedElements.Remove(typeof(T));
        }

        public T GetView<T>()
            where T : VisualElement
        {
            this.loadedElements.TryGetValue(typeof(T), out var element);
            return element as T;
        }
    }
}
