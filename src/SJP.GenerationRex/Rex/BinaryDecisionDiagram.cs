using System.Collections.Generic;

namespace Rex
{
    /// <summary>
    /// Represents a binary decision diagram
    /// </summary>
    internal class BinaryDecisionDiagram
    {
        public static BinaryDecisionDiagram True { get; } = new BinaryDecisionDiagram(1, short.MaxValue);
        public static BinaryDecisionDiagram False { get; } = new BinaryDecisionDiagram(0, short.MaxValue);
        internal BinaryDecisionDiagram T;
        internal BinaryDecisionDiagram F;
        public readonly int Id;
        public readonly int Ordinal;

        public BinaryDecisionDiagram TrueCase => T;

        public BinaryDecisionDiagram FalseCase => F;

        internal BinaryDecisionDiagram(int id, int x)
        {
            Id = id;
            Ordinal = x;
        }

        internal BinaryDecisionDiagram(int id, int x, BinaryDecisionDiagram t, BinaryDecisionDiagram f)
        {
            Id = id;
            Ordinal = x;
            T = t;
            F = f;
        }

        public int CalculateSize()
        {
            if (T == null)
                return 1;
            var bddSet = new HashSet<BinaryDecisionDiagram>();
            var bddStack = new Stack<BinaryDecisionDiagram>();
            bddStack.Push(this);
            bddSet.Add(this);
            while (bddStack.Count > 0)
            {
                BinaryDecisionDiagram bdd = bddStack.Pop();
                if (bdd != False && bdd != True)
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
            return Id;
        }
    }
}
