﻿// <copyright file="PanelService.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Services
{
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
        private List<IViewRoot> panels;

        public IReadOnlyCollection<IViewRoot> Panels
        {
            get
            {
                if (this.panels == null)
                {
                    var roots = Core.GetAllImplementations<IViewRoot>().ToArray();

                    this.panels = new List<IViewRoot>(roots.Length);

                    foreach (var type in roots)
                    {
                        var root = (IViewRoot)BLApp.Current.services.GetService(type);
                        if (root is not VisualElement)
                        {
                            Debug.LogError($"{nameof(IViewRoot)} must be used on a {nameof(VisualElement)}");
                            continue;
                        }

                        this.panels.Add(root);
                    }
                }

                return this.panels;
            }
        }
    }
}
