using System.Collections.Generic;

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
                    { "Comment", "# {0}\n" },
                    { "ErrorLevel","0" }
                },
            Programs = new Dictionary<string, string>
                {
                    { "Updater","if __name__ == \"__main__\":\n{ib}" },
                    { "Value<System.Int32>","{Out:0} = {Args:0}\n{b}" }
                    { "AdditionOperator","{a}\n{Out:0} = {In:0} + {In:1}\n{b}" },
                    { "DebugText","{a}\nprint({In:0})\n{b}" }
                }
        };
    }
}
