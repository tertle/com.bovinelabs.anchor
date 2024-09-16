// <copyright file="BindableObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.ComponentModel;
    using Unity.AppUI.MVVM;
    using UnityEngine.UIElements;

    public abstract class BindableObservableObject : ObservableObject, INotifyBindablePropertyChanged
    {
        private event EventHandler<BindablePropertyChangedEventArgs> PropertyChangedInternal;

        /// <inheritdoc/>
        event EventHandler<BindablePropertyChangedEventArgs> INotifyBindablePropertyChanged.propertyChanged
        {
            add => this.PropertyChangedInternal += value;
            remove => this.PropertyChangedInternal -= value;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            this.PropertyChangedInternal?.Invoke(this, new BindablePropertyChangedEventArgs(e.PropertyName));
        }
    }
}
