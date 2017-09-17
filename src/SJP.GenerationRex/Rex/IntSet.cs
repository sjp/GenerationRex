using System;
using System.Collections.Generic;
using System.Text;

namespace SJP.GenerationRex
{
    internal class IntSet
    {
        private int choice = int.MaxValue;
        private HashSet<int> elems;
        private string repr;

        private string Repr
        {
            get
            {
                if (this.repr == null)
                    this.repr = this.MkString();
                return this.repr;
            }
        }

        internal IntSet(IEnumerable<int> elems)
        {
            this.elems = new HashSet<int>();
            foreach (int elem in elems)
            {
                this.elems.Add(elem);
                this.choice = Math.Min(elem, this.choice);
            }
        }

        public override int GetHashCode()
        {
            return this.Repr.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.Repr.Equals(((IntSet)obj).Repr);
        }

        public override string ToString()
        {
            return this.Repr;
        }

        internal int Choice
        {
            get
            {
                return this.choice;
            }
        }

        private string MkString()
        {
            if (this.elems.Count == 0)
                return "{}";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("{");
            bool flag = false;
            List<int> intList = new List<int>((IEnumerable<int>)this.elems);
            intList.Sort();
            foreach (int num in intList)
            {
                if (flag)
                    stringBuilder.Append(",");
                else
                    flag = true;
                stringBuilder.Append(num);
            }
            stringBuilder.Append("}");
            return stringBuilder.ToString();
        }

        internal bool Contains(int elem)
        {
            return this.elems.Contains(elem);
        }

        internal IntSet Intersect(IEnumerable<int> other)
        {
            HashSet<int> intSet = new HashSet<int>((IEnumerable<int>)this.elems);
            intSet.IntersectWith(other);
            return new IntSet((IEnumerable<int>)intSet);
        }

        internal bool IsSingleton
        {
            get
            {
                return this.elems.Count == 1;
            }
        }

        internal IEnumerable<int> EnumerateMembers()
        {
            return (IEnumerable<int>)this.elems;
        }
    }
}
