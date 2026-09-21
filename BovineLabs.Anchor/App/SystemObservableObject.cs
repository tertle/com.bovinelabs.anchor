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

    [IsService]
    [Serializable]
    public abstract class SystemObservableObject<T> : ObservableObject, IBindingObjectNotify<T>
        where T : unmanaged
    {
        [SerializeField]
        private DataBox _data = new();

        private GCHandle _pin;

        public ref T Value => ref _data.Value;

        void IBindingObjectNotify<T>.Pin()
        {
            Check.Assume(!_pin.IsAllocated);
            _pin = GCHandle.Alloc(_data, GCHandleType.Pinned);
        }

        void IBindingObjectNotify<T>.Unpin()
        {
            Check.Assume(_pin.IsAllocated);
            _pin.Free();
            _pin = default;
        }

        public void OnPropertyChanging(in FixedString64Bytes property)
        {
            OnPropertyChanging(new PropertyChangingEventArgs(property.ToString()));
        }

        public void OnPropertyChanged(in FixedString64Bytes property)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(property.ToString()));
        }

        protected bool SetProperty<TV>(ChangedList<TV> oldValue, IEnumerable<TV> newValue, [CallerMemberName] string propertyName = null)
            where TV : unmanaged
        {
            if (newValue != null && oldValue.Value.AsArray().SequenceEqual(newValue))
            {
                return false;
            }

            OnPropertyChanging(propertyName);
            oldValue.SetValue(newValue);
            OnPropertyChanged(propertyName);
            return true;
        }

        [Serializable]
        private sealed class DataBox
        {
            public T Value;
        }
    }
}
