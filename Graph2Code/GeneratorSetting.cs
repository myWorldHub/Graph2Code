using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Graph2Code
{
    class GeneratorSetting
    {
        public string Version { get; set; }

        public string Name { get; set; }

        public Dictionary<string, string> Settings { get; set; }

        public Dictionary<string, string> Programs { get; set; }

        public static GeneratorSetting Sample => new GeneratorSetting
        {
            Version = "0",
            Name = "Python3",
            Settings = new Dictionary<string, string>
                {
                    { "Mode","Sequential" },
                    { "Indent","  " },
                    { "Variable","Lower"},
                    { "Comment", "# {0}\n" },
                    { "ErrorLevel","0" }
                },
            Programs = new Dictionary<string, string>
                {
                    { "Updater","if __name__ == \"__main__\":\n{ia}" },
                    { "Value<Int32>","{Out:0} = {Args:0}\n{a}" },
                    { "AdditionOperator","{b}{Out:0} = {In:0} + {In:1}\n{a}" },
                    { "DebugText","{b}print({In:0})\n{a}" }
                }
        };

        private bool _loaded = false;
        
        private string _indent;

        private string _commentFormat;

        //TODO キャメルケースとかを指定できるように
        //TODO 命名もformatにしたい
        private Func<string, string> _variableFormat;

        private static Random rand = new Random();

        public string Format(string id, IList<string> inItems, IList<string> outItems, IList<string> args, string before, string after)
        {

            // 見つからない
            if (!Programs.ContainsKey(id))
            {
                return Comment($"{id} is not defined in setting.");
            }

            var keys = new HashSet<string>();
            foreach (Match m in Regex.Matches(Programs[id], @"\{([^\s]*?)\}"))
            {
                var key = m.Groups[1].Value;
                keys.Add(key);
            }

            var list = new List<string>();
            var format = Programs[id];

            for (var i = 0; i < keys.Count; i++)
            {

                var key = keys.ElementAt(i);

                switch (key)
                {
                    case "a":
                        list.Add(after);
                        break;
                    case "b":
                        list.Add(before);
                        break;
                    case "ia":
                        list.Add(AddIndent(after));
                        break;
                    case "ib":
                        list.Add(AddIndent(before));
                        break;
                    case "i":
                        list.Add(AddIndent(""));
                        break;
                    default:
                        var sp = key.Split(':');
                        switch (sp[0])
                        {
                            case "In":
                                list.Add(GetItem(id, sp, inItems));
                                break;
                            case "Out":
                                list.Add(GetItem(id, sp, outItems));
                                break;
                            case "Args":
                                list.Add(GetItem(id, sp, args));
                                break;
                            default:
                                throw new Exception($"Unknoen identifier {key} in {id}");
                        }
                        break;
                }

                format = format.Replace("{" + key + "}", "{" + i + "}");
            }

            return String.Format(format, args: list.ToArray<object>());
        }

        private void LoadSetting()
        {
            if (_loaded) return;
            _loaded = true;

            _indent = Settings.ContainsKey("Indent") ? Settings["Indent"] : "";
            _commentFormat = Settings.ContainsKey("Comment") ? Settings["Comment"] : "";

            //Variable case
            switch (Settings.ContainsKey("Variable") ? Settings["Variable"] : "default")
            {
                case "Lower":
                    _variableFormat = str => str.ToLower();
                    break;
                case "Upper":
                    _variableFormat = str => str.ToUpper();
                    break;
                default:
                    _variableFormat = str => str;
                    break;
            }
        }

        public string Comment(string str)
        {
            LoadSetting(); 
            return string.Format(_commentFormat, str);
        }

        public string AddIndent(string str)
        {
            LoadSetting();
            return str.Split('\n').Select(s => _indent + s).Join("\n");
        }

        public string CreateVariable(string name)
        {
            LoadSetting();
            return _variableFormat(name + "_" + RandomString(5));
        }

        private string GetItem(string id, IList<string> sp, IList<string> items)
        {
            if (sp.Count == 1)
            {
                return items[0];
            }
            else
            {
                if (int.TryParse(sp[1], out int index))
                {
                    if (index < items.Count)
                    {
                        return items[index];
                    }
                    else
                    {
                        throw new IndexOutOfRangeException($"{sp[0]} : {sp[1]} is out of range in {id}");
                    }
                }
                else
                {
                    throw new Exception($"Failed to parse int : {sp[1]} in {id}");
                }
            }
        }

        private static string RandomString(int count)
        {
            var root = "abcdefghijklmnopqrstuvwxyz0123456789";

            var result = "";
            for (int i = 0; i < count; i++)
            {
                result += root[rand.Next(root.Length)];
            }

            return result;
        }

    }
}
