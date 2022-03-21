using System.Globalization;

namespace SJP.GenerationRex;

internal interface IUnicodeCategoryConditions<TConstraint>
{
    TConstraint CategoryCondition(UnicodeCategory category);

    TConstraint WhiteSpaceCondition { get; }

    TConstraint WordLetterCondition { get; }
}
