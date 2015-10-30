using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptParser
{
    public static class ListExtentions
    {
        public static T pop<T>(this List<T> list)
        {
            var last = list.Last();
            list.Remove(last);
            return last;
        }
    }
}
