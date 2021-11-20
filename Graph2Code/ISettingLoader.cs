using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph2Code
{
    internal interface ISettingLoader
    {
        public GeneratorSetting Load(string path);

        public string Encode(GeneratorSetting setting);
    }
}
