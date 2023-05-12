using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Community.QuickBlocks;

internal class QuickBlocksManifestFilter : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        var assembly = typeof(QuickBlocksManifestFilter).Assembly;

        manifests.Add(new PackageManifest
        {
            PackageName = "QuickBlocks",
            Version = assembly.GetName()?.Version?.ToString(3) ?? "1.0.0",
            AllowPackageTelemetry = true
        });
    }
}
