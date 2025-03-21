﻿// <copyright file="DropDownHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.AppUI.UI;
    using Unity.Collections;
#if BL_CORE
    using ListExtensions = BovineLabs.Core.Extensions.ListExtensions;
#endif

    public interface IDropDownItem
    {
        string GetString();
    }

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

        public static List<string> GetItems<T>(List<string> items, NativeArray<T> native)
            where T : unmanaged, IDropDownItem
        {
            if (native.IsCreated)
            {
                items.Clear();
                foreach (var c in native)
                {
                    items.Add(c.GetString());
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

        public static List<string> GetItems<T>(List<string> items, NativeArray<T> native, Func<string, string> formatter)
            where T : unmanaged, IDropDownItem
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
#if BL_CORE
                ListExtensions.AddRangeNative(values, native.AsArray());
#else
                foreach (var i in native.AsArray())
                {
                    values.Add(i);
                }
#endif
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

        public static void BindItem(List<string> items, DropdownItem item, int index)
        {
            item.label = items[index];
        }

        public static void BindTitle(List<string> items, string unselected, DropdownItem item, IEnumerable<int> selected)
        {
            var text = string.Join(',', selected.Select(s => items[s]));
            if (string.IsNullOrWhiteSpace(text))
            {
                text = unselected;
            }

            item.labelElement.text = text;
        }
    }
}
