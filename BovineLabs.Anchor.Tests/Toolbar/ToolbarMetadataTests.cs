// <copyright file="ToolbarMetadataTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

#if UNITY_APPUI
namespace BovineLabs.Anchor.Tests.Toolbar
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using NUnit.Framework;
    using UnityEngine.UIElements;
    using ToolbarGroupElement = BovineLabs.Anchor.Debug.Toolbar.ToolbarGroupElement;

    public class ToolbarMetadataTests
    {
        [Test]
        public void AutoToolbarAttribute_StoresValues()
        {
            var attribute = new AutoToolbarAttribute("Element", "Tab");

            Assert.AreEqual("Element", attribute.ElementName);
            Assert.AreEqual("Tab", attribute.TabName);
        }

        [Test]
        public void AutoToolbarAttribute_WhitespaceName_Throws()
        {
            Assert.Throws<System.ArgumentException>(() => new AutoToolbarAttribute(" "));
        }

        [Test]
        public void ToolbarGroupElement_AddRemoveToTab_ManagesHierarchy()
        {
            var tab = new VisualElement();
            var groupElement = new ToolbarGroupElement();

            Assert.DoesNotThrow(() => groupElement.AddToTab(tab));
            Assert.AreSame(tab, groupElement.parent);
            Assert.AreSame(tab, groupElement.horizontalScroller.parent);

            Assert.DoesNotThrow(() => groupElement.RemoveFromTab());
            Assert.IsNull(groupElement.parent);
            Assert.IsNull(groupElement.horizontalScroller.parent);
        }
    }
}
#endif
