using GraphConnectEngine;
using GraphConnectEngine.Graphs;
using GraphConnectEngine.Graphs.Event;
using GraphConnectEngine.Graphs.Value;
using GraphConnectEngine.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Graph2Code
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var gen = new SequentialCodeGenerator();
            LoadRule(gen);

            var result = gen.Generate(CreateTestGraph());
            Console.WriteLine(result);

            Console.ReadKey();
        }

        static UpdaterGraph CreateTestGraph()
        {
            var conn = new NodeConnector();

            var updater = new UpdaterGraph(conn, new MockProcessSender());
            var debugText = new DebugTextGraph(conn,msg =>
            {
                return Task.FromResult(true);
            });
            var valueInt = new ValueGraph<int>(conn, 1);

            conn.ConnectNode(updater.OutProcessNodes[0], debugText.InProcessNodes[0]);
            conn.ConnectNode(valueInt.OutItemNodes[0], debugText.InItemNodes[0]);

            return updater;
        }

        static void LoadRule(SequentialCodeGenerator gen)
        {
            var indent = "  ";

            gen.Register<UpdaterGraph>((graph,before, after, inVar, outVar) =>
            {
                var list = new List<string>();

                if (before.Count > 0)
                {
                    foreach (var v in before)
                    {
                        if (v.Count == 0) continue;
                        list.AddRange(v);
                    }
                }

                list.Add("if __name__ == \"__main__\":");

                if (after.Count > 0)
                {
                    foreach (var v in after)
                    {
                        if (v.Count == 0) continue;
                        list.AddRange(v.Select(s => indent + s));
                    }
                }

                return list;
            });

            gen.Register<ValueGraph<int>>((graph, before, after, inVar, outVar) =>
            {
                var list = new List<string>();

                if (before.Count > 0)
                {
                    foreach (var v in before)
                    {
                        if (v.Count == 0) continue;
                        list.AddRange(v);
                    }
                }

                list.Add($"{outVar[0]} = {graph.Value}");

                if (after.Count > 0)
                {
                    foreach (var v in after)
                    {
                        if (v.Count == 0) continue;
                        list.AddRange(v.Select(s => indent + s));
                    }
                }

                return list;
            });

            gen.Register<DebugTextGraph>((graph, before, after, inVar, outVar) =>
            {
                var list = new List<string>();

                if (before.Count > 0)
                {
                    foreach (var v in before)
                    {
                        if (v.Count == 0) continue;
                        list.AddRange(v);
                    }
                }

                list.Add($"print({inVar[0]})");

                if (after.Count > 0)
                {
                    foreach (var v in after)
                    {
                        if (v.Count == 0) continue;
                        list.AddRange(v.Select(s => indent + s));
                    }
                }

                return list;
            });
        }

        private class MockProcessSender : IProcessSender
        {
            public Task Fire(IGraph graph, object[] parameters)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}