// <copyright file="DropDownHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BovineLabs.Core.Extensions;
    using Unity.AppUI.UI;
    using Unity.Collections;

    public static class DropDownHelper
    {
        public static List<string> GetItems(List<string> items, NativeArray<FixedString64Bytes> native)
        {
            if (native.IsCreated)
            {
                items.Clear();
                foreach (var c in native)
                {
                    items.Add(c.ToString());
                }
            }

            return items;
        }

        public static List<string> GetItems(List<string> items, NativeArray<FixedString32Bytes> native)
        {
            if (native.IsCreated)
            {
                items.Clear();
                foreach (var c in native)
                {
                    items.Add(c.ToString());
                }
            }

            return items;
        }

        public static List<string> GetItems(List<string> items, NativeArray<FixedString64Bytes> native, Func<string, string> formatter)
        {
            var items2 = GetItems(items, native);
            for (var index = 0; index < items2.Count; index++)
            {
                items2[index] = formatter(items2[index]);
            }

            return items2;
        }

        public static List<string> GetItems(List<string> items, NativeArray<FixedString32Bytes> native, Func<string, string> formatter)
        {
            var items2 = GetItems(items, native);
            for (var index = 0; index < items2.Count; index++)
            {
                items2[index] = formatter(items2[index]);
            }

            return items2;
        }

        public static IEnumerable<int> GetMultiValues(List<int> values, NativeList<int> native)
        {
            if (native.IsCreated)
            {
                values.Clear();
                values.AddRangeNative(native.AsArray());
            }

            return values;
        }

        public static void SetMultiValues(IEnumerable<int> values, NativeList<int> native)
        {
            if (!native.IsCreated)
            {
                return;
            }

            native.Clear();
            foreach (var v in values)
            {
                native.Add(v);
            }
        }

        public static Action<DropdownItem, int> BindItem(List<string> items)
        {
            return (item, i) => item.label = items[i];
        }

        public static Action<DropdownItem, IEnumerable<int>> BindTitle(List<string> items, string unselected)
        {
            return (item, ints) =>
            {
                var text = string.Join(',', ints.Select(s => items[s]));
                if (string.IsNullOrWhiteSpace(text))
                {
                    text = unselected;
                }

                item.labelElement.text = text;
            };
        }
    }
}
