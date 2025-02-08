namespace DynamicCloudFormationTemplate.Models;

public class VpcConfig
{
    public string VpcCidr { get; set; } = "10.0.0.0/16";
    public bool CreatePrivateSubnets { get; set; }
    public bool CreateNatGateway { get; set; }
    public List<SubnetConfig> PublicSubnets { get; set; } = new();
    public List<SubnetConfig> PrivateSubnets { get; set; } = new();
}
