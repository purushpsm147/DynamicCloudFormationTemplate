using DynamicCloudFormationTemplate.Interfaces;
using DynamicCloudFormationTemplate.Models;

namespace DynamicCloudFormationTemplate.Managers;

public class EC2ConfigurationManager
{
    private readonly IEC2Service _ec2Service;

    public EC2ConfigurationManager(IEC2Service ec2Service)
    {
        _ec2Service = ec2Service;
    }

    public async Task<bool> ConfigureEC2(CloudformationInputs inputs)
    {
        Console.WriteLine("\nConfiguring EC2 instances...");

        var ec2Config = new EC2Config();
        inputs.EC2Config = ec2Config;

        Console.Write("Do you want to enable SSM for EC2 instances? (y/N): ");
        ec2Config.EnableSSMRole = ReadInput().ToLower() == "y";

        Console.Write("Enter allowed SSH IP addresses (comma-separated): ");
        var ips = ReadInput().Split(',', StringSplitOptions.RemoveEmptyEntries);
        ec2Config.AllowedSSHIps.AddRange(ips.Select(ip => ip.Trim()));

        Console.WriteLine("\nEnter EC2 instance configurations:");
        while (true)
        {
            var instance = new ServerInstance();

            Console.Write("Enter instance name (or press Enter to finish): ");
            var name = ReadInput();
            if (string.IsNullOrWhiteSpace(name)) break;

            instance.Name = name;

            Console.Write("Enter instance type (e.g., t2.micro): ");
            instance.InstanceType = ReadInput();
            if (!await _ec2Service.ValidateInstanceType(instance.InstanceType))
            {
                Console.WriteLine($"Invalid instance type: {instance.InstanceType}");
                continue;
            }

            Console.Write("Enter number of instances: ");
            if (!int.TryParse(ReadInput(), out int count) || count < 1)
            {
                Console.WriteLine("Invalid instance count. Must be a positive number.");
                continue;
            }
            instance.Count = count;

            ec2Config.Instances.Add(instance);
        }

        return ec2Config.Instances.Any();
    }

    private static string ReadInput(string defaultValue = "") => 
        string.IsNullOrWhiteSpace(Console.ReadLine()?.Trim()) ? defaultValue : Console.ReadLine().Trim();

    private bool ValidateIpAddress(string ipAddress)
    {
        return System.Net.IPAddress.TryParse(ipAddress, out _);
    }
}
