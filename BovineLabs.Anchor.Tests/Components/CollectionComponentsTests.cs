namespace BovineLabs.Anchor.Tests.Components
{
    using System;
    using System.Collections.Generic;
    using BovineLabs.Anchor.Components;
    using NUnit.Framework;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    public class CollectionComponentsTests
    {
        private readonly List<VisualElement> _roots = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var root in _roots)
            {
                root.RemoveFromHierarchy();
                root.hierarchy.Clear(VisualElementClearOptions.RecursiveReleaseResources);
                root.ReleaseResources();
            }

            _roots.Clear();
        }

        [Test]
        public void GridTemplate_BindsRowsFromCurrentStockItemsSource()
        {
            var first = new object();
            var second = new object();
            var replacement = new object();
            var grid = new GridView { itemsSource = new[] { first, second } };
            _roots.Add(grid);
            grid.GetOrAddComponent<GridItemTemplate>();
            var row = grid.makeItem();
            _roots.Add(row);

            grid.bindItem(row, 1);
            Assert.AreSame(second, row.dataSource);
            Assert.AreEqual("Template Not Found", ((Label)row).text);

            grid.itemsSource = new[] { replacement };
            grid.bindItem(row, 0);

            Assert.AreSame(replacement, row.dataSource);
        }

        [Test]
        public void GridTemplate_RemovalRestoresPreviousCallbacks()
        {
            Func<VisualElement> makeItem = static () => new VisualElement();
            Action<VisualElement, int> bindItem = static (element, index) => element.dataSource = index;
            var grid = new GridView { makeItem = makeItem, bindItem = bindItem };
            _roots.Add(grid);
            grid.GetOrAddComponent<GridItemTemplate>();

            grid.RemoveComponent<GridItemTemplate>();

            Assert.AreSame(makeItem, grid.makeItem);
            Assert.AreSame(bindItem, grid.bindItem);
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        public void GridTemplate_RemovalPreservesCallbacksReplacedByTheOwner(bool replaceFactory, bool replaceBinder)
        {
            Func<VisualElement> previousMakeItem = static () => new VisualElement();
            Action<VisualElement, int> previousBindItem = static (element, index) => element.dataSource = index;
            Func<VisualElement> replacementMakeItem = static () => new Label();
            Action<VisualElement, int> replacementBindItem = static (element, index) => element.name = index.ToString();
            var grid = new GridView { makeItem = previousMakeItem, bindItem = previousBindItem };
            _roots.Add(grid);
            grid.GetOrAddComponent<GridItemTemplate>();
            if (replaceFactory)
            {
                grid.makeItem = replacementMakeItem;
            }

            if (replaceBinder)
            {
                grid.bindItem = replacementBindItem;
            }

            grid.RemoveComponent<GridItemTemplate>();

            Assert.AreSame(replaceFactory ? replacementMakeItem : previousMakeItem, grid.makeItem);
            Assert.AreSame(replaceBinder ? replacementBindItem : previousBindItem, grid.bindItem);
        }

        [Test]
        public void AccordionItems_WithoutTemplatePreservesExistingContentAndDataSource()
        {
            var accordion = new Accordion();
            _roots.Add(accordion);
            var source = new object();
            var item = new AccordionItem { dataSource = source };
            accordion.Add(item);
            accordion.AddComponent(new AccordionItems { ItemsSource = new[] { new object() } });

            accordion.RemoveComponent<AccordionItems>();

            Assert.AreSame(item, accordion[0]);
            Assert.AreSame(source, item.dataSource);
            Assert.IsFalse(item.resourcesReleased);
        }
    }
}
