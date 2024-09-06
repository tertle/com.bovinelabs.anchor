// <copyright file="ObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.ComponentModel;
    using BovineLabs.Core.UI;
    using Unity.Collections;
    using UnityEngine.UIElements;

    public abstract class BLObservableObject : Unity.AppUI.MVVM.ObservableObject, INotifyBindablePropertyChanged
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

    public abstract class BLObservableObject<T> : BLObservableObject, IBindingObjectNotify<T>
        where T : unmanaged, IBindingObjectNotifyData
    {
        public abstract ref T Value { get; }

        public void OnPropertyChanged(in FixedString64Bytes property)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(property.ToString()));
        }
    }
}
