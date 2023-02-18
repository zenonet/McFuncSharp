using FuncSharp.Core;
using FuncSharp.DataPackGen;

namespace FuncSharp;

public class BlockTag
{
    public BlockTag(string name, params Block[] blocks)
    {
        Blocks = blocks.ToList();
        Name = name;
    }

    public string Name { get; set; }
    public List<Block> Blocks { get; set; }

    public void GenerateFile(string path, DataPackGenerator gen)
    {
        Directory.CreateDirectory(Path.Combine(path, "data/minecraft/tags/blocks/"));
        string filePath = Path.Combine(path, "data/minecraft/tags/blocks/" + Name + ".json");

        string list = string.Join(", ", Blocks.Select(b => "\"" + b + "\""));
        
        string json = "{\n\t\"replace\": false,\n\t\"values\": [" + list + "]\n}";
        
        File.WriteAllText(filePath, json);
    }
}