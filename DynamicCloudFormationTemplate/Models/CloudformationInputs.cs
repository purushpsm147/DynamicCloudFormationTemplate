namespace DynamicCloudFormationTemplate.Models;

public class CloudformationInputs
{
    public string Region { get; set; } = "us-east-1";
    public VpcConfig VpcConfig { get; set; } = new();
    public EC2Config EC2Config { get; set; } = new();
    public bool CreateBastionHost { get; set; }
    public string ResourceTag { get; set; } = "from-cft";
    // Remove IamRoleArn property since we're creating the role in the template
}
