// <copyright file="ToolbarMetadataTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Toolbar
{
    using BovineLabs.Anchor.Toolbar;
    using NUnit.Framework;
    using AppUIButton = Unity.AppUI.UI.Button;
    using UnityEngine.UIElements;

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
        public void ToolbarGroupAndTab_Constructors_PreserveReferencesAndIds()
        {
            var groupElement = new ToolbarGroupElement();
            var button = new AppUIButton();
            var group = new ToolbarGroup("Group", button, groupElement);

            var tabContainer = new ToolbarTabElement("Tab");
            var view = new VisualElement();
            var tab = new ToolbarGroup.Tab(7, "TabName", tabContainer, group, view);

            Assert.AreEqual("Group", group.Name);
            Assert.AreSame(button, group.Button);
            Assert.AreSame(groupElement, group.Parent);
            Assert.AreEqual(7, tab.ID);
            Assert.AreEqual("TabName", tab.Name);
            Assert.AreSame(tabContainer, tab.Container);
            Assert.AreSame(group, tab.Group);
            Assert.AreSame(view, tab.View);
        }

        [Test]
        public void ToolbarTabElement_ContentContainer_ReceivesAddedChildren()
        {
            var tab = new ToolbarTabElement("Title");
            var child = new VisualElement();

            tab.contentContainer.Add(child);

            Assert.AreSame(tab.contentContainer, child.parent);
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
