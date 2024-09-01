using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RudeExporterSettings : ScriptableObject
{
	public static string DefaultBackupFolderPath
	{
		get => Path.GetFullPath("Backups");
	}

	public string buildPath = "";
	public bool promptBeforeExport = false;
	public bool summaryAfterExport = false;
	public bool checkForChanges = false;

	public bool enableBackups = true;
	public string backupPath = DefaultBackupFolderPath;
	public int maxBackupFolderSizeMB = 8192;
	public int maxSizePerBundleMB = 2048;

	public bool skipLegacyExporterRemoval = false;
	public List<string> legacyBundleLabelsToSkip = new List<string>();
}
