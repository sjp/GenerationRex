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

        public int RandomSeed
        {
            get => _seed;
            set
            {
                _seed = value;
                _rand = new Random(value);
            }
        }

        public int Choose(int n) => _rand.Next(0, n);

        public bool ChooseBoolean() => _rand.Next(0, 2) == 1;

        private Random _rand;
        private int _seed;
    }
}
