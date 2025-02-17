using Amazon.EC2;
using Amazon.EC2.Model;

namespace DynamicCloudFormationTemplate;

public class EC2InstanceSelector
{
    private readonly AmazonEC2Client _ec2Client;

    public EC2InstanceSelector(string region)
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
                    Values = new List<string> { vCpus.ToString(), (vCpus * 2).ToString() } // Include next size up
                },
                new Filter
                {
                    Name = "memory-info.size-in-mib",
                    Values = new List<string> 
                    { 
                        (memoryGb * 1024).ToString(),
                        ((memoryGb + 4) * 1024).ToString() // Include options with more memory
                    }
                },
                new Filter
                {
                    Name = "current-generation",
                    Values = new List<string> { "true" }
                }
            }
        };

        var response = await _ec2Client.DescribeInstanceTypesAsync(request);
        return response.InstanceTypes
            .OrderBy(i => i.MemoryInfo.SizeInMiB)
            .ThenBy(i => i.VCpuInfo.DefaultVCpus)
            .ToList();
    }
}
