using System;
using System.Collections.Generic;

namespace SJP.GenerationRex;

internal class Pair<TFirst, TSecond> : IEquatable<Pair<TFirst, TSecond>>
{
    public Pair(TFirst first, TSecond second)
    {
        if (first == null)
            throw new ArgumentNullException(nameof(first));
        if (second == null)
            throw new ArgumentNullException(nameof(second));

        First = first;
        Second = second;
    }

    public TFirst First { get; }

    public TSecond Second { get; }

    public override string ToString() => "(" + First + "," + Second + ")";

    public bool Equals(Pair<TFirst, TSecond> other)
    {
        if (other is null)
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
        if (obj is null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj is Pair<TFirst, TSecond> pair && Equals(pair);
    }

    public override int GetHashCode() => HashCode.Combine(First, Second);
}