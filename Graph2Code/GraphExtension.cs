using GraphConnectEngine;
using GraphConnectEngine.Graphs.Value;
using System;
using System.Collections.Generic;

namespace Graph2Code
{
    internal static class GraphExtension
    {
        public static IList<string> GetArgs(this IGraph graph)
        {
            if(graph is ValueGraph<int> intValue)
            {
                return new List<string> { intValue.Value.ToString() };
            }
            return Array.Empty<string>();
        }
    }
}
