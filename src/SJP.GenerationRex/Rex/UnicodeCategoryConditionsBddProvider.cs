namespace SJP.GenerationRex
{
    internal class UnicodeCategoryConditionsBddProvider : IUnicodeCategoryConditions<BDD>
    {
        private BDD[] catConditions = new BDD[30];
        private BddBuilder bddb;
        private BDD whiteSpaceCondition;
        private BDD wordLetterCondition;

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
                    this.catConditions[index] = UnicodeCategoryRanges.ASCIIBdd[index] != null ? this.bddb.DeserializeCompact(UnicodeCategoryRanges.ASCIIBdd[index]) : BDD.False;
                this.whiteSpaceCondition = this.bddb.DeserializeCompact(UnicodeCategoryRanges.ASCIIWhitespaceBdd);
                this.wordLetterCondition = this.bddb.DeserializeCompact(UnicodeCategoryRanges.ASCIIWordCharacterBdd);
            }
            else if (this.bddb.NrOfBits == 8)
            {
                for (int index = 0; index < 30; ++index)
                    this.catConditions[index] = UnicodeCategoryRanges.CP437Bdd[index] != null ? this.bddb.DeserializeCompact(UnicodeCategoryRanges.CP437Bdd[index]) : BDD.False;
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

        public BDD CategoryCondition(int cat)
        {
            return this.catConditions[cat];
        }

        public BDD WhiteSpaceCondition
        {
            get
            {
                return this.whiteSpaceCondition;
            }
        }

        public BDD WordLetterCondition
        {
            get
            {
                return this.wordLetterCondition;
            }
        }
    }
}
