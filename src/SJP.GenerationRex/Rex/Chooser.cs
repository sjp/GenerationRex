using System;

namespace SJP.GenerationRex
{
    internal class Chooser
    {
        public Chooser()
        {
            seed = new Random().Next();
            rand = new Random(seed);
        }

        public int RandomSeed
        {
            get => seed;
            set
            {
                seed = value;
                rand = new Random(value);
            }
        }

        public int Choose(int n) => rand.Next(0, n);

        public bool ChooseBoolean() => rand.Next(0, 2) == 1;

        private Random rand;
        private int seed;
    }
}
