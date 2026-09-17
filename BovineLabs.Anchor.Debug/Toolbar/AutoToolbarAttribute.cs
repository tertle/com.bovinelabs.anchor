namespace BovineLabs.Anchor.Debug.Toolbar
{
    using System;
    using JetBrains.Annotations;

    [MeansImplicitUse]
    public class AutoToolbarAttribute : Attribute
    {
        public AutoToolbarAttribute(string elementName, string tabName = null)
        {
            if (string.IsNullOrWhiteSpace(elementName))
            {
                throw new ArgumentException("Element name cannot be null or whitespace.", nameof(elementName));
            }

            this.ElementName = elementName;
            this.TabName = tabName;
        }

        public string ElementName { get; }

        public string TabName { get; }
    }
}
