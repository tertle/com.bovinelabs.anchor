// <copyright file="SystemObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.MVVM;
    using BovineLabs.Core.Assertions;
    using Unity.Collections;
    using UnityEngine;

    /// <summary>
    /// Observable object base class that exposes unmanaged data for burst-compatible bindings.
    /// </summary>
    /// <typeparam name="T"> The unmanaged data. </typeparam>
    [IsService]
    [Serializable]
    public abstract class SystemObservableObject<T> : ObservableObject, IBindingObjectNotify<T>
        where T : unmanaged
    {
        [SerializeField]
        private DataBox data = new();

        private GCHandle pin;

        public ref T Value => ref this.data.Value;

        /// <inheritdoc/>
        void IBindingObjectNotify<T>.Pin()
        {
            Check.Assume(!this.pin.IsAllocated);
            this.pin = GCHandle.Alloc(this.data, GCHandleType.Pinned);
        }

        /// <inheritdoc/>
        void IBindingObjectNotify<T>.Unpin()
        {
            Check.Assume(this.pin.IsAllocated);
            this.pin.Free();
            this.pin = default;
        }

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
            if (newValue != null && oldValue.Value.AsArray().SequenceEqual(newValue))
            {
                return false;
            }

            this.OnPropertyChanging(propertyName);
            oldValue.SetValue(newValue);
            this.OnPropertyChanged(propertyName);
            return true;
        }

        [Serializable]
        private sealed class DataBox
        {
            public T Value;
        }
    }
}
