using DynamicCloudFormationTemplate.Models;
using System.Threading.Tasks;

namespace DynamicCloudFormationTemplate;

internal class Program
{
    private static ValidateEc2Configurations _validator;
    private static InventoryParser _inventory;

    static async Task Main(string[] args)
    {
        try
        {
            // Parse system information, ensure to use XML character entities to represent special characters:
            _inventory = new InventoryParser("C:\\Users\\pgupta12\\source\\repos\\DynamicCloudFormationTemplate\\DynamicCloudFormationTemplate\\SampleInventory.xml"); // fetch it from CDR Inventory service
            var systemInfo = _inventory.GetSystemInfo();
            var inputs = new CloudformationInputs();

            // Get AWS region
            Console.Write("Enter AWS region (default: us-east-1): "); //Input from CDR Service Post endpoint
            string region = ReadInput("us-east-1");
            inputs.Region = region;
            _validator = new ValidateEc2Configurations(region);

            // Configure and validate EC2 settings
            if (!await ConfigureEC2(inputs, systemInfo))
            {
                Console.WriteLine("EC2 configuration validation failed. Exiting...");
                return;
            }

            // Configure networking
            if (!ConfigureNetworking(inputs, systemInfo))
            {
                Console.WriteLine("Network configuration validation failed. Exiting...");
                return;
            }

            // Generate CloudFormation template
            string yaml = YamlTemplateGenerator.GenerateYaml(inputs, systemInfo);
            string outputPath = "cloudformation-template.yaml";
            await File.WriteAllTextAsync(outputPath, yaml);
            Console.WriteLine($"Template generated successfully: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Dispose AWS client if needed
            _validator?.Dispose();
        }
    }

    private static async Task<bool> ConfigureEC2(CloudformationInputs inputs, Dictionary<string, string> systemInfo)
    {
        // Select instance type based on system specs
        int memoryInMB = int.Parse(systemInfo["MemoryInMB"]);
        int numCPU = int.Parse(systemInfo["NumProcessors"]);
        
        string instanceType = DetermineInstanceType(memoryInMB, numCPU);
        if (!await _validator.ValidateInstanceType(instanceType))
        {
            Console.WriteLine($"Invalid instance type: {instanceType}");
            return false;
        }
        inputs.EC2Config.InstanceType = instanceType;

        // Configure security group
        Console.Write("Enter allowed IP for SSH access (default: your IP): ");
        string sshIp = ReadInput(systemInfo["PrimaryIP"] + "/32");
        inputs.EC2Config.SecurityGroupRules.Add(new SecurityGroupRule
        {
            FromPort = 22,
            ToPort = 22,
            Protocol = "tcp",
            CidrIp = sshIp
        });

        return true;
    }

    private static bool ConfigureNetworking(CloudformationInputs inputs, Dictionary<string, string> systemInfo)
    {
        // Use system's subnet to suggest VPC CIDR
        string defaultVpcCidr = "10.0.0.0/16";
        Console.Write($"Enter VPC CIDR (default: {defaultVpcCidr}): ");
        inputs.VpcConfig.VpcCidr = ReadInput(defaultVpcCidr);

        // Rest of the VPC configuration...
        Console.Write("Do you need private subnets? (y/n): ");
        inputs.VpcConfig.CreatePrivateSubnets = ReadInput("n").ToLower() == "y";

        // Public Subnets
        Console.Write("Number of public subnets (1-2): ");
        int publicSubnetCount = int.Parse(ReadInput("1"));
        for (int i = 0; i < publicSubnetCount; i++)
        {
            Console.Write($"Enter Public Subnet {i + 1} CIDR: ");
            inputs.VpcConfig.PublicSubnets.Add(new SubnetConfig
            {
                CidrBlock = ReadInput($"10.0.{i + 1}.0/24"),
                AvailabilityZoneIndex = i
            });
        }

        // Private Subnets if needed
        if (inputs.VpcConfig.CreatePrivateSubnets)
        {
            Console.Write("Do you need NAT Gateway? (y/n): ");
            inputs.VpcConfig.CreateNatGateway = ReadInput("n").ToLower() == "y";

            Console.Write("Number of private subnets (1-2): ");
            int privateSubnetCount = int.Parse(ReadInput("1"));
            for (int i = 0; i < privateSubnetCount; i++)
            {
                Console.Write($"Enter Private Subnet {i + 1} CIDR: ");
                inputs.VpcConfig.PrivateSubnets.Add(new SubnetConfig
                {
                    CidrBlock = ReadInput($"10.0.{i + 10}.0/24"),
                    AvailabilityZoneIndex = i
                });
            }
        }

        return true;
    }

    private static string DetermineInstanceType(int memoryMB, int cpuCount)
    {
        //User Selection from Drop down list
        if (memoryMB <= 4096 && cpuCount <= 2) return "t2.micro";
        if (memoryMB <= 8192 && cpuCount <= 4) return "t2.medium";
        return "t2.large";
    }

    private static string ReadInput(string defaultValue = "")
    {
        string input = Console.ReadLine()?.Trim() ?? "";
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }
}