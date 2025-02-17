using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;

namespace DynamicCloudFormationTemplate;

public class ValidateEc2Configurations : IDisposable
{
    private readonly AmazonEC2Client _ec2Client;
    private bool _disposed = false;

    public ValidateEc2Configurations()
        : this("us-east-1")
    {
    }

    public ValidateEc2Configurations(string region)
    {
        region = string.IsNullOrWhiteSpace(region) ? "us-east-1" : region;

        try
        {
            var regionEndpoint = RegionEndpoint.GetBySystemName(region);
            _ec2Client = new AmazonEC2Client(regionEndpoint);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Failed to initialize EC2 client for region {region}: {ex.Message}");
        }
    }


    //Not required
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

    //Not required
    public async Task<bool> ValidateAmiId(string amiId)
    {
        if (string.IsNullOrWhiteSpace(amiId))
            return false;

        try
        {
            var request = new DescribeImagesRequest
            {
                ImageIds = new List<string> { amiId }
            };

            var response = await _ec2Client.DescribeImagesAsync(request);
            return response.Images.Any();
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
            if (parts.Length != 2) 
            {
                return false;
            }

            // Validate IP part
            string[] ipParts = parts[0].Split('.');
            if (ipParts.Length != 4) 
            {
                return false;
            }

            // Validate subnet mask
            if (!int.TryParse(parts[1], out int mask) || mask < 16 || mask > 28)
            {
                return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _ec2Client?.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}