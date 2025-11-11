// <copyright file="IBindingObjectNotify.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

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
        /// <summary>
        /// Signals listeners that a property is about to change.
        /// </summary>
        /// <param name="property">The property identifier that is changing.</param>
        void OnPropertyChanging(in FixedString64Bytes property);

        /// <summary>
        /// Signals listeners that a property value changed.
        /// </summary>
        /// <param name="property">The property identifier that changed.</param>
        void OnPropertyChanged(in FixedString64Bytes property);
    }
}
