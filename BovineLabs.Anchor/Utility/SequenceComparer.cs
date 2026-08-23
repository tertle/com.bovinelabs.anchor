// <copyright file="SequenceComparer.cs" company="BovineLabs">
//     Copyright (c) BovineLabs. All rights reserved.
// </copyright>

namespace BovineLabs.Anchor
{
    using System.Collections.Generic;
    using System.Linq;
    using Unity.Scripting.LifecycleManagement;

    /// <summary>
    /// Provides cached sequence comparers for common primitive types.
    /// </summary>
    public static class SequenceComparer
    {
        [NoAutoStaticsCleanup]
        public static readonly SequenceComparer<int> Int = new();

        [NoAutoStaticsCleanup]
        public static readonly SequenceComparer<float> Float = new();

        [NoAutoStaticsCleanup]
        public static readonly SequenceListComparer<int> IntList = new();

        [NoAutoStaticsCleanup]
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
            return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
        }

        /// <inheritdoc/>
        public override int GetHashCode(IEnumerable<T> obj)
        {
            if (obj == null)
            {
                return 0;
            }

            var hashCode = 17;
            var elementComparer = EqualityComparer<T>.Default;

            unchecked
            {
                foreach (var element in obj)
                {
                    hashCode = (hashCode * 31) + (ReferenceEquals(element, null) ? 0 : elementComparer.GetHashCode(element));
                }
            }

            return hashCode;
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
            return ReferenceEquals(x, y) || (x != null && y != null && x.SequenceEqual(y));
        }

        /// <inheritdoc/>
        public override int GetHashCode(List<T> obj)
        {
            if (obj == null)
            {
                return 0;
            }

            var hashCode = 17;
            var elementComparer = EqualityComparer<T>.Default;

            unchecked
            {
                foreach (var element in obj)
                {
                    hashCode = (hashCode * 31) + (ReferenceEquals(element, null) ? 0 : elementComparer.GetHashCode(element));
                }
            }

            return hashCode;
        }
    }
}
