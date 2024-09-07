// <copyright file="PanelService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    public interface IPanelService
    {
        IReadOnlyCollection<IViewRoot> Panels { get; }
    }

    [Preserve]
    internal class PanelService : IPanelService
    {
        public PanelService(IServiceProvider services)
        {
            var roots = Utility.GetAllImplementations<IViewRoot>().ToArray();

            var viewRoots = new List<IViewRoot>(roots.Length);

            foreach (var type in roots)
            {
                var root = (IViewRoot)services.GetService(type);
                if (root is not VisualElement)
                {
                    Debug.LogError($"{nameof(IViewRoot)} must be used on a {nameof(VisualElement)}");
                    continue;
                }

                viewRoots.Add(root);
            }

            this.Panels = viewRoots;
        }

        public IReadOnlyCollection<IViewRoot> Panels { get; }
    }
}
