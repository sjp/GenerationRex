namespace SJP.GenerationRex
{
    internal interface IUnicodeCategoryConditions<TConstraint>
    {
        TConstraint CategoryCondition(int cat);

        TConstraint WhiteSpaceCondition { get; }

        TConstraint WordLetterCondition { get; }
    }
}
