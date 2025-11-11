// <copyright file="SystemObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.UI;
    using Unity.Collections;
    using UnityEngine;

    /// <summary>
    /// Observable object base class that exposes unmanaged data for burst-compatible bindings.
    /// </summary>
    [IsService]
    [Serializable]
    public abstract class SystemObservableObject<T> : ObservableObject, IBindingObjectNotify<T>
        where T : unmanaged
    {
        [SerializeField]
        private T data;

        /// <summary>Gets the underlying unmanaged value reference that can be mutated from burst.</summary>
        public ref T Value => ref this.data;

        /// <inheritdoc/>
        public void OnPropertyChanging(in FixedString64Bytes property)
        {
            this.OnPropertyChanging(new PropertyChangingEventArgs(property.ToString()));
        }

        /// <inheritdoc/>
        public void OnPropertyChanged(in FixedString64Bytes property)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(property.ToString()));
        }

        protected bool SetProperty<TV>(ChangedList<TV> oldValue, IEnumerable<TV> newValue, [CallerMemberName] string propertyName = null)
            where TV : unmanaged
        {
            if (EnumerableExtensions.SequenceEqual(oldValue.Value.AsArray(), newValue))
            {
                return false;
            }

            this.OnPropertyChanging(propertyName);
            oldValue.SetValue(newValue);
            this.OnPropertyChanged(propertyName);
            return true;
        }
    }
}
