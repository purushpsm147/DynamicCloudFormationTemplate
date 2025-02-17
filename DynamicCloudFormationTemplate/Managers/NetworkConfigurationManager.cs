using DynamicCloudFormationTemplate.Interfaces;
using DynamicCloudFormationTemplate.Models;

namespace DynamicCloudFormationTemplate.Managers;

public class NetworkConfigurationManager
{
    private readonly IEC2Service _ec2Service;
    private readonly CidrValidator _cidrValidator;

    public NetworkConfigurationManager(IEC2Service ec2Service)
    {
        _ec2Service = ec2Service;
        _cidrValidator = new CidrValidator();
    }

    private string PresentCidrSuggestions(string title, List<string> suggestions, string defaultChoice)
    {
        Console.WriteLine($"\n{title}");
        for (int i = 0; i < suggestions.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {suggestions[i]}");
        }
        Console.Write($"Choose a number (1-{suggestions.Count}) or enter custom CIDR [default: {defaultChoice}]: ");
        var input = ReadInput(defaultChoice);
        
        if (int.TryParse(input, out int choice) && choice >= 1 && choice <= suggestions.Count)
        {
            return suggestions[choice - 1];
        }
        return input;
    }

    private List<string> GetVpcCidrSuggestions()
    {
        return new List<string>
        {
            "10.0.0.0/16",
            "172.16.0.0/16",
            "192.168.0.0/16"
        };
    }

    public bool ConfigureNetworking(CloudformationInputs inputs)
    {
        Console.WriteLine("\nConfiguring VPC and networking...");
        
        var vpcConfig = new VpcConfig();
        inputs.VpcConfig = vpcConfig;

        // Configure VPC CIDR with suggestions
        while (true)
        {
            vpcConfig.VpcCidr = PresentCidrSuggestions(
                "Select VPC CIDR:",
                GetVpcCidrSuggestions(),
                "10.0.0.0/16"
            );

            if (_ec2Service.ValidateCidrBlock(vpcConfig.VpcCidr))
                break;

            Console.WriteLine("Invalid CIDR format. Please use format like '10.0.0.0/16'");
        }

        // Configure public subnets
        Console.Write("Enter number of public subnets (1-4): ");
        if (!int.TryParse(ReadInput("2"), out int publicSubnetCount) || publicSubnetCount < 1 || publicSubnetCount > 4)
        {
            Console.WriteLine("Invalid number of public subnets. Using default of 2.");
            publicSubnetCount = 2;
        }

        var suggestedPublicSubnets = _cidrValidator.SuggestSubnetCidrs(vpcConfig.VpcCidr, publicSubnetCount);
        for (int i = 0; i < publicSubnetCount; i++)
        {
            var subnet = new SubnetConfig { AvailabilityZoneIndex = i };
            
            while (true)
            {
                subnet.CidrBlock = PresentCidrSuggestions(
                    $"Select Public Subnet {i + 1} CIDR:",
                    suggestedPublicSubnets,
                    suggestedPublicSubnets[i]
                );

                if (_cidrValidator.IsValidSubnetCidr(vpcConfig.VpcCidr, subnet.CidrBlock))
                    break;

                Console.WriteLine($"Invalid subnet CIDR. Must be within VPC range {vpcConfig.VpcCidr}");
            }
            
            vpcConfig.PublicSubnets.Add(subnet);
        }

        // Configure private subnets
        Console.Write("Do you want to create private subnets? (y/N): ");
        vpcConfig.CreatePrivateSubnets = ReadInput().ToLower() == "y";

        if (vpcConfig.CreatePrivateSubnets)
        {
            Console.Write("Enter number of private subnets (1-4): ");
            if (!int.TryParse(ReadInput("2"), out int privateSubnetCount) || privateSubnetCount < 1 || privateSubnetCount > 4)
            {
                Console.WriteLine("Invalid number of private subnets. Using default of 2.");
                privateSubnetCount = 2;
            }

            var suggestedPrivateSubnets = _cidrValidator.SuggestSubnetCidrs(vpcConfig.VpcCidr, privateSubnetCount, publicSubnetCount);
            for (int i = 0; i < privateSubnetCount; i++)
            {
                var subnet = new SubnetConfig { AvailabilityZoneIndex = i };
                
                while (true)
                {
                    subnet.CidrBlock = PresentCidrSuggestions(
                        $"Select Private Subnet {i + 1} CIDR:",
                        suggestedPrivateSubnets,
                        suggestedPrivateSubnets[i]
                    );

                    if (_cidrValidator.IsValidSubnetCidr(vpcConfig.VpcCidr, subnet.CidrBlock))
                        break;

                    Console.WriteLine($"Invalid subnet CIDR. Must be within VPC range {vpcConfig.VpcCidr}");
                }
                
                vpcConfig.PrivateSubnets.Add(subnet);
            }

            Console.Write("Do you want to create a NAT Gateway for private subnets? (y/N): ");
            vpcConfig.CreateNatGateway = ReadInput().ToLower() == "y";
        }

        return true;
    }

    private static string ReadInput(string defaultValue = "")
    {
        string input = Console.ReadLine()?.Trim() ?? "";
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }
}
