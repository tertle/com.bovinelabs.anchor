// <copyright file="AnchorApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Diagnostics.CodeAnalysis;
    using BovineLabs.Anchor.Toolbar;
    using Unity.AppUI.MVVM;
    using Unity.AppUI.Navigation;
    using Unity.AppUI.UI;
    using UnityEngine.Scripting;
    using UnityEngine.UIElements;

    [Preserve]
    public class AnchorApp : App
    {
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Overwriting AppUI standard")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:Element should begin with upper-case letter", Justification = "Overwriting AppUI standard")]
        public static new AnchorApp current => App.current as AnchorApp;

        public virtual Panel Panel => (Panel)this.rootVisualElement;

        public NavGraphViewAsset GraphViewAsset { get; internal set; }

        public VisualElement PopupContainer { get; private set; }

        public VisualElement NotificationContainer { get; private set; }

        public VisualElement TooltipContainer { get; private set; }

        public virtual INavVisualController NavVisualController { get; internal set; }

        public virtual void AddRoots()
        {
            var toolbarView = this.services.GetService<ToolbarView>();
            this.rootVisualElement.Add(toolbarView);

            var navigationView = this.services.GetService<NavigationView>();
            this.rootVisualElement.Add(navigationView);

            this.PopupContainer = this.rootVisualElement.Q<VisualElement>("popup-container");
            this.NotificationContainer = this.rootVisualElement.Q<VisualElement>("notification-container");
            this.TooltipContainer = this.rootVisualElement.Q<VisualElement>("tooltip-container");
        }
    }
}
