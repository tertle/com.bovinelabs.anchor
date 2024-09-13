﻿// <copyright file="BLApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Linq;
    using BovineLabs.Anchor.Services;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class BLApp : App
    {
        private readonly IPanelService panelService;

        public BLApp(IPanelService panelService)
        {
            this.panelService = panelService;
        }

        public static BLApp Current => current as BLApp;

        public virtual Panel Panel => (Panel)this.rootVisualElement;

        public override void InitializeComponent()
        {
            base.InitializeComponent();

            foreach (var view in this.panelService.Panels.OrderBy(r => r.Priority))
            {
                this.rootVisualElement.Add((VisualElement)view);
                view.AttachedToPanel(this.rootVisualElement); // TODO can we do without?
            }
        }
    }
}
