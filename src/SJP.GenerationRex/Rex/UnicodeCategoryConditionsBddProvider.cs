namespace SJP.GenerationRex
{
    internal class UnicodeCategoryConditionsBddProvider : IUnicodeCategoryConditions<BinaryDecisionDiagram>
    {
        private BinaryDecisionDiagram[] catConditions = new BinaryDecisionDiagram[30];
        private BddBuilder bddb;
        private BinaryDecisionDiagram whiteSpaceCondition;
        private BinaryDecisionDiagram wordLetterCondition;

        internal UnicodeCategoryConditionsBddProvider(BddBuilder bddb)
        {
            this.bddb = bddb;
            this.InitializeUnicodeCategoryDefinitions();
        }

        private void InitializeUnicodeCategoryDefinitions()
        {
            if (this.bddb.NrOfBits == 7)
            {
                for (int index = 0; index < 30; ++index)
                    this.catConditions[index] = UnicodeCategoryRanges.ASCIIBdd[index] != null ? this.bddb.DeserializeCompact(UnicodeCategoryRanges.ASCIIBdd[index]) : BinaryDecisionDiagram.False;
                this.whiteSpaceCondition = this.bddb.DeserializeCompact(UnicodeCategoryRanges.ASCIIWhitespaceBdd);
                this.wordLetterCondition = this.bddb.DeserializeCompact(UnicodeCategoryRanges.ASCIIWordCharacterBdd);
            }
            else if (this.bddb.NrOfBits == 8)
            {
                for (int index = 0; index < 30; ++index)
                    this.catConditions[index] = UnicodeCategoryRanges.CP437Bdd[index] != null ? this.bddb.DeserializeCompact(UnicodeCategoryRanges.CP437Bdd[index]) : BinaryDecisionDiagram.False;
                this.whiteSpaceCondition = this.bddb.DeserializeCompact(UnicodeCategoryRanges.CP437WhitespaceBdd);
                this.wordLetterCondition = this.bddb.DeserializeCompact(UnicodeCategoryRanges.CP437WordCharacterBdd);
            }
            else
            {
                for (int index = 0; index < 30; ++index)
                    this.catConditions[index] = this.bddb.DeserializeCompact(UnicodeCategoryRanges.UnicodeBdd[index]);
                this.whiteSpaceCondition = this.bddb.DeserializeCompact(UnicodeCategoryRanges.UnicodeWhitespaceBdd);
                this.wordLetterCondition = this.bddb.DeserializeCompact(UnicodeCategoryRanges.UnicodeWordCharacterBdd);
            }
        }

        public BinaryDecisionDiagram CategoryCondition(int cat)
        {
            return this.catConditions[cat];
        }

        public BinaryDecisionDiagram WhiteSpaceCondition
        {
            get
            {
                return this.whiteSpaceCondition;
            }
        }

        public BinaryDecisionDiagram WordLetterCondition
        {
            get
            {
                return this.wordLetterCondition;
            }
        }
    }
}
