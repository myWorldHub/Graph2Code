using GraphConnectEngine;
using GraphConnectEngine.Graphs;
using GraphConnectEngine.Graphs.Event;
using GraphConnectEngine.Graphs.Operator;
using GraphConnectEngine.Graphs.Value;
using GraphConnectEngine.Nodes;
using System;
using System.Threading.Tasks;

namespace Graph2Code
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //Logger.LogLevel = Logger.LevelDebug;
            //Logger.SetLogMethod(msg => Console.WriteLine(msg));

            // System.IO.File.WriteAllText("python.yaml",new YamlSettingLoader().Encode(GeneratorSetting.Sample));
            // Environment.Exit(0);

            var gen = new SequentialCodeGenerator(GeneratorSetting.Sample);

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
            var valueInt1 = new ValueGraph<int>(conn, 1);
            var valueInt2 = new ValueGraph<int>(conn, 5);
            var addition = new AdditionOperatorGraph(conn);

            conn.ConnectNode(updater.OutProcessNodes[0], debugText.InProcessNodes[0]);
            conn.ConnectNode(valueInt1.OutItemNodes[0], addition.InItemNodes[0]);
            conn.ConnectNode(valueInt2.OutItemNodes[0], addition.InItemNodes[1]);
            conn.ConnectNode(addition.OutItemNodes[0], debugText.InItemNodes[0]);

            return updater;
        }

        private class MockProcessSender : IProcessSender
        {
            public Task Fire(IGraph graph, object[] parameters)
            {
                throw new NotImplementedException();
            }
        }
    }
}