using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SJP.GenerationRex
{
    internal class IntSet : IEnumerable<int>, IEquatable<IntSet>
    {
        public IntSet(IEnumerable<int> elems)
        {
            foreach (int elem in elems)
            {
                _elems.Add(elem);
                Choice = Math.Min(elem, Choice);
            }

            _strBuilder = new Lazy<string>(MkString);
        }

        public int Choice { get; } = int.MaxValue;

        internal IntSet Intersect(IEnumerable<int> other)
        {
            var intSet = new HashSet<int>(_elems);
            intSet.IntersectWith(other);
            return new IntSet(intSet);
        }

        private string Repr => _strBuilder.Value;

        public override string ToString() => Repr;

        public override int GetHashCode() => Repr.GetHashCode();

        public bool Equals(IntSet other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Repr == other.Repr;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as IntSet);
        }

        private string MkString()
        {
            if (_elems.Count == 0)
                return "{}";

            var sorted = _elems
                .OrderBy(i => i)
                .Select(i => i.ToString(CultureInfo.InvariantCulture));

            return "{" + string.Join(",", sorted) + "}";
        }

        public IEnumerator<int> GetEnumerator() => _elems.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _elems.GetEnumerator();
        private readonly HashSet<int> _elems = new HashSet<int>();
        private readonly Lazy<string> _strBuilder;
    }
}