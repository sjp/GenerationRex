using System;
using System.Collections.Generic;

namespace SJP.GenerationRex
{
    internal class Pair<TFirst, TSecond> : IEquatable<Pair<TFirst, TSecond>>
    {
        public Pair(TFirst first, TSecond second)
        {
            if (ReferenceEquals(first, null))
                throw new ArgumentNullException(nameof(first));
            if (ReferenceEquals(second, null))
                throw new ArgumentNullException(nameof(second));

            First = first;
            Second = second;
        }

        public TFirst First { get; }

        public TSecond Second { get; }

        public override string ToString() => "(" + First + "," + Second + ")";

        public bool Equals(Pair<TFirst, TSecond> other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            var firstComparer = EqualityComparer<TFirst>.Default;
            var secondComparer = EqualityComparer<TSecond>.Default;

            return firstComparer.Equals(First, other.First)
                && secondComparer.Equals(Second, other.Second);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as Pair<TFirst, TSecond>);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = (hash * 23) + First.GetHashCode();
                hash = (hash * 23) + Second.GetHashCode();
                return hash;
            }
        }
    }
}
