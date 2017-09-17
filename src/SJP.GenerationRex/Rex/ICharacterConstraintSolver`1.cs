using System.Collections.Generic;

namespace SJP.GenerationRex
{
    internal interface ICharacterConstraintSolver<S>
    {
        S MkOr(S constraint1, S constraint2);

        S MkOr(IEnumerable<S> constraints);

        S MkAnd(S constraint1, S constraint2);

        S MkAnd(IEnumerable<S> constraints);

        S MkNot(S constraint);

        S True { get; }

        S False { get; }

        S MkRangeConstraint(bool caseInsensitive, char lower, char upper);

        S MkCharConstraint(bool caseInsensitive, char c);

        S MkRangesConstraint(bool caseInsensitive, IEnumerable<char[]> ranges);
    }
}
