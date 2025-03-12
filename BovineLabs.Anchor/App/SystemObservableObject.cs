// <copyright file="SystemObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor
{
    using System;
    using System.ComponentModel;
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.MVVM;
    using Unity.Collections;
    using UnityEngine;

    [Serializable]
    public abstract class SystemObservableObject<T> : ObservableObject, IBindingObjectNotify<T>, IViewModel
        where T : unmanaged
    {
        [SerializeField]
        private T data;

        public ref T Value => ref this.data;

        public void OnPropertyChanging(in FixedString64Bytes property)
        {
            this.OnPropertyChanging(new PropertyChangingEventArgs(property.ToString()));
        }

        public void OnPropertyChanged(in FixedString64Bytes property)
        {
            this.OnPropertyChanged(new PropertyChangedEventArgs(property.ToString()));
        }
    }
}
#endif
