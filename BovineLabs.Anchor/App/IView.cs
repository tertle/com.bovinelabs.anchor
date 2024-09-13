// <copyright file="IView.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    public interface IView
    {
    }

    public interface IView<out T> : IView
    {
        T ViewModel { get; }
    }
}