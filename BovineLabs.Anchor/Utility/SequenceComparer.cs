// <copyright file="SequenceComparer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Collections.Generic;
    using Unity.AppUI.UI;

    /// <summary>
    /// Provides cached sequence comparers for common primitive types.
    /// </summary>
    public static class SequenceComparer
    {
        public static readonly SequenceComparer<int> Int = new();
        public static readonly SequenceComparer<float> Float = new();

        public static readonly SequenceListComparer<int> IntList = new();
        public static readonly SequenceListComparer<float> FloatList = new();
    }

    /// <summary>
    /// Equality comparer that performs element-wise comparisons on enumerable sequences.
    /// </summary>
    public class SequenceComparer<T> : EqualityComparer<IEnumerable<T>>
    {
        /// <inheritdoc/>
        public override bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            return EnumerableExtensions.SequenceEqual(x, y);
        }

        /// <inheritdoc/>
        public override int GetHashCode(IEnumerable<T> obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    /// Equality comparer that performs element-wise comparisons on <see cref="List{T}"/> instances.
    /// </summary>
    public class SequenceListComparer<T> : EqualityComparer<List<T>>
    {
        /// <inheritdoc/>
        public override bool Equals(List<T> x, List<T> y)
        {
            return EnumerableExtensions.SequenceEqual(x, y);
        }

        /// <inheritdoc/>
        public override int GetHashCode(List<T> obj)
        {
            return obj.GetHashCode();
        }
    }
}
