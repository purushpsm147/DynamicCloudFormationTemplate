using System.Text;
using DynamicCloudFormationTemplate.Models;

namespace DynamicCloudFormationTemplate;

internal static class YamlTemplateGenerator
{
    public static string GenerateYaml(CloudformationInputs inputs, Dictionary<string, string> systemInfo)
    {
        var template = new StringBuilder();
        
        // Metadata section
        template.AppendLine("Metadata:");
        template.AppendLine("  SourceSystem:");
        template.AppendLine($"    Memory: {systemInfo["MemoryInMB"]}");
        template.AppendLine($"    Processors: {systemInfo["NumProcessors"]}");
        template.AppendLine($"    WindowsVersion: {systemInfo["WindowsVersion"]}");
        
        template.AppendLine("AWSTemplateFormatVersion: '2010-09-09'");
        template.AppendLine("Description: 'AWS CloudFormation Template for VPC and Resources'");
        
        // Parameters section
        GenerateParameters(template, inputs);
        
        // Resources section
        template.AppendLine("Resources:");
        GenerateVpcResources(template, inputs);
        
        if (inputs.EC2Config != null)
        {
            GenerateEC2Resources(template, inputs);
        }
        
        GenerateOutputs(template, inputs);
        
        return template.ToString();
    }

    private static void GenerateParameters(StringBuilder template, CloudformationInputs inputs)
    {
        template.AppendLine("Parameters:");
        template.AppendLine("  Region:");
        template.AppendLine("    Type: String");
        template.AppendLine($"    Default: {inputs.Region}");
        // Add more parameters as needed
    }

    private static void GenerateVpcResources(StringBuilder template, CloudformationInputs inputs)
    {
        // VPC
        template.AppendLine("  MyVPC:");
        template.AppendLine("    Type: 'AWS::EC2::VPC'");
        template.AppendLine("    Properties:");
        template.AppendLine($"      CidrBlock: {inputs.VpcConfig.VpcCidr}");
        template.AppendLine("      EnableDnsHostnames: true");
        template.AppendLine("      EnableDnsSupport: true");
        template.AppendLine("      Tags:");
        template.AppendLine("        - Key: Name");
        template.AppendLine("          Value: MyVPC");

        // Internet Gateway Resources
        GenerateInternetGatewayResources(template);

        // Public Subnets
        foreach (var subnet in inputs.VpcConfig.PublicSubnets)
        {
            GeneratePublicSubnet(template, subnet);
        }

        // Private Subnets and NAT Gateway if needed
        if (inputs.VpcConfig.CreatePrivateSubnets)
        {
            foreach (var subnet in inputs.VpcConfig.PrivateSubnets)
            {
                GeneratePrivateSubnet(template, subnet);
            }

            if (inputs.VpcConfig.CreateNatGateway)
            {
                GenerateNatGatewayResources(template);
            }
        }
    }

    private static void GenerateEC2Resources(StringBuilder template, CloudformationInputs inputs)
    {
        // If SSM support is enabled, generate an IAM role resource.
        if (inputs.EC2Config.EnableSSMRole)
        {
            GenerateIAMResources(template);
        }

        // Security Groups resource
        GenerateSecurityGroups(template, inputs.EC2Config.SecurityGroupRules);

        // Bastion Host resource if requested
        if (inputs.CreateBastionHost)
        {
            GenerateBastionHost(template, inputs);
        }
    }

    private static void GenerateInternetGatewayResources(StringBuilder template)
    {
        template.AppendLine("  MyIGW:");
        template.AppendLine("    Type: 'AWS::EC2::InternetGateway'");
        template.AppendLine("    Properties:");
        template.AppendLine("      Tags:");
        template.AppendLine("        - Key: Name");
        template.AppendLine("          Value: MyIGW");
        template.AppendLine("  IGWAttachment:");
        template.AppendLine("    Type: 'AWS::EC2::VPCGatewayAttachment'");
        template.AppendLine("    Properties:");
        template.AppendLine("      VpcId: !Ref MyVPC");
        template.AppendLine("      InternetGatewayId: !Ref MyIGW");
    }

    private static void GeneratePublicSubnet(StringBuilder template, SubnetConfig subnet)
    {
        template.AppendLine($"  PublicSubnet{subnet.AvailabilityZoneIndex + 1}:");
        template.AppendLine("    Type: 'AWS::EC2::Subnet'");
        template.AppendLine("    Properties:");
        template.AppendLine("      VpcId: !Ref MyVPC");
        template.AppendLine($"      CidrBlock: {subnet.CidrBlock}");
        template.AppendLine($"      AvailabilityZone: !Select [{subnet.AvailabilityZoneIndex}, !GetAZs '']");
        template.AppendLine("      MapPublicIpOnLaunch: true");
        template.AppendLine("      Tags:");
        template.AppendLine("        - Key: Name");
        template.AppendLine($"          Value: PublicSubnet{subnet.AvailabilityZoneIndex + 1}");
    }

    private static void GeneratePrivateSubnet(StringBuilder template, SubnetConfig subnet)
    {
        template.AppendLine($"  PrivateSubnet{subnet.AvailabilityZoneIndex + 1}:");
        template.AppendLine("    Type: 'AWS::EC2::Subnet'");
        template.AppendLine("    Properties:");
        template.AppendLine("      VpcId: !Ref MyVPC");
        template.AppendLine($"      CidrBlock: {subnet.CidrBlock}");
        template.AppendLine($"      AvailabilityZone: !Select [{subnet.AvailabilityZoneIndex}, !GetAZs '']");
        template.AppendLine("      MapPublicIpOnLaunch: false");
        template.AppendLine("      Tags:");
        template.AppendLine("        - Key: Name");
        template.AppendLine($"          Value: PrivateSubnet{subnet.AvailabilityZoneIndex + 1}");
    }

    private static void GenerateNatGatewayResources(StringBuilder template)
    {
        template.AppendLine("  NatEIP:");
        template.AppendLine("    Type: 'AWS::EC2::EIP'");
        template.AppendLine("    Properties:");
        template.AppendLine("      Domain: vpc");
        template.AppendLine("  NatGateway:");
        template.AppendLine("    Type: 'AWS::EC2::NatGateway'");
        template.AppendLine("    Properties:");
        template.AppendLine("      AllocationId: !GetAtt NatEIP.AllocationId");
        template.AppendLine("      SubnetId: !Ref PublicSubnet1");
    }

    private static void GenerateIAMResources(StringBuilder template)
    {
        template.AppendLine("  EC2IAMRole:");
        template.AppendLine("    Type: 'AWS::IAM::Role'");
        template.AppendLine("    Properties:");
        template.AppendLine("      AssumeRolePolicyDocument:");
        template.AppendLine("        Version: '2012-10-17'");
        template.AppendLine("        Statement:");
        template.AppendLine("          - Effect: Allow");
        template.AppendLine("            Principal:");
        template.AppendLine("              Service:");
        template.AppendLine("                - ec2.amazonaws.com");
        template.AppendLine("            Action:");
        template.AppendLine("              - sts:AssumeRole");
        // ...additional IAM properties as needed...
    }

    private static void GenerateSecurityGroups(StringBuilder template, List<SecurityGroupRule> rules)
    {
        template.AppendLine("  EC2SecurityGroup:");
        template.AppendLine("    Type: 'AWS::EC2::SecurityGroup'");
        template.AppendLine("    Properties:");
        template.AppendLine("      GroupDescription: 'Security Group for EC2 instances'");
        template.AppendLine("      VpcId: !Ref MyVPC");
        template.AppendLine("      SecurityGroupIngress:");
        foreach (var rule in rules)
        {
            template.AppendLine("        - IpProtocol: " + rule.Protocol);
            template.AppendLine("          FromPort: " + rule.FromPort);
            template.AppendLine("          ToPort: " + rule.ToPort);
            template.AppendLine("          CidrIp: " + rule.CidrIp);
        }
    }

    private static void GenerateBastionHost(StringBuilder template, CloudformationInputs inputs)
    {
        template.AppendLine("  BastionHost:");
        template.AppendLine("    Type: 'AWS::EC2::Instance'");
        template.AppendLine("    Properties:");
        template.AppendLine("      ImageId: 'ami-xxxxxxxx'  # Replace with a valid AMI or parameter reference");
        template.AppendLine($"      InstanceType: '{inputs.EC2Config.InstanceType}'");
        template.AppendLine("      SubnetId: !Ref PublicSubnet1");
        template.AppendLine("      SecurityGroupIds:");
        template.AppendLine("        - !Ref EC2SecurityGroup");
        template.AppendLine("      Tags:");
        template.AppendLine("        - Key: Name");
        template.AppendLine("          Value: BastionHost");
    }

    private static void GenerateOutputs(StringBuilder template, CloudformationInputs inputs)
    {
        template.AppendLine("Outputs:");
        template.AppendLine("  VpcId:");
        template.AppendLine("    Value: !Ref MyVPC");
        if (inputs.CreateBastionHost)
        {
            template.AppendLine("  BastionPublicIP:");
            template.AppendLine("    Value: !GetAtt BastionHost.PublicIp");
        }
    }
}