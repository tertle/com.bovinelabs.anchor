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

        public static readonly SequenceListComparer<int> IntList = new();
        public static readonly SequenceListComparer<float> FloatList = new();
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

    public class SequenceListComparer<T> : EqualityComparer<List<T>>
    {
        public override bool Equals(List<T> x, List<T> y)
        {
            return EnumerableExtensions.SequenceEqual(x, y);
        }

        public override int GetHashCode(List<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}
