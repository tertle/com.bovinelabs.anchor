// <copyright file="ViewRoot.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using UnityEngine.UIElements;

    public interface IViewRoot : IView
    {
        int Priority { get; }

        void AttachedToPanel(VisualElement value)
        {
        }
    }
}
