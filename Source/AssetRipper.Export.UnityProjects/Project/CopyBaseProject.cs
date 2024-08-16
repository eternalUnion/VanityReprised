using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Processing;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Export.UnityProjects.Project
{
	public sealed class CopyBaseProject : IPostExporter
	{
		private static string SPITE_PROJECT_PATH = "Resources/BaseSpiteProject.zip";

		public void DoPostExport(GameData gameData, LibraryConfiguration settings)
		{
			List<string> scriptsToKeep = new List<string>()
			{
				"Assembly-CSharp",
				"Naelstrof.JigglePhysics",
				"NewBlood.LegacyInput",
				"pcon.core",
				"plog",
				"plog.unity",
			};

			string scriptsDirectory = Path.Combine(settings.AssetsPath, "Scripts");
			if (Directory.Exists(scriptsDirectory))
			{
				foreach (string dir in Directory.GetDirectories(scriptsDirectory))
				{
					if (!scriptsToKeep.Contains(Path.GetFileName(dir)))
						Directory.Delete(dir, true);
				}
			}

			if (!File.Exists(SPITE_PROJECT_PATH))
				throw new IOException($"Could not find the required file: {SPITE_PROJECT_PATH}");

			using (ZipArchive zip = new ZipArchive(File.Open(SPITE_PROJECT_PATH, FileMode.Open, FileAccess.Read), ZipArchiveMode.Read))
			{
				zip.ExtractToDirectory(settings.ProjectRootPath, true);
			}
		}
	}
}
