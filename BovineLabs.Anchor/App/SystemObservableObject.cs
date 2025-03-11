﻿// <copyright file="SystemObservableObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor
{
    using System.ComponentModel;
    using BovineLabs.Anchor.Binding;
    using Unity.AppUI.MVVM;
    using Unity.Collections;

    public abstract class SystemObservableObject<T> : ObservableObject, IBindingObjectNotify<T>, IViewModel
        where T : unmanaged
    {
        public abstract ref T Value { get; }

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
