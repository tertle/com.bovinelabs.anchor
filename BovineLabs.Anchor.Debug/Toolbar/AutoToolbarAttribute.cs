// <copyright file="AutoToolbarAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;
    using JetBrains.Annotations;

    /// <summary>
    /// Marks an <see cref="IToolbarElement"/> service so it can automatically register with the Anchor toolbar.
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
            if (string.IsNullOrWhiteSpace(elementName))
            {
                throw new ArgumentException("Element name cannot be null or whitespace.", nameof(elementName));
            }

            this.ElementName = elementName;
            this.TabName = tabName;
        }

        /// <summary>Gets the label shown for the toolbar element.</summary>
        public string ElementName { get; }

        /// <summary>Gets the target tab that should host the element.</summary>
        public string TabName { get; }
    }
}
