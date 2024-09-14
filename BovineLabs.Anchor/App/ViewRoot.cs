// <copyright file="ViewRoot.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    public interface IViewRoot : IView
    {
        int Priority { get; }
    }
}
