// <copyright file="DropDownBindings.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor.Binding
{
    using System.Collections.Generic;

    public class DropDownBindings
    {
        public readonly List<string> SourceItems = new();
        public readonly List<int> ValuesCache = new();
        public IEnumerable<int> Values = new List<int>();
    }
}
