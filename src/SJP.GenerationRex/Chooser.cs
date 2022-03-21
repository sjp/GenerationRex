using System;

namespace SJP.GenerationRex;

internal class Chooser
{
    public Chooser()
    {
        var seed = new Random().Next();
        _rand = new Random(seed);
    }

    public Chooser(int randomSeed)
    {
        _rand = new Random(randomSeed);
    }

    public int Choose(int n) => _rand.Next(0, n);

    public bool ChooseBoolean() => _rand.Next(0, 2) == 1;

    private readonly Random _rand;
}
