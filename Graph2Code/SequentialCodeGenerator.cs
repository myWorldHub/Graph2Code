using GraphConnectEngine;
using GraphConnectEngine.Graphs.Event;
using GraphConnectEngine.Nodes;
using System;
using System.Collections.Generic;

namespace Graph2Code
{
    internal class SequentialCodeGenerator
    {

        private GeneratorSetting _setting;

        private IDictionary<string, int> _nameCounts = new Dictionary<string,int>();

        public SequentialCodeGenerator(GeneratorSetting setting)
        {
            _setting = setting;
        }

        /// <summary>
        /// 生成する
        /// </summary>
        /// <param name="updater"></param>
        /// <returns></returns>
        public string Generate(UpdaterGraph updater)
        {
            return Run(updater, new Dictionary<IItemTypeResolver, string>());
        }

        /// <summary>
        /// 再帰的にコードを生成する
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="variables"></param>
        /// <param name="moveNext"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        string Run(IGraph graph,Dictionary<IItemTypeResolver,string> variables,bool moveNext = true)
        {
            var before       = "";
            var after        = "";
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
                        //TODO default
                        before += "/*TODO*/";
                        inVariables.Add(CreateVariable(node.TypeResolver.ItemName));
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
                    //TODO Processチェックすべき
                    //Console.WriteLine("Check" + another.TypeResolver.GetHashCode());
                    before += Run(another.Graph, variables, false);
                }

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
                    varName = CreateVariable(resolver.ItemName);
                    //Console.WriteLine("Create New Variable " + varName);
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
                    if(others.Length != 0) 
                    { 
                        foreach(var another in others)
                        {
                            after += Run(another.Graph, variables);
                        }
                    }
                }
            }

            return _setting.Format(graph.GraphName,inVariables,outVariables,graph.GetArgs(),before,after);
        }

        private string CreateVariable(string name)
        {
            var formatter = _setting.GetVariableFormat(name);
            
            if (!_nameCounts.ContainsKey(name))
            {
                _nameCounts[name] = 0;
            }
            _nameCounts[name]++;

            return formatter(_nameCounts[name]);
        }
    }
}
