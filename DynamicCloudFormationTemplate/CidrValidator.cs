using System.Net;

namespace DynamicCloudFormationTemplate;

public class CidrValidator
{
    public bool IsValidSubnetCidr(string vpcCidr, string subnetCidr)
    {
        try
        {
            // Parse CIDR blocks
            string[] vpcParts = vpcCidr.Split('/');
            string[] subnetParts = subnetCidr.Split('/');
            
            IPAddress vpcIp = IPAddress.Parse(vpcParts[0]);
            IPAddress subnetIp = IPAddress.Parse(subnetParts[0]);
            int vpcMask = int.Parse(vpcParts[1]);
            int subnetMask = int.Parse(subnetParts[1]);

            // Validate masks
            if (subnetMask <= vpcMask || subnetMask > 28)
            {
                Console.WriteLine($"Subnet mask /{subnetMask} must be larger than VPC mask /{vpcMask} and not larger than /28");
                return false;
            }

            // Get network addresses
            uint vpcAddr = BitConverter.ToUInt32(vpcIp.GetAddressBytes().Reverse().ToArray(), 0);
            uint subnetAddr = BitConverter.ToUInt32(subnetIp.GetAddressBytes().Reverse().ToArray(), 0);

            // Calculate network portions using VPC mask for both
            uint vpcNetwork = vpcAddr & (uint.MaxValue << (32 - vpcMask));
            uint subnetNetwork = subnetAddr & (uint.MaxValue << (32 - vpcMask));

            if (vpcNetwork != subnetNetwork)
            {
                Console.WriteLine($"Subnet {subnetCidr} is not within VPC network range {vpcCidr}");
                var vpcNetworkIp = new IPAddress(BitConverter.GetBytes(vpcNetwork).Reverse().ToArray());
                Console.WriteLine($"VPC network portion: {vpcNetworkIp}/{vpcMask}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating CIDR: {ex.Message}");
            return false;
        }
    }

    public List<string> SuggestSubnetCidrs(string vpcCidr, int count)
    {
        var suggestions = new List<string>();
        string[] vpcParts = vpcCidr.Split('/');
        IPAddress vpcIp = IPAddress.Parse(vpcParts[0]);
        int vpcMask = int.Parse(vpcParts[1]);

        // Get the VPC network address
        byte[] vpcBytes = vpcIp.GetAddressBytes();
        uint vpcAddr = BitConverter.ToUInt32(vpcBytes.Reverse().ToArray(), 0);
        uint vpcNetwork = vpcAddr & (uint.MaxValue << (32 - vpcMask));

        // Calculate subnet mask (VPC mask + 4 to create subnets)
        int subnetMask = Math.Min(vpcMask + 4, 28);
        uint subnetSize = 1u << (32 - subnetMask);

        for (int i = 0; i < count; i++)
        {
            uint subnetAddr = vpcNetwork + (subnetSize * (uint)i);
            byte[] subnetBytes = BitConverter.GetBytes(subnetAddr).Reverse().ToArray();
            var subnetIp = new IPAddress(subnetBytes);
            suggestions.Add($"{subnetIp}/{subnetMask}");
        }

        return suggestions;
    }

    public List<string> SuggestSubnetCidrs(string vpcCidr, int count, int offset = 0)
    {
        var suggestions = new List<string>();
        string[] vpcParts = vpcCidr.Split('/');
        IPAddress vpcIp = IPAddress.Parse(vpcParts[0]);
        int vpcMask = int.Parse(vpcParts[1]);

        // Get the VPC network address
        byte[] vpcBytes = vpcIp.GetAddressBytes();
        uint vpcAddr = BitConverter.ToUInt32(vpcBytes.Reverse().ToArray(), 0);
        uint vpcNetwork = vpcAddr & (uint.MaxValue << (32 - vpcMask));

        // Calculate subnet mask (VPC mask + 4 to create subnets)
        int subnetMask = Math.Min(vpcMask + 4, 28);
        uint subnetSize = 1u << (32 - subnetMask);

        for (int i = 0; i < count; i++)
        {
            uint subnetAddr = vpcNetwork + (subnetSize * (uint)(i + offset));
            byte[] subnetBytes = BitConverter.GetBytes(subnetAddr).Reverse().ToArray();
            var subnetIp = new IPAddress(subnetBytes);
            suggestions.Add($"{subnetIp}/{subnetMask}");
        }

        return suggestions;
    }
}
