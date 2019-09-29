using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.GenerationRex
{
    internal class BddBuilder : ICharacterConstraintSolver<BinaryDecisionDiagram>
    {
        public BddBuilder(int bitLength)
        {
            _bitMaps = new int[bitLength];
            for (int index = 0; index < bitLength; ++index)
            {
                _bitMaps[index] = 1 << bitLength - 1 - index;
            }

            BitLength = bitLength;
        }

        public BinaryDecisionDiagram Or(BinaryDecisionDiagram constraint1, BinaryDecisionDiagram constraint2)
        {
            if (constraint1 == BinaryDecisionDiagram.False)
                return constraint2;
            if (constraint2 == BinaryDecisionDiagram.False)
                return constraint1;
            if (constraint1 == BinaryDecisionDiagram.True || constraint2 == BinaryDecisionDiagram.True)
                return BinaryDecisionDiagram.True;

            var key = MkApplyKey(constraint1, constraint2);
            if (_orCache.TryGetValue(key, out var bdd))
                return bdd;

            if (constraint2.Ordinal < constraint1.Ordinal)
            {
                var trueCase = Or(constraint1, Restrict(constraint2.Ordinal, true, constraint2));
                var falseCase = Or(constraint1, Restrict(constraint2.Ordinal, false, constraint2));
                bdd = trueCase == falseCase
                    ? trueCase
                    : new BinaryDecisionDiagram(GetNextId(), constraint2.Ordinal, trueCase, falseCase);
            }
            else if (constraint1.Ordinal < constraint2.Ordinal)
            {
                var trueCase = Or(Restrict(constraint1.Ordinal, true, constraint1), constraint2);
                var falseCase = Or(Restrict(constraint1.Ordinal, false, constraint1), constraint2);
                bdd = trueCase == falseCase
                    ? trueCase
                    : new BinaryDecisionDiagram(GetNextId(), constraint1.Ordinal, trueCase, falseCase);
            }
            else
            {
                var trueCase = Or(Restrict(constraint1.Ordinal, true, constraint1), Restrict(constraint1.Ordinal, true, constraint2));
                var falseCase = Or(Restrict(constraint1.Ordinal, false, constraint1), Restrict(constraint1.Ordinal, false, constraint2));
                bdd = trueCase == falseCase
                    ? trueCase
                    : new BinaryDecisionDiagram(GetNextId(), constraint1.Ordinal, trueCase, falseCase);
            }
            _orCache[key] = bdd;
            return bdd;
        }

        public BinaryDecisionDiagram And(BinaryDecisionDiagram constraint1, BinaryDecisionDiagram constraint2)
        {
            if (constraint1 == BinaryDecisionDiagram.True)
                return constraint2;
            if (constraint2 == BinaryDecisionDiagram.True)
                return constraint1;
            if (constraint1 == BinaryDecisionDiagram.False || constraint2 == BinaryDecisionDiagram.False)
                return BinaryDecisionDiagram.False;

            var key = MkApplyKey(constraint1, constraint2);
            if (_andCache.TryGetValue(key, out var bdd))
                return bdd;

            if (constraint2.Ordinal < constraint1.Ordinal)
            {
                var trueCase = And(constraint1, Restrict(constraint2.Ordinal, true, constraint2));
                var falseCase = And(constraint1, Restrict(constraint2.Ordinal, false, constraint2));
                bdd = trueCase == falseCase
                    ? trueCase
                    : new BinaryDecisionDiagram(GetNextId(), constraint2.Ordinal, trueCase, falseCase);
            }
            else if (constraint1.Ordinal < constraint2.Ordinal)
            {
                var trueCase = And(Restrict(constraint1.Ordinal, true, constraint1), constraint2);
                var falseCase = And(Restrict(constraint1.Ordinal, false, constraint1), constraint2);
                bdd = trueCase == falseCase
                    ? trueCase
                    : new BinaryDecisionDiagram(GetNextId(), constraint1.Ordinal, trueCase, falseCase);
            }
            else
            {
                var trueCase = And(Restrict(constraint1.Ordinal, true, constraint1), Restrict(constraint1.Ordinal, true, constraint2));
                var falseCase = And(Restrict(constraint1.Ordinal, false, constraint1), Restrict(constraint1.Ordinal, false, constraint2));
                bdd = trueCase == falseCase
                    ? trueCase
                    : new BinaryDecisionDiagram(GetNextId(), constraint1.Ordinal, trueCase, falseCase);
            }
            _andCache[key] = bdd;
            return bdd;
        }

        public BinaryDecisionDiagram Not(BinaryDecisionDiagram constraint)
        {
            if (constraint == BinaryDecisionDiagram.False)
                return BinaryDecisionDiagram.True;
            if (constraint == BinaryDecisionDiagram.True)
                return BinaryDecisionDiagram.False;
            if (_notCache.TryGetValue(constraint, out var cachedBdd))
                return cachedBdd;

            var result = new BinaryDecisionDiagram(GetNextId(), constraint.Ordinal, Not(constraint.TrueCase), Not(constraint.FalseCase));
            _notCache[constraint] = result;
            return result;
        }

        public BinaryDecisionDiagram And(IEnumerable<BinaryDecisionDiagram> constraints)
        {
            var result = BinaryDecisionDiagram.True;
            foreach (var condition in constraints)
                result = And(result, condition);
            return result;
        }

        public BinaryDecisionDiagram Or(IEnumerable<BinaryDecisionDiagram> constraints)
        {
            var result = BinaryDecisionDiagram.False;
            foreach (var condition in constraints)
                result = Or(result, condition);
            return result;
        }

        public BinaryDecisionDiagram True => BinaryDecisionDiagram.True;

        public BinaryDecisionDiagram False => BinaryDecisionDiagram.False;

        public BinaryDecisionDiagram CreateFromInt(int n)
        {
            if (_intCache.TryGetValue(n, out var cachedBdd))
                return cachedBdd;

            var result = BinaryDecisionDiagram.True;
            for (var bit = BitLength - 1; bit >= 0; --bit)
            {
                result = (n & _bitMaps[bit]) != 0
                    ? new BinaryDecisionDiagram(GetNextId(), bit, result, BinaryDecisionDiagram.False)
                    : new BinaryDecisionDiagram(GetNextId(), bit, BinaryDecisionDiagram.False, result);
            }
            _intCache[n] = result;
            return result;
        }

        public BinaryDecisionDiagram CreateCharConstraint(bool caseInsensitive, char c)
        {
            if (caseInsensitive)
            {
                if (char.IsUpper(c))
                    return Or(CreateFromInt(c), CreateFromInt(char.ToLower(c)));
                if (char.IsLower(c))
                    return Or(CreateFromInt(c), CreateFromInt(char.ToUpper(c)));
            }
            return CreateFromInt(c);
        }

        public BinaryDecisionDiagram CreateForRange(int start, int end)
        {
            var result = BinaryDecisionDiagram.False;
            for (var i = start; i <= end; ++i)
                result = Or(result, CreateFromInt(i));
            return result;
        }

        public BinaryDecisionDiagram CreateRangeConstraint(bool caseInsensitive, char lower, char upper)
        {
            if (BitLength == 7)
                return CreateRangedConstraint(caseInsensitive, lower < sbyte.MaxValue ? lower : '\x007F', upper < sbyte.MaxValue ? upper : '\x007F');
            if (BitLength == 8)
                return CreateRangedConstraint(caseInsensitive, lower < byte.MaxValue ? lower : 'ÿ', upper < byte.MaxValue ? upper : 'ÿ');

            var lowerCharCode = (int)lower;
            var upperCharCode = (int)upper;
            if (upperCharCode - lowerCharCode < ushort.MaxValue - upperCharCode + lowerCharCode || caseInsensitive)
                return CreateRangedConstraint(caseInsensitive, lower, upper);

            return Not(
                Or(
                    lower > 0
                        ? CreateRangedConstraint(caseInsensitive, char.MinValue, (char)(lower - 1U))
                        : BinaryDecisionDiagram.False,
                    upper < ushort.MaxValue
                        ? CreateRangedConstraint(caseInsensitive, (char)(upper + 1U), char.MaxValue)
                        : BinaryDecisionDiagram.False));
        }

        public BinaryDecisionDiagram CreateRangedConstraint(bool ignoreCase, char c, char d)
        {
            var result = BinaryDecisionDiagram.False;
            for (var c1 = c; c1 <= d; ++c1)
                result = Or(result, CreateCharConstraint(ignoreCase, c1));
            return result;
        }

        public BinaryDecisionDiagram CreateFromRanges(IEnumerable<int[]> ranges)
        {
            var result = BinaryDecisionDiagram.False;
            foreach (var range in ranges)
                result = Or(result, CreateForRange(range[0], range[1]));
            return result;
        }

        public BinaryDecisionDiagram CreateRangedConstraint(bool caseInsensitive, IEnumerable<char[]> ranges)
        {
            var result = BinaryDecisionDiagram.False;
            foreach (var range in ranges)
                result = Or(result, CreateRangeConstraint(caseInsensitive, range[0], range[1]));
            return result;
        }

        public char GenerateMember(Chooser chooser, BinaryDecisionDiagram bdd)
        {
            int num = 0;
            for (var index = 0; index < BitLength; ++index)
            {
                if (index < bdd.Ordinal)
                {
                    num |= chooser.ChooseBoolean() ? _bitMaps[index] : 0;
                }
                else if (bdd.FalseCase == BinaryDecisionDiagram.False)
                {
                    num |= _bitMaps[index];
                    bdd = bdd.TrueCase;
                }
                else if (bdd.TrueCase == BinaryDecisionDiagram.False)
                {
                    bdd = bdd.FalseCase;
                }
                else if (chooser.ChooseBoolean())
                {
                    num |= _bitMaps[index];
                    bdd = bdd.TrueCase;
                }
                else
                {
                    bdd = bdd.FalseCase;
                }
            }
            return (char)num;
        }

        private int BitLength { get; }

        private int GetNextId() => _id++;

        private static long MkRestrictKey(int value, bool makeTrue, BinaryDecisionDiagram bdd)
        {
            return ((long)bdd.Id << 16) + (value << 4) + (makeTrue ? 1L : 0L);
        }

        private static BddPair MkApplyKey(BinaryDecisionDiagram bdd1, BinaryDecisionDiagram bdd2) => new BddPair(bdd1, bdd2);

        private BinaryDecisionDiagram Restrict(int value, bool makeTrue, BinaryDecisionDiagram bdd)
        {
            var key = MkRestrictKey(value, makeTrue, bdd);
            if (_restrictCache.TryGetValue(key, out var cachedBdd))
                return cachedBdd;

            BinaryDecisionDiagram result;
            if (value < bdd.Ordinal)
            {
                result = bdd;
            }
            else if (bdd.Ordinal < value)
            {
                var trueCase = Restrict(value, makeTrue, bdd.TrueCase);
                var falseCase = Restrict(value, makeTrue, bdd.FalseCase);
                result = trueCase == falseCase
                    ? trueCase
                    : (falseCase != bdd.FalseCase || trueCase != bdd.TrueCase
                        ? new BinaryDecisionDiagram(GetNextId(), bdd.Ordinal, trueCase, falseCase)
                        : bdd);
            }
            else
            {
                result = makeTrue ? bdd.TrueCase : bdd.FalseCase;
            }

            _restrictCache[key] = result;
            return result;
        }

        private int _id = 2;
        private readonly int[] _bitMaps;

        // caches
        private readonly IDictionary<long, BinaryDecisionDiagram> _restrictCache = new Dictionary<long, BinaryDecisionDiagram>();
        private readonly IDictionary<BddPair, BinaryDecisionDiagram> _orCache = new Dictionary<BddPair, BinaryDecisionDiagram>();
        private readonly IDictionary<BddPair, BinaryDecisionDiagram> _andCache = new Dictionary<BddPair, BinaryDecisionDiagram>();
        private readonly IDictionary<int, BinaryDecisionDiagram> _intCache = new Dictionary<int, BinaryDecisionDiagram>();
        private readonly IDictionary<BinaryDecisionDiagram, BinaryDecisionDiagram> _notCache = new Dictionary<BinaryDecisionDiagram, BinaryDecisionDiagram>();

        private class BddPair : IEquatable<BddPair>
        {
            internal BddPair(BinaryDecisionDiagram a, BinaryDecisionDiagram b)
            {
                First = a;
                Second = b;
            }

            public BinaryDecisionDiagram First { get; }

            public BinaryDecisionDiagram Second { get; }

            public override int GetHashCode() => First.Id + (Second.Id << 1);

            public override bool Equals(object obj)
            {
                if (obj is null)
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                return obj is BddPair pair && Equals(pair);
            }

            public bool Equals(BddPair other)
            {
                if (other is null)
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                return First == other.First
                    && Second == other.Second;
            }
        }
    }
}
