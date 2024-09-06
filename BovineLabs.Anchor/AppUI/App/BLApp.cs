// <copyright file="BLApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Linq;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class BLApp : App
    {
        public BLApp(IServiceProvider serviceProvider, IPanelService panelService)
        {
            this.Panel = new Panel();
            foreach (var view in panelService.Panels.OrderBy(r => r.Priority))
            {
                this.Panel.Add((VisualElement)view);
                view.AttachedToPanel(this.Panel);
            }

            this.mainPage = this.Panel;
        }

        public static BLApp Current => current as BLApp;

        public Panel Panel { get; }
    }
}
