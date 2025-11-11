// <copyright file="AutoToolbarAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Marks a VisualElement service so it can automatically be added to the Anchor toolbar.
    /// </summary>
    [MeansImplicitUse]
    public class AutoToolbarAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoToolbarAttribute"/> class.
        /// </summary>
        /// <param name="elementName">Display name for the toolbar panel.</param>
        /// <param name="tabName">Optional tab name; defaults to the service tab.</param>
        public AutoToolbarAttribute(string elementName, string tabName = null)
        {
            this.ElementName = elementName;
            this.TabName = tabName;
        }

        /// <summary>Gets the label shown for the toolbar element.</summary>
        public string ElementName { get; }

        /// <summary>Gets the target tab that should host the element.</summary>
        public string TabName { get; }
    }
}
#endif
