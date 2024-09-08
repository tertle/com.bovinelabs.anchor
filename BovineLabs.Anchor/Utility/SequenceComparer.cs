// <copyright file="SequenceComparer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Collections.Generic;
    using Unity.AppUI.UI;

    public static class SequenceComparer
    {
        public static readonly SequenceComparer<int> Int = new();
        public static readonly SequenceComparer<float> Float = new();
    }

    public class SequenceComparer<T> : EqualityComparer<IEnumerable<T>>
    {
        public override bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            return EnumerableExtensions.SequenceEqual(x, y);
        }

        public override int GetHashCode(IEnumerable<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}
