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
    using BovineLabs.Anchor.Binding;
    using BovineLabs.Anchor.MVVM;
    using Unity.Collections;

    /// <summary>
    /// Observable object base class that exposes unmanaged data for burst-compatible bindings.
    /// </summary>
    [IsService]
    [Serializable]
    public abstract unsafe class SystemObservableObject<T> : ObservableObject, IBindingObjectNotify<T>, IDisposable
        where T : unmanaged
    {
        private T* data;

        protected SystemObservableObject()
        {
            this.data = AllocatorManager.Allocate<T>(Allocator.Persistent);
            *this.data = new T();
        }

        /// <summary>Gets the underlying unmanaged value reference that can be mutated from burst.</summary>
        public ref T Value => ref *this.data;

        public void Dispose()
        {
            AllocatorManager.Free(Allocator.Persistent, this.data);
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
    }
}
