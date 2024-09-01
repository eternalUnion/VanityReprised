#region Online Update Catalog
using System.Collections.Generic;

public class RudeUpdateInfo
{
    public string Version { get; set; }
    public string Changelog { get; set; }
    public string Url { get; set; }
    public List<string> FilesToDelete;
}

public class RudeFileInfo
{
    public string Path { get; set; }
    public string Guid { get; set; }
    public string SHA256 { get; set; }
    public string Url { get; set; }
}

public class RudeUpdateCatalog
{
    public string CurrentBranch { get; set; }
    public string LatestVersion { get; set; }

    public Dictionary<string, List<RudeUpdateInfo>> Updates;
    public List<RudeFileInfo> Files;
}
#endregion

#region Local Version Catalog
public class RudeVersionInfo
{
    public string CurrentBranch { get; set; }
    public string CurrentVersion { get; set; }

    public List<string> AppliedPatches;
}
#endregion
