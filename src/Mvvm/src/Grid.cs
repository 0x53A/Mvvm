using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mvvm
{
    /// <summary>
    /// A 2D-Grid of elements
    /// </summary>
    public class Grid<T>
    {
        Dictionary<Tuple<int, int>, T> dict = new Dictionary<Tuple<int, int>, T>();
        public T this[int x, int y]
        {
            get { return Get(x, y); }
            set { Set(x, y, value); }
        }

        public bool HasField(int x, int y)
        {
            var key = Tuple.Create(x, y);
            return dict.ContainsKey(key);
        }

        public T Get(int x, int y)
        {
            var key = Tuple.Create(x, y);
            if (dict.ContainsKey(key))
                return dict[key];
            else
                return default(T);
        }

        public void Set(int x, int y, T value)
        {
            var key = Tuple.Create(x, y);
            dict[key] = value;
        }
    }
}
