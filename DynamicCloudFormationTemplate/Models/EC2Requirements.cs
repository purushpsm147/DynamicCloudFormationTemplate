namespace DynamicCloudFormationTemplate.Models;

public class EC2Requirements
{
    public int VCpus { get; set; }
    public int MemoryGb { get; set; }
    public string Region { get; set; }
}
