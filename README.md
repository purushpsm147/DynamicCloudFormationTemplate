# DynamicCloudFormationTemplate

## Project Overview

DynamicCloudFormationTemplate is a tool for generating AWS CloudFormation YAML templates dynamically. The codebase integrates subnet CIDR suggestions, EC2 instance validations, and configuration management for VPC, EC2, and networking resources.

## Codebase Structure

- **Models**: Contains definitions for configuration classes such as `CloudformationInputs`, `VpcConfig`, `SubnetConfig`, `ServerInstance`, `EC2Config`, and others.
- **Services**: Includes classes like `EC2Service` that interact with AWS SDK APIs, e.g., to validate instance types and get recommended instance types.
- **Managers**: Hosts configuration and helper classes including `EC2ConfigurationManager` and `NetworkConfigurationManager` for assembling the stack configurations.
- **Interfaces**: Defines contracts for services and generators, e.g., `IEC2Service` and `ICloudFormationGenerator`.
- **Generators**: The `YamlTemplateGenerator` is responsible for constructing the YAML CloudFormation template.
- **Utilities**: Additional classes like `CidrValidator` perform CIDR and subnet validation.
- **Entry Point**: The `Program.cs` file initializes and orchestrates the configuration and template generation process.

## Prerequisites

- .NET 6.0 or later.
- AWS SDK for .NET.
- Access to AWS with necessary permissions for CloudFormation, EC2, and IAM.
- An XML inventory file if using inventory-based configurations.

## Installation

1. Clone the repository to your local machine.
2. Restore NuGet packages.
3. Build the solution using your preferred IDE or the CLI:
   ```
   dotnet build
   ```

## Usage

1. Run the application:
   ```
   dotnet run --project DynamicCloudFormationTemplate
   ```
2. Follow the interactive prompts to configure the VPC, subnets, and EC2 instances.
3. The generated YAML template will be saved as `cloudformation-template.yaml` in the project root.

## Deployment

- Use the generated CloudFormation template along with the necessary IAM capabilities:
  ```
  aws cloudformation create-stack --stack-name YourStackName --template-body file://cloudformation-template.yaml --capabilities CAPABILITY_IAM CAPABILITY_NAMED_IAM
  ```

## Codebase Reference

This repository forms the entire codebase for the DynamicCloudFormationTemplate project. Refer to individual folders and files for detailed implementations.

## Detailed Component Overview

### Core Components

1. **YAML Template Generator**
   - Located in `YamlTemplateGenerator.cs`
   - Implements `ICloudFormationGenerator` interface
   - Generates CloudFormation YAML using a StringBuilder
   - Handles resource dependencies and ordering
   - Supports VPC, subnet, EC2, and IAM resource generation

2. **Configuration Managers**
   - `EC2ConfigurationManager.cs`: Handles EC2 instance configuration
     - Validates instance types
     - Manages security group configurations
     - Handles SSH access rules
   - `NetworkConfigurationManager.cs`: Manages network resource configuration
     - CIDR block validation
     - Subnet allocation
     - VPC resource organization

3. **Validation Components**
   - `ValidateEc2Configurations.cs`: EC2 configuration validator
     - Instance type validation
     - AMI validation
     - CIDR block validation
   - `CidrValidator.cs`: Network CIDR validator
     - Subnet CIDR validation
     - CIDR range calculations
     - Subnet suggestion generation

### Models

1. **Network Configuration Models**
   - `VpcConfig`: VPC configuration container
   - `SubnetConfig`: Subnet specification model
   - `SecurityGroupRule`: Security group rule definition

2. **EC2 Configuration Models**
   - `EC2Config`: EC2 instance configuration container
   - `ServerInstance`: Individual server specification
   - `EC2Requirements`: System requirements model

### Services

1. **EC2Service**
   - AWS SDK integration
   - Instance type recommendations
   - Resource validation
   - Region-specific operations

### Utilities

1. **InventoryParser**
   - XML inventory file parsing
   - System specification extraction
   - Network configuration parsing
   - Volume information processing

## Implementation Details

### VPC Configuration
```yaml
# Example VPC Configuration
VpcConfig:
  VpcCidr: "10.0.0.0/16"
  CreatePrivateSubnets: true
  CreateNatGateway: true
  PublicSubnets:
    - CidrBlock: "10.0.1.0/24"
      AvailabilityZoneIndex: 0
    - CidrBlock: "10.0.2.0/24"
      AvailabilityZoneIndex: 1
```

### EC2 Configuration
```yaml
# Example EC2 Configuration
EC2Config:
  EnableSSMRole: true
  AllowedSSHIps: 
    - "10.0.0.0/32"
  Instances:
    - Name: "WebServer"
      InstanceType: "t2.micro"
      Count: 2
```

## Development Guidelines

1. **Resource Generation**
   - Follow AWS best practices for resource dependencies
   - Implement proper IAM roles and permissions
   - Use parameter sections for configurability

2. **Error Handling**
   - Validate all user inputs
   - Provide meaningful error messages
   - Implement proper AWS SDK error handling

3. **Testing**
   - Unit test validation components
   - Integration test AWS SDK interactions
   - Validate generated templates

## Best Practices

1. **Security**
   - Use least privilege IAM roles
   - Implement security group best practices
   - Validate CIDR blocks and IP ranges

2. **Networking**
   - Follow AWS VPC design best practices
   - Implement proper subnet sizing
   - Consider availability zone distribution

3. **Resource Management**
   - Use proper resource naming
   - Implement proper tagging
   - Consider resource dependencies

## Contributing

1. Fork the repository
2. Create a feature branch
3. Submit a pull request with:
   - Clear description of changes
   - Updated documentation
   - Test coverage for new features

## License

This project is licensed under the MIT License - see the LICENSE file for details.