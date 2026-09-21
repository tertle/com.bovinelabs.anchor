namespace BovineLabs.Anchor.Tests.Utility
{
    using System.Collections;
    using System.Collections.Generic;
    using NUnit.Framework;

    public class SequenceComparerTests
    {
        [Test]
        public void SequenceComparer_NullSequences_AreEqualAndHaveZeroHashCode()
        {
            var comparer = new SequenceComparer<int>();

            Assert.IsTrue(comparer.Equals(null, null));
            Assert.AreEqual(0, comparer.GetHashCode(null));
            Assert.IsFalse(comparer.Equals(null, new[] { 1 }));
        }

        [Test]
        public void SequenceComparer_EqualElements_ProduceEqualHashCodes()
        {
            var comparer = new SequenceComparer<int>();
            var first = new FixedHashSequence<int>(new[] { 1, 2, 3 }, 1);
            var second = new FixedHashSequence<int>(new[] { 1, 2, 3 }, 2);

            Assert.IsTrue(comparer.Equals(first, second));
            Assert.AreEqual(comparer.GetHashCode(first), comparer.GetHashCode(second));
        }

        [Test]
        public void SequenceListComparer_EqualAndNullLists_ObeyEqualityHashContract()
        {
            var comparer = new SequenceListComparer<int>();
            var first = new FixedHashList<int>(new[] { 1, 2, 3 }, 1);
            var second = new FixedHashList<int>(new[] { 1, 2, 3 }, 2);

            Assert.IsTrue(comparer.Equals(null, null));
            Assert.AreEqual(0, comparer.GetHashCode(null));
            Assert.IsTrue(comparer.Equals(first, second));
            Assert.AreEqual(comparer.GetHashCode(first), comparer.GetHashCode(second));
        }

        private sealed class FixedHashSequence<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _values;
            private readonly int _hashCode;

            public FixedHashSequence(IEnumerable<T> values, int hashCode)
            {
                _values = values;
                _hashCode = hashCode;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return _values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }

        private sealed class FixedHashList<T> : List<T>
        {
            private readonly int _hashCode;

            public FixedHashList(IEnumerable<T> values, int hashCode)
                : base(values)
            {
                _hashCode = hashCode;
            }

            public override int GetHashCode()
            {
                return _hashCode;
            }
        }
    }
}
