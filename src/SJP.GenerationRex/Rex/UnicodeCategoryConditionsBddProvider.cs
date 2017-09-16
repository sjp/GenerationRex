namespace Rex
{
    internal class UnicodeCategoryConditionsBddProvider : IUnicodeCategoryConditions<BinaryDecisionDiagram>
    {
        private BinaryDecisionDiagram[] catConditions = new BinaryDecisionDiagram[30];
        private BinaryDecisionDiagramBuilder bddb;
        private BinaryDecisionDiagram whiteSpaceCondition;
        private BinaryDecisionDiagram wordLetterCondition;

        internal UnicodeCategoryConditionsBddProvider(BinaryDecisionDiagramBuilder bddb)
        {
            this.bddb = bddb;
            InitializeUnicodeCategoryDefinitions();
        }

        private void InitializeUnicodeCategoryDefinitions()
        {
            for (int index = 0; index < 30; ++index)
                catConditions[index] = bddb.DeserializeCompact(UnicodeCategoryRanges.UnicodeBdd[index]);
            whiteSpaceCondition = bddb.DeserializeCompact(UnicodeCategoryRanges.UnicodeWhitespaceBdd);
            wordLetterCondition = bddb.DeserializeCompact(UnicodeCategoryRanges.UnicodeWordCharacterBdd);
        }

        public BinaryDecisionDiagram CategoryCondition(int cat) => catConditions[cat];

        public BinaryDecisionDiagram WhiteSpaceCondition => whiteSpaceCondition;

        public BinaryDecisionDiagram WordLetterCondition => wordLetterCondition;
    }
}
