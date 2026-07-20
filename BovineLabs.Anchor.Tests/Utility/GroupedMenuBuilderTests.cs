// <copyright file="GroupedMenuBuilderTests.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Tests.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using Unity.AppUI.UI;
    using UnityEngine.UIElements;

    public class GroupedMenuBuilderTests
    {
        [Test]
        public void AddGroupedActions_WhenUnderLimit_AddsFlatSortedActions()
        {
            var menu = BuildMenu(
                new[]
                {
                    new TestItem("Beta"),
                    new TestItem("Alpha"),
                    new TestItem("Gamma"),
                },
                new GroupedMenuBuilderOptions { MaxItemsPerMenu = 4 });

            CollectionAssert.AreEqual(new[] { "Alpha", "Beta", "Gamma" }, GetLabels(menu));
            Assert.IsFalse(GetItems(menu).Any(i => i.hasSubMenu));
        }

        [Test]
        public void AddGroupedActions_DefaultGrouping_CombinesFirstLettersIntoRanges()
        {
            var menu = BuildMenu(
                new[]
                {
                    new TestItem("Alpha"),
                    new TestItem("Beta"),
                    new TestItem("Charlie"),
                    new TestItem("Delta"),
                    new TestItem("Echo"),
                    new TestItem("Foxtrot"),
                },
                new GroupedMenuBuilderOptions { MaxItemsPerMenu = 3 });

            CollectionAssert.AreEqual(new[] { "A-C", "D-F" }, GetLabels(menu));
            CollectionAssert.AreEqual(new[] { "Alpha", "Beta", "Charlie" }, GetLabels(GetItems(menu)[0].subMenu));
            CollectionAssert.AreEqual(new[] { "Delta", "Echo", "Foxtrot" }, GetLabels(GetItems(menu)[1].subMenu));
        }

        [Test]
        public void AddGroupedActions_CustomPrimaryGrouping_UsesCallerBucketsBeforeLabelPrefixes()
        {
            var menu = BuildMenu(
                new[]
                {
                    new TestItem("Quest Beta", "Quest"),
                    new TestItem("Combat Alpha", "Combat"),
                    new TestItem("Quest Alpha", "Quest"),
                    new TestItem("Social Alpha", "Social"),
                },
                new GroupedMenuBuilderOptions { MaxItemsPerMenu = 3 },
                item => item.Group);

            CollectionAssert.AreEqual(new[] { "Combat Alpha", "Quest", "Social Alpha" }, GetLabels(menu));
            CollectionAssert.AreEqual(new[] { "Quest Alpha", "Quest Beta" }, GetLabels(GetItems(menu)[1].subMenu));
        }

        [Test]
        public void AddGroupedActions_CustomPrimaryGrouping_UsesCallerComparer()
        {
            var menu = BuildMenu(
                new[]
                {
                    new TestItem("Quest Alpha", "Quest"),
                    new TestItem("Combat Alpha", "Combat"),
                    new TestItem("Social Alpha", "Social"),
                    new TestItem("Quest Beta", "Quest"),
                },
                new GroupedMenuBuilderOptions { MaxItemsPerMenu = 3 },
                item => item.Group,
                new GroupOrderComparer("Quest", "Combat", "Social"));

            CollectionAssert.AreEqual(new[] { "Quest", "Combat Alpha", "Social Alpha" }, GetLabels(menu));
        }

        [Test]
        public void AddGroupedActions_WhenSingleLetterGroupIsOversized_SplitsByLongerPrefixes()
        {
            var menu = BuildMenu(
                new[]
                {
                    new TestItem("Alabaster"),
                    new TestItem("Algae"),
                    new TestItem("Almond"),
                    new TestItem("Amber"),
                    new TestItem("Amethyst"),
                    new TestItem("Anchor"),
                },
                new GroupedMenuBuilderOptions { MaxItemsPerMenu = 3 });

            CollectionAssert.AreEqual(new[] { "AL", "AM", "Anchor" }, GetLabels(menu));
            CollectionAssert.AreEqual(new[] { "Alabaster", "Algae", "Almond" }, GetLabels(GetItems(menu)[0].subMenu));
            CollectionAssert.AreEqual(new[] { "Amber", "Amethyst" }, GetLabels(GetItems(menu)[1].subMenu));
        }

        [Test]
        public void AddGroupedActions_DuplicateLabels_KeepSeparateActionIdsAndCallbacks()
        {
            var selected = new List<int>();
            var menu = BuildMenu(
                new[]
                {
                    new TestItem("Same", id: 10),
                    new TestItem("Same", id: 20),
                    new TestItem("Same", id: 30),
                },
                new GroupedMenuBuilderOptions { MaxItemsPerMenu = 5 },
                null,
                null,
                item => selected.Add(item.Id));

            var items = GetItems(menu);
            CollectionAssert.AreEqual(new[] { "Same", "Same", "Same" }, items.Select(i => i.label).ToArray());
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, items.Select(i => (int)i.userData).ToArray());

            items[1].clickable.InvokePressed(null);
            CollectionAssert.AreEqual(new[] { 20 }, selected);
        }

        private static Menu BuildMenu(
            IReadOnlyList<TestItem> items,
            GroupedMenuBuilderOptions options,
            Func<TestItem, string> primaryGroupSelector = null,
            IComparer<string> primaryGroupComparer = null,
            Action<TestItem> callback = null)
        {
            var menu = new Menu();
            var builder = MenuBuilder.Build(new VisualElement(), menu);
            builder.AddGroupedActions(items, static item => item.Label, primaryGroupSelector, primaryGroupComparer, callback ?? (_ => { }), options);
            return menu;
        }

        private static MenuItem[] GetItems(Menu menu)
        {
            return menu.Children().OfType<MenuItem>().ToArray();
        }

        private static string[] GetLabels(Menu menu)
        {
            return GetItems(menu).Select(item => item.label).ToArray();
        }

        private sealed class GroupOrderComparer : IComparer<string>
        {
            private readonly IReadOnlyList<string> order;

            public GroupOrderComparer(params string[] order)
            {
                this.order = order;
            }

            public int Compare(string x, string y)
            {
                var xIndex = this.IndexOf(x);
                var yIndex = this.IndexOf(y);
                if (xIndex != yIndex)
                {
                    return xIndex.CompareTo(yIndex);
                }

                return StringComparer.OrdinalIgnoreCase.Compare(x, y);
            }

            private int IndexOf(string value)
            {
                for (var i = 0; i < this.order.Count; i++)
                {
                    if (string.Equals(this.order[i], value, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }

                return int.MaxValue;
            }
        }

        private readonly struct TestItem
        {
            public TestItem(string label, string group = null, int id = 0)
            {
                this.Label = label;
                this.Group = group;
                this.Id = id;
            }

            public string Label { get; }

            public string Group { get; }

            public int Id { get; }
        }
    }
}
