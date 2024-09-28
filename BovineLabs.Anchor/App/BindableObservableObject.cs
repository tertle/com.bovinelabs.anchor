// <copyright file="BindableObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.ComponentModel;
    using Unity.AppUI.MVVM;
    using UnityEngine.UIElements;

    /// <summary>
    /// An extension of <see cref="ObservableObject"/> that also automatically implements the <see cref="INotifyBindablePropertyChanged.propertyChanged"/>
    /// event via the <see cref="OnPropertyChanged"/>.
    /// </summary>
    public abstract class BindableObservableObject : ObservableObject, INotifyBindablePropertyChanged
    {
        private event EventHandler<BindablePropertyChangedEventArgs> PropertyChangedInternal;

        /// <inheritdoc/>
        event EventHandler<BindablePropertyChangedEventArgs> INotifyBindablePropertyChanged.propertyChanged
        {
            add => this.PropertyChangedInternal += value;
            remove => this.PropertyChangedInternal -= value;
        }

        /// <inheritdoc/>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            this.PropertyChangedInternal?.Invoke(this, new BindablePropertyChangedEventArgs(e.PropertyName));
        }
    }
}
