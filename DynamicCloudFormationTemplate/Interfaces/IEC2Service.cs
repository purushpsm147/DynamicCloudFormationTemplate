using Amazon.EC2.Model;

namespace DynamicCloudFormationTemplate.Interfaces;

public interface IEC2Service : IDisposable
{
    Task<List<InstanceTypeInfo>> GetRecommendedInstanceTypes(int vCpus, int memoryGb);
    Task<bool> ValidateInstanceType(string instanceType);
    bool ValidateCidrBlock(string cidr);
}
