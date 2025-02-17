using DynamicCloudFormationTemplate.Interfaces;
using DynamicCloudFormationTemplate.Managers;
using DynamicCloudFormationTemplate.Models;
using DynamicCloudFormationTemplate.Services;

namespace DynamicCloudFormationTemplate;

internal class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            var inputs = new CloudformationInputs();
            Console.Write("Enter AWS region (default: us-east-1): ");
            inputs.Region = ReadInput("us-east-1");

            // Initialize services and managers
            IEC2Service ec2Service = new EC2Service(inputs.Region);
            var ec2Manager = new EC2ConfigurationManager(inputs.Region);
            var networkManager = new NetworkConfigurationManager(ec2Service);
            ICloudFormationGenerator generator = new YamlTemplateGenerator(inputs.Region);

            // Configure resources
            if (!await ec2Manager.ConfigureEC2(inputs))
            {
                Console.WriteLine("EC2 configuration validation failed. Exiting...");
                return;
            }

            if (!networkManager.ConfigureNetworking(inputs))
            {
                Console.WriteLine("Network configuration validation failed. Exiting...");
                return;
            }

            // Generate and save template
            string yaml = await generator.GenerateTemplate(inputs);
            await File.WriteAllTextAsync("./cloudformation-template.yaml", yaml);

            Console.WriteLine("Template generated successfully");
            Console.WriteLine("Use --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM when creating the stack");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static string ReadInput(string defaultValue = "")
    {
        string input = Console.ReadLine()?.Trim() ?? "";
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }
}