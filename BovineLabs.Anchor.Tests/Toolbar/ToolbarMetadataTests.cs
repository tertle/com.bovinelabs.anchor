// <copyright file="ToolbarMetadataTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Toolbar
{
    using BovineLabs.Anchor.Debug.Toolbar;
    using NUnit.Framework;
    using UnityEngine.UIElements;
    using ToolbarGroupElement = BovineLabs.Anchor.Debug.Toolbar.ToolbarGroupElement;

    public class ToolbarMetadataTests
    {
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
