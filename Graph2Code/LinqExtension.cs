using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph2Code
{
    internal static class LinqExtension
    {

        private static Random rand = new Random();

        public static string Join<IResult>(this IEnumerable<IResult> enumerable,string key)
        {
            return string.Join(key,enumerable);
        }

        public static string Random(this string root, int count)
        {
            var result = "";
            for (int i = 0; i < count; i++)
            {
                result += root[rand.Next(root.Length)];
            }
            return result;
        }
    }
}
