namespace SJP.GenerationRex
{
    internal class MOVE<S>
    {
        public readonly int SourceState;
        public readonly int TargetState;
        public readonly S Condition;

        private MOVE(int sourceState, int targetState, S condition)
        {
            this.SourceState = sourceState;
            this.TargetState = targetState;
            this.Condition = condition;
        }

        private MOVE(int sourceState, int targetState)
        {
            this.SourceState = sourceState;
            this.TargetState = targetState;
            this.Condition = default(S);
        }

        public static MOVE<S> T(int sourceState, int targetState, S condition)
        {
            return new MOVE<S>(sourceState, targetState, condition);
        }

        public static MOVE<S> Epsilon(int sourceState, int targetState)
        {
            return new MOVE<S>(sourceState, targetState);
        }

        public bool IsEpsilon
        {
            get
            {
                return (object)this.Condition == null;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MOVE<S>))
                return false;
            MOVE<S> move = (MOVE<S>)obj;
            if (move.SourceState != this.SourceState || move.TargetState != this.TargetState)
                return false;
            if ((object)move.Condition == null && (object)this.Condition == null)
                return true;
            if ((object)move.Condition != null)
                return move.Condition.Equals((object)this.Condition);
            return false;
        }

        public override int GetHashCode()
        {
            return this.SourceState + this.TargetState * 2 + ((object)this.Condition == null ? 0 : this.Condition.GetHashCode());
        }

        public override string ToString()
        {
            return "(" + (object)this.SourceState + "," + ((object)this.Condition == null ? (object)"" : (object)(this.Condition.ToString() + ",")) + (object)this.TargetState + ")";
        }
    }
}
