using GraphConnectEngine;
using GraphConnectEngine.Graphs.Event;
using GraphConnectEngine.Nodes;
using System;
using System.Collections.Generic;

namespace Graph2Code
{
    internal class SequentialCodeGenerator
    {

        /// <summary>
        /// beforeが___DEFAULT___ならデフォルト関数を使う、代入すべき変数名はinVariables
        /// </summary>
        /// <param name="inVariables"></param>
        /// <param name="outVariables"></param>
        /// <param name="before"></param>
        /// <param name="after"></param>
        /// <returns></returns>
        public delegate IList<string> CodeGenFunction(IGraph graph, IList<IList<string>> before, IList<IList<string>> after, IList<string> inVariables, IList<string> outVariables);
        public delegate IList<string> CodeGenFunction<T>(T graph, IList<IList<string>> before, IList<IList<string>> after, IList<string> inVariables, IList<string> outVariables) where T : IGraph;

        Dictionary<Type,CodeGenFunction> _funcs = new Dictionary<Type, CodeGenFunction>();

        /// <summary>
        /// 生成する
        /// </summary>
        /// <param name="updater"></param>
        /// <returns></returns>
        public string Generate(UpdaterGraph updater)
        {
            return string.Join("\n", Run(updater, new Dictionary<IItemTypeResolver, string>()));
        }

        /// <summary>
        /// 再帰的にコードを生成する
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="variables"></param>
        /// <param name="moveNext"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        IList<string> Run(IGraph graph,Dictionary<IItemTypeResolver,string> variables,bool moveNext = true)
        {

            //TODO ジェネリクスの検索
            var graphType = graph.GetType();
            if (!_funcs.ContainsKey(graphType))
            {
                throw new Exception($"GraphType {graphType} is not registered.");
            }

            var before       = new List<IList<string>>();
            var after        = new List<IList<string>>();
            var inVariables  = new List<string>();
            var outVariables = new List<string>();

            var conn = graph.Connector;

            //InItemNodeの処理
            foreach (var node in graph.InItemNodes)
            {
                //Outにつながっているか確認する
                if (!conn.TryGetAnotherNode(node,out OutItemNode another))
                {
                    if (node.HasDefaultItemGetter())
                    {
                        before.Add(new List<string> { "___DEFAULT___" });
                        inVariables.Add(CreateVariableName(node.TypeResolver.ItemName));
                        continue;
                    }
                    else
                    {
                        throw new Exception("InItemNode is not connected and doesn't have default ItemGetter.");
                    }
                }

                //まだ処理されていない(Processがつながっていないであろうノード)
                if (!variables.ContainsKey(another.TypeResolver))
                {
                    //TODO Processチェックするべき
                    before.Add(Run(another.Graph, variables, false));
                }

                //登録
                before.Add(Array.Empty<string>());
                inVariables.Add(variables[another.TypeResolver]);
            }

            //inVariablesの変数を登録する
            for(var i = 0; i < graph.InItemNodes.Count; i++)
            {
                variables[graph.InItemNodes[i].TypeResolver] = inVariables[i];
            }

            //OutItemNodeの処理
            for (var i = 0; i < graph.OutItemNodes.Count; i++)
            {
                var resolver = graph.OutItemNodes[i].TypeResolver;

                //InとOutが同じ(値を流すだけなら新しく変数を作らない)
                var varName = "";
                var useStream = false;
                for (var j = 0; j < graph.InItemNodes.Count; j++)
                {
                    if (graph.InItemNodes[j].TypeResolver == resolver)
                    {
                        useStream = true;
                        varName = inVariables[j];
                        break;
                    }
                }

                //新しく変数を生成する
                if (!useStream)
                {
                    varName = CreateVariableName(resolver.ItemName);
                }

                //登録
                outVariables.Add(varName);
                variables[resolver] = varName;
            }

            //OutProcessNodeの処理
            if (moveNext)
            {
                foreach(var node in graph.OutProcessNodes)
                {
                    var others = conn.GetOtherNodes(node);
                    if(others.Length == 0)
                    {
                        after.Add(Array.Empty<string>());
                    }
                    else
                    {
                        var statements = new List<string>();
                        foreach(var another in others)
                        {
                            statements.AddRange(Run(another.Graph, variables));
                        }
                        after.Add(statements);
                    }
                }
            }


            return _funcs[graphType](graph,before,after,inVariables,outVariables);
        }

        /// <summary>
        /// グラフからコードを生成する関数を登録する。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        public void Register<T>(CodeGenFunction<T> func) where T : IGraph
        {
            _funcs[typeof(T)] = (graph,before,after,inVar,outVar) => {
                return func((T)graph, before, after, inVar, outVar);
            };
        }

        string CreateVariableName(string name)
        {
            return name + "_" + RandomString(5);
        }

        static string RandomString(int count)
        {
            var root = "abcdefghijklmnopqrstuvwxyz0123456789";
            var rand = new Random();

            var result = "";
            for(int i = 0; i < count; i++)
            {
                result += root[rand.Next(root.Length)];
            }

            return result;
        }
    }
}
