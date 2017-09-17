using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace SJP.GenerationRex
{
    internal class ConsList<E> : IEnumerable<E>
    {
        public E car;
        public ConsList<E> cdr;
        private ConsList<E> last;

        public ConsList(E car, ConsList<E> cdr)
        {
            this.car = car;
            this.cdr = cdr;
            if (cdr == null)
                last = this;
            else
                last = cdr.last;
        }

        public int Length
        {
            get
            {
                int num = 1;
                for (var cdr = this.cdr; cdr != null; cdr = cdr.cdr)
                    ++num;
                return num;
            }
        }

        public E[] ToArray()
        {
            var eArray = new E[Length];
            eArray[0] = car;
            ConsList<E> cdr = this.cdr;
            int num = 1;
            for (; cdr != null; cdr = cdr.cdr)
                eArray[num++] = cdr.car;
            return eArray;
        }

        public ConsList(E elem)
          : this(elem, null)
        {
        }

        public static ConsList<E> Create(IEnumerable<E> elems)
        {
            ConsList<E> consList = null;
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
            if (check(car))
                return true;
            if (cdr == null)
                return false;
            return cdr.Exists(check);
        }

        public static ConsList<E> RemoveAll(ConsList<E> list, Func<E, bool> check)
        {
            if (list == null)
                return null;
            if (check(list.car))
                return RemoveAll(list.cdr, check);
            return new ConsList<E>(list.car, RemoveAll(list.cdr, check));
        }

        public void DeleteAllFromRest(Func<E, bool> check)
        {
            var consList = this;
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
            var consList = new ConsList<E>(elem);
            last.cdr = consList;
            last = consList;
        }

        public void Append(ConsList<E> l)
        {
            if (l == null)
                return;
            last.cdr = l;
            last = l.last;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("(");
            AddElems(sb);
            sb.Append(")");
            return sb.ToString();
        }

        public ConsList<E> Reverse()
        {
            var cdr1 = new ConsList<E>(car);
            for (ConsList<E> cdr2 = cdr; cdr2 != null; cdr2 = cdr2.cdr)
                cdr1 = new ConsList<E>(cdr2.car, cdr1);
            return cdr1;
        }

        private void AddElems(StringBuilder sb)
        {
            sb.Append(car.ToString());
            if (cdr == null)
                return;
            sb.Append(",");
            cdr.AddElems(sb);
        }

        public IEnumerator<E> GetEnumerator() => new SimpleListEnumerator<E>(this);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static IEnumerable<ConsList<Pair<bool, E>>> GenerateChoiceLists(ConsList<E> tl)
        {
            if (tl != null)
            {
                foreach (var choiceList in GenerateChoiceLists(tl.cdr))
                {
                    yield return new ConsList<Pair<bool, E>>(new Pair<bool, E>(true, tl.car), choiceList);
                    yield return new ConsList<Pair<bool, E>>(new Pair<bool, E>(false, tl.car), choiceList);
                }
            }
            else
                yield return null;
        }

        private class SimpleListEnumerator<E1> : IEnumerator<E1>
        {
            internal SimpleListEnumerator(ConsList<E1> tl)
            {
                _list = tl;
            }

            public E1 Current
            {
                get
                {
                    if (_list == null || !initialized)
                        throw new InvalidOperationException(nameof(Current) + " is undefined");

                    return _list.car;
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _list = null;
            }

            public bool MoveNext()
            {
                if (_list == null)
                    return false;

                if (!initialized)
                {
                    initialized = true;
                    return true;
                }

                _list = _list.cdr;
                return _list != null;
            }

            public void Reset() => throw new NotSupportedException();

            private ConsList<E1> _list;
            private bool initialized;
        }
    }
}
