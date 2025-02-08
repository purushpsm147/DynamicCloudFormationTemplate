namespace DynamicCloudFormationTemplate.Models;

public class EC2Config
{
    public string InstanceType { get; set; } = "t2.micro";
    public string AmiId { get; set; }
    public string KeyPairName { get; set; }
    public bool EnableSSMRole { get; set; } = true;
    public List<SecurityGroupRule> SecurityGroupRules { get; set; } = new();
}
