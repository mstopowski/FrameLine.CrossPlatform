using System.Runtime.InteropServices;
using Rhino.PlugIns;

// Plug-in Description Attributes - all of these are optional
// These will show in Rhino's option dialog, in the tab Plug-ins
[assembly: PlugInDescription(DescriptionType.Address, "")]
[assembly: PlugInDescription(DescriptionType.Country, "Poland")]
[assembly: PlugInDescription(DescriptionType.Email, "maurycy.stopowski@gmail.com")]
[assembly: PlugInDescription(DescriptionType.Phone, "+48 799-775-742")]
[assembly: PlugInDescription(DescriptionType.Organization, "")]
[assembly: PlugInDescription(DescriptionType.UpdateUrl, "")]
[assembly: PlugInDescription(DescriptionType.WebSite, "")]

// Rhino requires a Guid assigned to the assembly.
[assembly: Guid("80D528E3-ACE9-4F57-8185-BE5D4D2E8B76")]