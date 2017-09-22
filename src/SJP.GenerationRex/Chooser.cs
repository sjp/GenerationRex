using System;

namespace SJP.GenerationRex
{
    internal class Chooser
    {
        public Chooser()
        {
            _seed = new Random().Next();
            _rand = new Random(_seed);
        }

        public Chooser(int randomSeed)
        {
            _seed = randomSeed;
            _rand = new Random(_seed);
        }

        public int Choose(int n) => _rand.Next(0, n);

        public bool ChooseBoolean() => _rand.Next(0, 2) == 1;

        private readonly Random _rand;
        private readonly int _seed;
    }
}
