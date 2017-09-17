using System;

namespace SJP.GenerationRex
{
    internal class Chooser
    {
        private Random rand;
        private int seed;

        public Chooser()
        {
            this.seed = new Random().Next();
            this.rand = new Random(this.seed);
        }

        public int RandomSeed
        {
            get
            {
                return this.seed;
            }
            set
            {
                this.seed = value;
                this.rand = new Random(value);
            }
        }

        public int Choose(int n)
        {
            return this.rand.Next(0, n);
        }

        public bool ChooseTrueOrFalse()
        {
            return this.rand.Next(0, 2) == 1;
        }
    }
}
