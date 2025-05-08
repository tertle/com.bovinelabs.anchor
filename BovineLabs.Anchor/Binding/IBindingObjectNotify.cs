// <copyright file="IBindingObjectNotify.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Binding
{
    using Unity.Collections;
    using UnityEngine.UIElements;

    /// <summary>
    /// Base interface for binding and notifying from burst. This should very rarely be used,
    /// instead you should be implementing the generic <see cref="IBindingObjectNotify{T}"/>.
    /// </summary>
    public interface IBindingObjectNotify : INotifyBindablePropertyChanged
    {
        void OnPropertyChanging(in FixedString64Bytes property);

        void OnPropertyChanged(in FixedString64Bytes property);
    }
}
#endif
