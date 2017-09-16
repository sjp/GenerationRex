using System.Collections.Generic;

namespace Rex
{
    internal interface ICharacterConstraintSolver<TConstraint>
    {
        TConstraint MkOr(TConstraint constraint1, TConstraint constraint2);

        TConstraint MkOr(IEnumerable<TConstraint> constraints);

        TConstraint MkAnd(TConstraint constraint1, TConstraint constraint2);

        TConstraint MkAnd(IEnumerable<TConstraint> constraints);

        TConstraint MkNot(TConstraint constraint);

        TConstraint True { get; }

        TConstraint False { get; }

        TConstraint MkRangeConstraint(bool caseInsensitive, char lower, char upper);

        TConstraint MkCharConstraint(bool caseInsensitive, char c);

        TConstraint MkRangesConstraint(bool caseInsensitive, IEnumerable<char[]> ranges);
    }
}
