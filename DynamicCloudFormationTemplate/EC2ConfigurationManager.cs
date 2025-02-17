using DynamicCloudFormationTemplate.Models;
using DynamicCloudFormationTemplate.Interfaces;
using Amazon.EC2.Model;
using System.Text.RegularExpressions;
using System.Net;

namespace DynamicCloudFormationTemplate;

public class EC2ConfigurationManager
{
    private readonly IEC2Service _ec2Service;
    private readonly string _region;
    private readonly ValidateEc2Configurations _validator;

    public EC2ConfigurationManager(string region)
    {
        _region = region;
        _ec2Service = new Services.EC2Service(region);
        _validator = new ValidateEc2Configurations(region);
    }

    public async Task<bool> ConfigureEC2(CloudformationInputs inputs)
    {
        try
        {
            Console.WriteLine("\nEC2 Instance Configuration");
            Console.WriteLine("-------------------------");

            var ec2Config = new EC2Config();
            
            Console.Write("Enter the total number of EC2 instances needed (1-10): ");
            if (!int.TryParse(Console.ReadLine(), out int totalInstances) || totalInstances < 1 || totalInstances > 10)
            {
                Console.WriteLine("Invalid number of instances. Must be between 1 and 10.");
                return false;
            }

            int configuredInstances = 0;
            while (configuredInstances < totalInstances)
            {
                Console.WriteLine($"\nConfiguring instance {configuredInstances + 1} of {totalInstances}");
                var instance = await ConfigureInstance();
                
                if (instance != null)
                {
                    ec2Config.Instances.Add(instance);
                    configuredInstances += instance.Count;

                    if (configuredInstances < totalInstances)
                    {
                        Console.Write("\nConfigure another instance type? (y/n): ");
                        if (Console.ReadLine()?.ToLower() != "y")
                            break;
                    }
                }
            }

            if (ec2Config.Instances.Any())
            {
                await ConfigureSSHAccess(ec2Config);
                inputs.EC2Config = ec2Config;
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring EC2: {ex.Message}");
            return false;
        }
    }

    private async Task<ServerInstance?> ConfigureInstance()
    {
        try
        {
            var instance = new ServerInstance();

            Console.Write("Enter instance name (letters, numbers, and hyphens only): ");
            string? name = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(name) || !Regex.IsMatch(name, "^[a-zA-Z0-9-]+$"))
            {
                Console.WriteLine("Invalid instance name. Use only letters, numbers, and hyphens.");
                return null;
            }
            instance.Name = name;

            Console.Write("Enter required number of CPUs (1-128): ");
            if (!int.TryParse(Console.ReadLine(), out int cpus) || cpus < 1 || cpus > 128)
            {
                Console.WriteLine("Invalid CPU count. Must be between 1 and 128.");
                return null;
            }

            Console.Write("Enter required RAM in GB (0.5-384): ");
            if (!float.TryParse(Console.ReadLine(), out float ram) || ram < 0.5 || ram > 384)
            {
                Console.WriteLine("Invalid RAM size. Must be between 0.5 and 384 GB.");
                return null;
            }

            var instanceTypes = await _ec2Service.GetRecommendedInstanceTypes(cpus, (int)Math.Ceiling(ram));

            if (!instanceTypes.Any())
            {
                Console.WriteLine("No suitable instance types found for the specified requirements.");
                return null;
            }

            await DisplayInstanceOptions(instanceTypes);
            instance.InstanceType = await SelectInstanceType(instanceTypes);

            return instance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error configuring instance: {ex.Message}");
            return null;
        }
    }

    private async Task<string> SelectInstanceType(List<InstanceTypeInfo> options)
    {
        const int maxAttempts = 3;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Console.Write("\nSelect instance type (1-3) or press Enter for default: ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
                return options.First().InstanceType;

            if (int.TryParse(input, out int selection) && selection >= 1 && selection <= Math.Min(options.Count, 3))
                return options[selection - 1].InstanceType;

            attempts++;
            Console.WriteLine($"Invalid selection. {maxAttempts - attempts} attempts remaining.");
        }

        Console.WriteLine("Using default instance type after maximum attempts.");
        return options.First().InstanceType;
    }

    private async Task DisplayInstanceOptions(List<InstanceTypeInfo> options)
    {
        Console.WriteLine("\nRecommended instance types for region: " + _region);
        for (int i = 0; i < Math.Min(options.Count, 3); i++)
        {
            var instance = options[i];
            Console.WriteLine($"{i + 1}. {instance.InstanceType} " +
                            $"(vCPUs: {instance.VCpuInfo.DefaultVCpus}, " +
                            $"Memory: {instance.MemoryInfo.SizeInMiB / 1024:F1} GB)");
        }
    }

    private async Task ConfigureSSHAccess(EC2Config ec2Config)
    {
        const int maxAttempts = 3;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            Console.Write("\nEnter comma-separated IP addresses for SSH access (e.g., 10.0.0.0,192.168.1.0): ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("No SSH access configured.");
                return;
            }

            var ips = input.Split(',').Select(ip => ip.Trim()).ToList();
            bool allValid = true;

            foreach (var ip in ips)
            {
                if (!ValidateIpAddress(ip))
                {
                    Console.WriteLine($"Invalid IP: {ip}");
                    allValid = false;
                    break;
                }
            }

            if (allValid)
            {
                ec2Config.AllowedSSHIps = [.. ips.Select(ip => $"{ip}/32")];
                return;
            }

            attempts++;
            if (attempts < maxAttempts)
            {
                Console.WriteLine($"Please try again. {maxAttempts - attempts} attempts remaining.");
            }
        }

        Console.WriteLine("SSH access configuration failed after maximum attempts.");
    }

    private static bool ValidateIpAddress(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out _);
    }
}
