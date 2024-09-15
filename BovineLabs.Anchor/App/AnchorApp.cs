// <copyright file="AnchorApp.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
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

        public virtual INavVisualController NavVisualController { get; internal set; }

        public virtual void AddRoots(IReadOnlyList<IViewRoot> panels)
        {
            foreach (var view in panels.OrderBy(r => r.Priority))
            {
                this.rootVisualElement.Add((VisualElement)view);
            }
        }
    }
}
