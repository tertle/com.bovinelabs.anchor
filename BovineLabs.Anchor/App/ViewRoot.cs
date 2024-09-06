// <copyright file="ViewRoot.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using Unity.AppUI.UI;

    public interface IViewRoot : IView
    {
        int Priority { get; }

        void AttachedToPanel(Panel value)
        {
        }
    }
}
