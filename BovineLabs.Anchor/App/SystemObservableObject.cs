// <copyright file="SystemObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor
{
    using System.ComponentModel;
    using BovineLabs.Anchor.Binding;
    using Unity.Collections;

    public abstract class SystemObservableObject<T> : BindableObservableObject, IBindingObjectNotify<T>
        where T : unmanaged, IBindingObjectNotifyData
    {
        public abstract ref T Value { get; }

        public void OnPropertyChanged(in FixedString64Bytes property)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(property.ToString()));
        }
    }
}
#endif
