// <copyright file="IBindingObject.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_ENTITIES
namespace BovineLabs.Anchor.Binding
{
    public interface IBindingObject
    {
        void Load()
        {
        }

        void Unload()
        {
        }
    }

    public interface IBindingObject<T> : IBindingObject
        where T : unmanaged
    {
        ref T Value { get; }
    }
}
#endif
