namespace DynamicCloudFormationTemplate.Models;

public class EC2Config
{
    public List<ServerInstance> Instances { get; set; } = new();
    public List<string> AllowedSSHIps { get; set; } = new();
    public bool EnableSSMRole { get; set; } = true;
}
