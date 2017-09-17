using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SJP.GenerationRex
{
    internal class ConsList<E> : IEnumerable<E>, IEnumerable
    {
        public E car;
        public ConsList<E> cdr;
        private ConsList<E> last;

        public ConsList(E car, ConsList<E> cdr)
        {
            this.car = car;
            this.cdr = cdr;
            if (cdr == null)
                this.last = this;
            else
                this.last = cdr.last;
        }

        public int Length
        {
            get
            {
                int num = 1;
                for (ConsList<E> cdr = this.cdr; cdr != null; cdr = cdr.cdr)
                    ++num;
                return num;
            }
        }

        public E[] ToArray()
        {
            E[] eArray = new E[this.Length];
            eArray[0] = this.car;
            ConsList<E> cdr = this.cdr;
            int num = 1;
            for (; cdr != null; cdr = cdr.cdr)
                eArray[num++] = cdr.car;
            return eArray;
        }

        public ConsList(E elem)
          : this(elem, (ConsList<E>)null)
        {
        }

        public static ConsList<E> Create(IEnumerable<E> elems)
        {
            ConsList<E> consList = (ConsList<E>)null;
            foreach (E elem in elems)
            {
                if (consList == null)
                    consList = new ConsList<E>(elem);
                else
                    consList.Add(elem);
            }
            return consList;
        }

        public bool Exists(Func<E, bool> check)
        {
            if (check(this.car))
                return true;
            if (this.cdr == null)
                return false;
            return this.cdr.Exists(check);
        }

        public static ConsList<E> RemoveAll(ConsList<E> list, Func<E, bool> check)
        {
            if (list == null)
                return (ConsList<E>)null;
            if (check(list.car))
                return ConsList<E>.RemoveAll(list.cdr, check);
            return new ConsList<E>(list.car, ConsList<E>.RemoveAll(list.cdr, check));
        }

        public void DeleteAllFromRest(Func<E, bool> check)
        {
            ConsList<E> consList = this;
            ConsList<E> cdr = this.cdr;
            while (cdr != null)
            {
                if (check(cdr.car))
                {
                    consList.cdr = cdr.cdr;
                    cdr = cdr.cdr;
                }
                else
                {
                    consList = cdr;
                    cdr = cdr.cdr;
                }
            }
        }

        public void Add(E elem)
        {
            ConsList<E> consList = new ConsList<E>(elem);
            this.last.cdr = consList;
            this.last = consList;
        }

        public void Append(ConsList<E> l)
        {
            if (l == null)
                return;
            this.last.cdr = l;
            this.last = l.last;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("(");
            this.AddElems(sb);
            sb.Append(")");
            return sb.ToString();
        }

        public ConsList<E> Reverse()
        {
            ConsList<E> cdr1 = new ConsList<E>(this.car);
            for (ConsList<E> cdr2 = this.cdr; cdr2 != null; cdr2 = cdr2.cdr)
                cdr1 = new ConsList<E>(cdr2.car, cdr1);
            return cdr1;
        }

        private void AddElems(StringBuilder sb)
        {
            sb.Append(this.car.ToString());
            if (this.cdr == null)
                return;
            sb.Append(",");
            this.cdr.AddElems(sb);
        }

        public IEnumerator<E> GetEnumerator()
        {
            return (IEnumerator<E>)new ConsList<E>.SimpleListEnumerator<E>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }

        public static IEnumerable<ConsList<Pair<bool, E>>> GenerateChoiceLists(ConsList<E> tl)
        {
            if (tl != null)
            {
                foreach (ConsList<Pair<bool, E>> choiceList in ConsList<E>.GenerateChoiceLists(tl.cdr))
                {
                    yield return new ConsList<Pair<bool, E>>(new Pair<bool, E>(true, tl.car), choiceList);
                    yield return new ConsList<Pair<bool, E>>(new Pair<bool, E>(false, tl.car), choiceList);
                }
            }
            else
                yield return (ConsList<Pair<bool, E>>)null;
        }

        private class SimpleListEnumerator<E1> : IEnumerator<E1>, IDisposable, IEnumerator
        {
            private ConsList<E1> tl;
            private bool initialized;

            internal SimpleListEnumerator(ConsList<E1> tl)
            {
                this.tl = tl;
            }

            public E1 Current
            {
                get
                {
                    if (this.tl == null || !this.initialized)
                        throw new InvalidOperationException("Current is undefined");
                    return this.tl.car;
                }
            }

            public void Dispose()
            {
                this.tl = (ConsList<E1>)null;
            }

            object IEnumerator.Current
            {
                get
                {
                    return (object)this.Current;
                }
            }

            public bool MoveNext()
            {
                if (this.tl == null)
                    return false;
                if (!this.initialized)
                {
                    this.initialized = true;
                    return true;
                }
                this.tl = this.tl.cdr;
                return this.tl != null;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }
        }
    }
}
