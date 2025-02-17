using Amazon.EC2;
using Amazon.EC2.Model;
using DynamicCloudFormationTemplate.Interfaces;

namespace DynamicCloudFormationTemplate.Services;

public class EC2Service : IEC2Service
{
    private readonly AmazonEC2Client _ec2Client;
    private bool _disposed;

    public EC2Service(string region)
    {
        var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
        _ec2Client = new AmazonEC2Client(regionEndpoint);
    }

    public async Task<List<InstanceTypeInfo>> GetRecommendedInstanceTypes(int vCpus, int memoryGb)
    {
        var request = new DescribeInstanceTypesRequest
        {
            Filters = new List<Filter>
            {
                new Filter
                {
                    Name = "vcpu-info.default-vcpus",
                    Values = new List<string> { vCpus.ToString() }
                },
                new Filter
                {
                    Name = "memory-info.size-in-mib",
                    Values = new List<string> { (memoryGb * 1024).ToString() }
                }
            }
        };

        var response = await _ec2Client.DescribeInstanceTypesAsync(request);
        return response.InstanceTypes
            .OrderBy(i => i.MemoryInfo.SizeInMiB)
            .ThenBy(i => i.VCpuInfo.DefaultVCpus)
            .ToList();
    }

    public async Task<bool> ValidateInstanceType(string instanceType)
    {
        if (string.IsNullOrWhiteSpace(instanceType))
            return false;

        try
        {
            var request = new DescribeInstanceTypesRequest
            {
                InstanceTypes = new List<string> { instanceType }
            };

            var response = await _ec2Client.DescribeInstanceTypesAsync(request);
            return response.InstanceTypes.Any();
        }
        catch
        {
            return false;
        }
    }

    public bool ValidateCidrBlock(string cidr)
    {
        if (string.IsNullOrWhiteSpace(cidr)) return false;

        try
        {
            string[] parts = cidr.Split('/');
            if (parts.Length != 2) return false;

            string[] ipParts = parts[0].Split('.');
            if (ipParts.Length != 4) return false;

            if (!int.TryParse(parts[1], out int mask) || mask < 16 || mask > 28)
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _ec2Client?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
