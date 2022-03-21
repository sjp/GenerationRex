using System.Collections.Generic;

namespace SJP.GenerationRex;

internal interface ICharacterConstraintSolver<TConstraint>
{
    TConstraint Or(TConstraint constraint1, TConstraint constraint2);

    TConstraint Or(IEnumerable<TConstraint> constraints);

    TConstraint And(TConstraint constraint1, TConstraint constraint2);

    TConstraint And(IEnumerable<TConstraint> constraints);

    TConstraint Not(TConstraint constraint);

    TConstraint True { get; }

    TConstraint False { get; }

    TConstraint CreateRangeConstraint(bool caseInsensitive, char lower, char upper);

    TConstraint CreateCharConstraint(bool caseInsensitive, char c);

    TConstraint CreateRangedConstraint(bool caseInsensitive, IEnumerable<char[]> ranges);
}
