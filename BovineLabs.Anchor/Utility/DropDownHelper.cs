// <copyright file="DropDownHelper.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unity.AppUI.UI;

    public static class DropDownHelper
    {
        public static void BindTitle<T>(DropdownItem item, MultiContainer<T> items, IEnumerable<int> selected, string unselected)
            where T : unmanaged, IEquatable<T>
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
