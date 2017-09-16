namespace Rex
{
    internal interface IUnicodeCategoryConditions<TCondition>
    {
        TCondition CategoryCondition(int cat);

        TCondition WhiteSpaceCondition { get; }

        TCondition WordLetterCondition { get; }
    }
}
