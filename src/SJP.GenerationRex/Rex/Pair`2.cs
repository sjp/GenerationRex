namespace SJP.GenerationRex
{
    internal class Pair<S, T>
    {
        public readonly S First;
        public readonly T Second;

        public Pair(S first, T second)
        {
            this.First = first;
            this.Second = second;
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", (object)this.First, (object)this.Second);
        }

        public override bool Equals(object obj)
        {
            Pair<S, T> pair = (Pair<S, T>)obj;
            if (this.First.Equals((object)pair.First))
                return this.Second.Equals((object)pair.Second);
            return false;
        }

        public override int GetHashCode()
        {
            return this.First.GetHashCode() + 2 * this.Second.GetHashCode();
        }
    }
}
