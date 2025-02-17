using Amazon.EC2;
using Amazon.EC2.Model;

namespace DynamicCloudFormationTemplate;

public class EC2InstanceFilter
{
    private readonly AmazonEC2Client _ec2Client;

    public EC2InstanceFilter(string region)
    {
        var regionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region);
        _ec2Client = new AmazonEC2Client(regionEndpoint);
    }

    public async Task<List<InstanceTypeInfo>> GetInstanceTypes(int vCpus, int memoryGb)
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
                    Values = new List<string> 
                    { 
                        (memoryGb * 1024).ToString() 
                    }
                }
            }
        };

        var response = await _ec2Client.DescribeInstanceTypesAsync(request);
        return response.InstanceTypes;
    }
}
