using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Processing;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Export.UnityProjects.Project
{
	public sealed class CopyBaseProject : IPostExporter
	{
		private static string RUDE_PROJECT_PATH = "Resources/BaseRudeProject.zip";
		private static string RUDE_PROJECT_URL = "https://raw.githubusercontent.com/eternalUnion/VanityReprised/master/BaseProjects/BaseRudeProject.zip";

		private static Stream? GetFileFromUrl(string url)
		{
			using (var client = new HttpClient())
			{
				Task<HttpResponseMessage> downloadTask = client.GetAsync(url);
				downloadTask.Wait(30000);

				if (!downloadTask.IsCompletedSuccessfully)
					return null;

				HttpResponseMessage result = downloadTask.Result;
				if (result == null || !result.IsSuccessStatusCode)
					return null;

				return result.Content.ReadAsStream();
			}
		}

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

			string baseProjectPath = RUDE_PROJECT_PATH;
			string baseProjectUrl = RUDE_PROJECT_URL;

			// Try downloading from internet, fallback to local base project
			/*Logger.Info("Downloading latest base project from the internet...");
			Stream baseProjectStream = GetFileFromUrl(baseProjectUrl);
			if (baseProjectStream != null)
			{
				try
				{
					using (ZipArchive zip = new ZipArchive(baseProjectStream, ZipArchiveMode.Read))
					{
						zip.ExtractToDirectory(settings.ProjectRootPath, true);
					}

					return;
				}
				catch (Exception)
				{
					Logger.Warning("Failed to use the zip from repo, will attempt to copy from local base project");
				}
			}
			else
			{
				Logger.Warning("Failed to download the base project, will attempt to copy from local base project");
			}*/

			if (!File.Exists(baseProjectPath))
			{
				Logger.Error($"Could not find the required file: {baseProjectPath}");
				throw new IOException($"Could not find the required file: {baseProjectPath}");
			}

			using (ZipArchive zip = new ZipArchive(File.Open(baseProjectPath, FileMode.Open, FileAccess.Read), ZipArchiveMode.Read))
			{
				zip.ExtractToDirectory(settings.ProjectRootPath, true);
			}
		}
	}
}
