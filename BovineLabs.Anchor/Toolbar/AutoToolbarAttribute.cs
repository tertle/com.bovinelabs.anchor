// <copyright file="AutoToolbarAttribute.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if BL_DEBUG || UNITY_EDITOR
namespace BovineLabs.Anchor.Toolbar
{
    using System;
    using JetBrains.Annotations;

    [MeansImplicitUse]
    public class AutoToolbarAttribute : Attribute
    {
        public AutoToolbarAttribute(string elementName, string tabName = null)
        {
            this.ElementName = elementName;
            this.TabName = tabName;
        }

        public string ElementName { get; }

        public string TabName { get; }
    }
}
#endif
