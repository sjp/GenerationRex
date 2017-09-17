namespace SJP.GenerationRex
{
    internal interface IUnicodeCategoryConditions<S>
    {
        S CategoryCondition(int cat);

        S WhiteSpaceCondition { get; }

        S WordLetterCondition { get; }
    }
}
