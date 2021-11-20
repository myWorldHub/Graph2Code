using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Graph2Code
{
    internal class YamlSettingLoader : ISettingLoader
    {
        public GeneratorSetting Load(string path)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();

            //yml contains a string containing your YAML
            return deserializer.Deserialize<GeneratorSetting>(File.ReadAllText(path));
        }

        public void CreateSample(string path)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(GeneratorSetting.Sample);
            File.WriteAllText(path, yaml);
        }
    }
}
