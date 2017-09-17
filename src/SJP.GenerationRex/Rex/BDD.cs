using System.Collections.Generic;

namespace SJP.GenerationRex
{
    internal class BDD
    {
        public static readonly BDD True = new BDD(1, (int)short.MaxValue);
        public static readonly BDD False = new BDD(0, (int)short.MaxValue);
        internal BDD T;
        internal BDD F;
        public readonly int Id;
        public readonly int Ordinal;

        public BDD TrueCase
        {
            get
            {
                return this.T;
            }
        }

        public BDD FalseCase
        {
            get
            {
                return this.F;
            }
        }

        internal BDD(int id, int x)
        {
            this.Id = id;
            this.Ordinal = x;
        }

        internal BDD(int id, int x, BDD t, BDD f)
        {
            this.Id = id;
            this.Ordinal = x;
            this.T = t;
            this.F = f;
        }

        public int CalculateSize()
        {
            if (this.T == null)
                return 1;
            HashSet<BDD> bddSet = new HashSet<BDD>();
            Stack<BDD> bddStack = new Stack<BDD>();
            bddStack.Push(this);
            bddSet.Add(this);
            while (bddStack.Count > 0)
            {
                BDD bdd = bddStack.Pop();
                if (bdd != BDD.False && bdd != BDD.True)
                {
                    if (!bddSet.Contains(bdd.T))
                    {
                        bddSet.Add(bdd.T);
                        bddStack.Push(bdd.T);
                    }
                    if (!bddSet.Contains(bdd.F))
                    {
                        bddSet.Add(bdd.F);
                        bddStack.Push(bdd.F);
                    }
                }
            }
            return bddSet.Count;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }
    }
}
