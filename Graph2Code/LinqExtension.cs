using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph2Code
{
    internal static class LinqExtension
    {
        public static string Join<IResult>(this IEnumerable<IResult> enumerable,string key)
        {
            return string.Join(key,enumerable);
        }
    }
}
