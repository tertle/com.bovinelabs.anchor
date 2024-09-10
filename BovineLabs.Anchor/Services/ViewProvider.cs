// <copyright file="ViewProvider.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
    using System;
    using JetBrains.Annotations;
    using UnityEngine.UIElements;

    [UsedImplicitly]
    public class ViewProvider
    {
        private readonly IServiceProvider serviceProvider;

        // private readonly Dictionary<Type, VisualElement> views = new();

        public ViewProvider(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            Instance = this;
        }

        public static ViewProvider Instance { get; private set; }

        // public T LoadView<T>()
        //     where T : VisualElement, IView
        // {
        //     var view = (T)this.serviceProvider.GetService(typeof(T));
        //     this.views.Add(typeof(T), view);
        //     return view;
        // }
        //
        // public void UnloadView<T>()
        //     where T : VisualElement, IView
        // {
        //     this.views.Remove(typeof(T));
        // }

        public T GetView<T>()
            where T : VisualElement, IView
        {
            // return (T)this.views[typeof(T)];
            return (T)this.serviceProvider.GetService(typeof(T));
        }
    }
}
