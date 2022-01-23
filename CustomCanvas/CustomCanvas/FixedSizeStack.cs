using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoorMansPaint.CustomCanvas
{
    public class FixedSizeStack<T>
    {
        protected LinkedList<T> _list = new LinkedList<T>();
        protected int _capacity;
        public int Capacity
        {
            get { return _capacity;}
            set
            {
                _capacity = value;
                while (_list.Count > _capacity) _list.RemoveFirst();
            }
        }

        public FixedSizeStack(int capacity)
        {
            _capacity = capacity;
        }

        public int Count => _list.Count;

        public void Push(T item)
        {
            _list.AddLast(item);
            if (_list.Count > _capacity) _list.RemoveFirst();
        }

        public T? Pop()
        {
            T? last = _list.Last();
            _list.RemoveLast();
            return last;
        }

        public void Clear()
        {
            _list.Clear();
        }
    }
}
