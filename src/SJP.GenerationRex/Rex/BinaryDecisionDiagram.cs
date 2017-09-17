using System.Collections.Generic;

namespace SJP.GenerationRex
{
    /// <summary>
    /// Represents a binary decision diagram
    /// </summary>
    public class BinaryDecisionDiagram
    {
        public BinaryDecisionDiagram(int id, int x)
        {
            Id = id;
            Ordinal = x;
        }

        public BinaryDecisionDiagram(int id, int x, BinaryDecisionDiagram trueCase, BinaryDecisionDiagram falseCase)
        {
            Id = id;
            Ordinal = x;
            TrueCase = trueCase;
            FalseCase = falseCase;
        }

        public int Id { get; }

        public int Ordinal { get; }

        public static BinaryDecisionDiagram True { get; } = new BinaryDecisionDiagram(1, short.MaxValue);

        public static BinaryDecisionDiagram False { get; } = new BinaryDecisionDiagram(0, short.MaxValue);

        public BinaryDecisionDiagram TrueCase { get; set; }

        public BinaryDecisionDiagram FalseCase { get; set; }

        public int CalculateSize()
        {
            if (TrueCase == null)
                return 1;

            var bddSet = new HashSet<BinaryDecisionDiagram>();
            var bddStack = new Stack<BinaryDecisionDiagram>();
            bddStack.Push(this);
            bddSet.Add(this);
            while (bddStack.Count > 0)
            {
                var bdd = bddStack.Pop();
                if (bdd == False || bdd == True)
                    continue;

                if (!bddSet.Contains(bdd.TrueCase))
                {
                    bddSet.Add(bdd.TrueCase);
                    bddStack.Push(bdd.TrueCase);
                }
                if (!bddSet.Contains(bdd.FalseCase))
                {
                    bddSet.Add(bdd.FalseCase);
                    bddStack.Push(bdd.FalseCase);
                }
            }
            return bddSet.Count;
        }

        public override int GetHashCode() => Id;
    }
}
