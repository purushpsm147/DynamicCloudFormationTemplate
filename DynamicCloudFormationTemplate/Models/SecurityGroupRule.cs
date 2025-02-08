namespace DynamicCloudFormationTemplate.Models;

public class SecurityGroupRule
{
    public string Protocol { get; set; } = "tcp";
    public int FromPort { get; set; }
    public int ToPort { get; set; }
    public string CidrIp { get; set; } = "0.0.0.0/0";
}
