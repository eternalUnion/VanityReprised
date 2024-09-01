#region Online Update Catalog
using System.Collections.Generic;

public class SpiteUpdateInfo
{
    public string Version { get; set; }
    public string Changelog { get; set; }
    public string Url { get; set; }
    public List<string> FilesToDelete;
}

public class SpiteFileInfo
{
    public string Path { get; set; }
    public string Guid { get; set; }
    public string SHA256 { get; set; }
    public string Url { get; set; }
}

public class SpiteUpdateCatalog
{
    public string CurrentBranch { get; set; }
    public string LatestVersion { get; set; }

    public Dictionary<string, List<SpiteUpdateInfo>> Updates;
    public List<SpiteFileInfo> Files;
}
#endregion

#region Local Version Catalog
public class SpiteVersionInfo
{
    public string CurrentBranch { get; set; }
    public string CurrentVersion { get; set; }

    public List<string> AppliedPatches;
}
#endregion
