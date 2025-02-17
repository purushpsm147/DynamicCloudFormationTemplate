using DynamicCloudFormationTemplate.Models;

namespace DynamicCloudFormationTemplate.Interfaces;

public interface ICloudFormationGenerator
{
    Task<string> GenerateTemplate(CloudformationInputs inputs);
}
