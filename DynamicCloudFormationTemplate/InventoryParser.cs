using System.Xml.Linq;

namespace DynamicCloudFormationTemplate;

public class InventoryParser
{
    private readonly XDocument _doc;

    public InventoryParser(string xmlPath)
    {
        // Use FileStream with using to load the XML efficiently
        using var stream = File.OpenRead(xmlPath);
        _doc = XDocument.Load(stream);
    }

    public Dictionary<string, string> GetSystemInfo()
    {
        var info = new Dictionary<string, string>();
        
        // System specifications
        info["MemoryInMB"] = _doc.Descendants("PhysicalMemory")
            .First().Attribute("Value")?.Value ?? "4096";
            
        info["NumProcessors"] = _doc.Descendants("NumProcessors")
            .First().Attribute("Value")?.Value ?? "2";

        info["WindowsVersion"] = _doc.Descendants("WindowsVersionString")
            .First().Attribute("Value")?.Value ?? "";

        // Network configuration
        var primaryAdapter = _doc.Descendants("Adapter").First();
        info["PrimaryIP"] = primaryAdapter
            .Descendants("IP")
            .First()
            .Element("Address")
            ?.Attribute("Value")?.Value ?? "";

        info["SubnetMask"] = primaryAdapter
            .Descendants("IP")
            .First()
            .Element("Subnet")
            ?.Attribute("Value")?.Value ?? "";

        info["Gateway"] = primaryAdapter
            .Descendants("Gateway")
            .First()
            .Element("Address")
            ?.Attribute("Value")?.Value ?? "";

        return info;
    }

    public List<string> GetVolumes()
    {
        return _doc.Descendants("VolumeInformation")
            .Select(v => v.Element("DriveName")?.Attribute("Value")?.Value ?? "")
            .Where(d => !string.IsNullOrEmpty(d))
            .ToList();
    }

    public bool IsVirtualMachine()
    {
        return _doc.Descendants("VMWareGuest")
            .Any(v => v.Element("InsideVMWare")?.Attribute("Value")?.Value == "-1");
    }
}
