using AssetRipper.PackageGuid;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AssetRipper.PackageGuid.Generator
{
	internal class Program
	{
		private static IEnumerable<string> GetAllFilesRecursive(string path)
		{
			foreach (string file in Directory.GetFiles(path))
				yield return file;
			foreach (string folder in Directory.GetDirectories(path))
				foreach (string subFile in GetAllFilesRecursive(folder))
					yield return subFile;
		}

		private static string GetGuidFromMeta(string path)
		{
			string text = File.ReadAllText(path);
			int index = text.IndexOf("guid: ") + 6;
			string guid = text.Substring(index, 32);

			return guid;
		}

		static void Main(string[] args)
		{
			Console.WriteLine("Package cache dirs:");
			List<string> dirs = new List<string>();

			while (true)
			{
				string dir = Console.ReadLine();
				if (string.IsNullOrEmpty(dir))
					break;

				if (!Directory.Exists(dir))
				{
					Console.WriteLine("Invalid directory");
					continue;
				}
				dirs.Add(dir);
			}

			PackageGuids data = new PackageGuids();
			data.Files = new List<FileData>();
			data.Shaders = new List<ShaderData>();
			data.Scripts = new List<ScriptData>();

			foreach (string dir in dirs)
			{
				foreach (string filePath in GetAllFilesRecursive(dir))
				{
					string ext = Path.GetExtension(filePath);

					if (ext == ".meta"
						|| ext == ".asmdef" 
						|| ext == ""
						|| ext == ".json"
						|| ext == ".txt"
						|| ext == ".psd"
						|| ext == ".unitypackage")
						continue;

					if (!File.Exists(filePath + ".meta"))
						continue;

					if (ext == ".cs")
					{
						ScriptData scriptData = new ScriptData();
						scriptData.ClassName = Path.GetFileNameWithoutExtension(filePath);

						if (scriptData.ClassName == "AssemblyInfo")
							continue;

						string @namespace = "";
						string script = File.ReadAllText(filePath);
						int namespaceIndex = script.IndexOf("namespace");
						if (namespaceIndex != -1)
						{
							int namespaceBegin = namespaceIndex + "namespace".Length;
							int namespaceEnd = script.IndexOf('{', namespaceIndex + 1) - 1;
							if (namespaceEnd >= 0)
								@namespace = script.Substring(namespaceBegin, namespaceEnd - namespaceBegin + 1).Trim();
						}

						scriptData.Namespace = @namespace;
						scriptData.Guid = GetGuidFromMeta(filePath + ".meta");

						data.Scripts.Add(scriptData);
					}
					else if (ext == ".shader")
					{
						Regex shaderNameRegex = new Regex("Shader\\s+\"([^\"]*)\"");
						var match = shaderNameRegex.Match(File.ReadAllText(filePath));

						if (!match.Success)
						{
							Console.WriteLine($"Could not read shader {Path.GetFileNameWithoutExtension(filePath)}");
							continue;
						}

						ShaderData shaderData = new ShaderData();
						shaderData.ShaderName = match.Groups[1].Value;
						shaderData.Guid = GetGuidFromMeta(filePath + ".meta");

						data.Shaders.Add(shaderData);
					}
					else
					{
						FileData fileData = new FileData();
						fileData.FileName = Path.GetFileName(filePath);
						fileData.Guid = GetGuidFromMeta(filePath + ".meta");

						data.Files.Add(fileData);
					}
				}
			}

			File.WriteAllText("out.json", JsonConvert.SerializeObject(data, Formatting.Indented));

			Console.WriteLine();
			Console.WriteLine("Done.");
		}
	}
}
