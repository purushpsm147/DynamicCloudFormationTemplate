namespace DynamicCloudFormationTemplate.Models;

public class ServerInstance
{
    public string InstanceType { get; set; }
    public string Name { get; set; }
    public int Count { get; set; } = 1;
    public List<string> AlternativeTypes { get; set; } = new();
}
