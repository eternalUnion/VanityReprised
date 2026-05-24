using AssetRipper.AccurateShaders;
using AssetRipper.Assets.Bundles;
using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.Export.UnityProjects.PathIdMapping;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.Export.UnityProjects.Scripts;
using AssetRipper.Import.Configuration;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Processing;
using AssetRipper.Processing.AnimatorControllers;
using AssetRipper.Processing.Assemblies;
using AssetRipper.Processing.AudioMixers;
using AssetRipper.Processing.Editor;
using AssetRipper.Processing.PrefabOutlining;
using AssetRipper.Processing.Scenes;
using AssetRipper.Processing.Textures;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System.Text;

namespace AssetRipper.Export.UnityProjects;

public class ExportHandler
{
	protected LibraryConfiguration Settings { get; }

	public ExportHandler(LibraryConfiguration settings)
	{
		Settings = settings;
	}

	public GameData Load(IReadOnlyList<string> paths)
	{
		Settings.ImportSettings = new ImportSettings()
		{
			IgnoreStreamingAssets = false,
			ScriptContentLevel = ScriptContentLevel.Level1,

			// Preserve custom settings
			Export_campaign_scenes_intermission1 = Settings.ImportSettings.Export_campaign_scenes_intermission1,
			Export_campaign_scenes_intermission2 = Settings.ImportSettings.Export_campaign_scenes_intermission2,
			Export_campaign_scenes_level0_1 = Settings.ImportSettings.Export_campaign_scenes_level0_1,
			Export_campaign_scenes_level0_2 = Settings.ImportSettings.Export_campaign_scenes_level0_2,
			Export_campaign_scenes_level0_3 = Settings.ImportSettings.Export_campaign_scenes_level0_3,
			Export_campaign_scenes_level0_4 = Settings.ImportSettings.Export_campaign_scenes_level0_4,
			Export_campaign_scenes_level0_5 = Settings.ImportSettings.Export_campaign_scenes_level0_5,
			Export_campaign_scenes_level0_s = Settings.ImportSettings.Export_campaign_scenes_level0_s,
			Export_campaign_scenes_level0_e = Settings.ImportSettings.Export_campaign_scenes_level0_e,
			Export_campaign_scenes_level1_1 = Settings.ImportSettings.Export_campaign_scenes_level1_1,
			Export_campaign_scenes_level1_2 = Settings.ImportSettings.Export_campaign_scenes_level1_2,
			Export_campaign_scenes_level1_3 = Settings.ImportSettings.Export_campaign_scenes_level1_3,
			Export_campaign_scenes_level1_4 = Settings.ImportSettings.Export_campaign_scenes_level1_4,
			Export_campaign_scenes_level1_s = Settings.ImportSettings.Export_campaign_scenes_level1_s,
			Export_campaign_scenes_level1_e = Settings.ImportSettings.Export_campaign_scenes_level1_e,
			Export_campaign_scenes_level2_1 = Settings.ImportSettings.Export_campaign_scenes_level2_1,
			Export_campaign_scenes_level2_2 = Settings.ImportSettings.Export_campaign_scenes_level2_2,
			Export_campaign_scenes_level2_3 = Settings.ImportSettings.Export_campaign_scenes_level2_3,
			Export_campaign_scenes_level2_4 = Settings.ImportSettings.Export_campaign_scenes_level2_4,
			Export_campaign_scenes_level2_s = Settings.ImportSettings.Export_campaign_scenes_level2_s,
			Export_campaign_scenes_level3_1 = Settings.ImportSettings.Export_campaign_scenes_level3_1,
			Export_campaign_scenes_level3_2 = Settings.ImportSettings.Export_campaign_scenes_level3_2,
			Export_campaign_scenes_level4_1 = Settings.ImportSettings.Export_campaign_scenes_level4_1,
			Export_campaign_scenes_level4_2 = Settings.ImportSettings.Export_campaign_scenes_level4_2,
			Export_campaign_scenes_level4_3 = Settings.ImportSettings.Export_campaign_scenes_level4_3,
			Export_campaign_scenes_level4_4 = Settings.ImportSettings.Export_campaign_scenes_level4_4,
			Export_campaign_scenes_level4_s = Settings.ImportSettings.Export_campaign_scenes_level4_s,
			Export_campaign_scenes_level5_1 = Settings.ImportSettings.Export_campaign_scenes_level5_1,
			Export_campaign_scenes_level5_2 = Settings.ImportSettings.Export_campaign_scenes_level5_2,
			Export_campaign_scenes_level5_3 = Settings.ImportSettings.Export_campaign_scenes_level5_3,
			Export_campaign_scenes_level5_4 = Settings.ImportSettings.Export_campaign_scenes_level5_4,
			Export_campaign_scenes_level5_s = Settings.ImportSettings.Export_campaign_scenes_level5_s,
			Export_campaign_scenes_level6_1 = Settings.ImportSettings.Export_campaign_scenes_level6_1,
			Export_campaign_scenes_level6_2 = Settings.ImportSettings.Export_campaign_scenes_level6_2,
			Export_campaign_scenes_level7_1 = Settings.ImportSettings.Export_campaign_scenes_level7_1,
			Export_campaign_scenes_level7_2 = Settings.ImportSettings.Export_campaign_scenes_level7_2,
			Export_campaign_scenes_level7_3 = Settings.ImportSettings.Export_campaign_scenes_level7_3,
			Export_campaign_scenes_level7_4 = Settings.ImportSettings.Export_campaign_scenes_level7_4,
			Export_campaign_scenes_level7_s = Settings.ImportSettings.Export_campaign_scenes_level7_s,
			Export_campaign_scenes_level8_1 = Settings.ImportSettings.Export_campaign_scenes_level8_1,
			Export_campaign_scenes_level8_2 = Settings.ImportSettings.Export_campaign_scenes_level8_2,
			Export_campaign_scenes_level8_3 = Settings.ImportSettings.Export_campaign_scenes_level8_3,
			Export_campaign_scenes_level8_4 = Settings.ImportSettings.Export_campaign_scenes_level8_4,
			Export_campaign_scenes_levelp_1 = Settings.ImportSettings.Export_campaign_scenes_levelp_1,
			Export_campaign_scenes_levelp_2 = Settings.ImportSettings.Export_campaign_scenes_levelp_2,
			Export_specialscenes_scenes_creditsmuseum2 = Settings.ImportSettings.Export_specialscenes_scenes_creditsmuseum2,
			Export_specialscenes_scenes_earlyaccessend = Settings.ImportSettings.Export_specialscenes_scenes_earlyaccessend,
			Export_specialscenes_scenes_endless = Settings.ImportSettings.Export_specialscenes_scenes_endless,
			Export_specialscenes_scenes_intro = Settings.ImportSettings.Export_specialscenes_scenes_intro,
			Export_specialscenes_scenes_mainmenu = Settings.ImportSettings.Export_specialscenes_scenes_mainmenu,
			Export_specialscenes_scenes_tundraassets = Settings.ImportSettings.Export_specialscenes_scenes_tundraassets,
			Export_specialscenes_scenes_tutorial = Settings.ImportSettings.Export_specialscenes_scenes_tutorial,
			Export_specialscenes_scenes_uk_construct = Settings.ImportSettings.Export_specialscenes_scenes_uk_construct,
		};

		Settings.ExportSettings = new ExportSettings()
		{
			AudioExportFormat = AudioExportFormat.Default,
			ImageExportFormat = Modules.Textures.ImageExportFormat.Png,
			LightmapTextureExportFormat = LightmapTextureExportFormat.Image,
			SaveSettingsToDisk = true,
			ScriptExportMode = ScriptExportMode.Decompiled,
			ScriptLanguageVersion = ScriptLanguageVersion.AutoSafe,
			ShaderExportMode = ShaderExportMode.Dummy,
			SpriteExportMode = SpriteExportMode.Native,
			TextExportMode = TextExportMode.Parse,
		};

		Settings.ProcessingSettings = new Processing.Configuration.ProcessingSettings()
		{
			BundledAssetsExportMode = Processing.Configuration.BundledAssetsExportMode.DirectExport,
			EnableAssetDeduplication = true,
			EnablePrefabOutlining = false,
			EnableStaticMeshSeparation = true,
		};

		Settings.SaveToDefaultPath();

		if (paths.Count == 1)
		{
			Logger.Info(LogCategory.Import, $"Attempting to read files from {paths[0]}");
		}
		else
		{
			Logger.Info(LogCategory.Import, $"Attempting to read files from {paths.Count} paths...");
		}

		GameStructure gameStructure = GameStructure.Load(paths, Settings);
		GameData gameData = GameData.FromGameStructure(gameStructure);
		Logger.Info(LogCategory.Import, "Finished reading files");
		return gameData;
	}

	public void Process(GameData gameData)
	{
		Logger.Info(LogCategory.Processing, "Processing loaded assets...");
		foreach (IAssetProcessor processor in GetProcessors())
		{
			processor.Process(gameData);
		}
		Logger.Info(LogCategory.Processing, "Finished processing assets");
	}

	protected virtual IEnumerable<IAssetProcessor> GetProcessors()
	{
		if (Settings.ImportSettings.ScriptContentLevel == ScriptContentLevel.Level1)
		{
			yield return new MethodStubbingProcessor();
		}
		yield return new SceneDefinitionProcessor();
		yield return new MainAssetProcessor();
		yield return new AnimatorControllerProcessor();
		yield return new AudioMixerProcessor();
		yield return new EditorFormatProcessor(Settings.ProcessingSettings.BundledAssetsExportMode);
		//Static mesh separation goes here
		if (Settings.ProcessingSettings.EnablePrefabOutlining)
		{
			yield return new PrefabOutliningProcessor();
		}
		yield return new LightingDataProcessor();//Needs to be after static mesh separation
		yield return new PrefabProcessor();
		yield return new SpriteProcessor();

		// Custom processor to match original paths from the addressable catalog
		yield return new ResolveAssetPaths();
		yield return new MergePackageAssets();
		yield return new RemoveAssetBundleNames();
	}

	public void Export(GameData gameData, string outputPath)
	{
		Logger.Info(LogCategory.Export, "Starting export");
		Logger.Info(LogCategory.Export, $"Attempting to export assets to {outputPath}...");
		Logger.Info(LogCategory.Export, $"Game files have these Unity versions:{GetListOfVersions(gameData.GameBundle)}");
		Logger.Info(LogCategory.Export, $"Exporting to Unity version {gameData.ProjectVersion}");

		Settings.ExportRootPath = outputPath;
		Settings.SetProjectSettings(gameData.ProjectVersion, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);

		ProjectExporter projectExporter = new(Settings, gameData.AssemblyManager);
		BeforeExport(projectExporter);
		projectExporter.DoFinalOverrides(Settings);
		projectExporter.Export(gameData.GameBundle, Settings);

		Logger.Info(LogCategory.Export, "Finished exporting assets");

		foreach (IPostExporter postExporter in GetPostExporters())
		{
			postExporter.DoPostExport(gameData, Settings);
		}
		Logger.Info(LogCategory.Export, "Finished post-export");

		static string GetListOfVersions(GameBundle gameBundle)
		{
			return string.Join(' ', gameBundle
				.FetchAssetCollections()
				.Select(c => c.Version)
				.Distinct()
				.Select(v => v.ToString()));
		}
	}

	public void ExportScenesOnly(GameData gameData, string outputPath)
	{
		Logger.Info(LogCategory.Export, "Starting export");
		Logger.Info(LogCategory.Export, $"Attempting to export unitypackage to {outputPath}...");
		Logger.Info(LogCategory.Export, $"Game files have these Unity versions:{GetListOfVersions(gameData.GameBundle)}");
		Logger.Info(LogCategory.Export, $"Exporting to Unity version {gameData.ProjectVersion}");

		string tempPath = Path.GetFullPath("temp");
		if (Directory.Exists(tempPath))
			Directory.Delete(tempPath, true);
		Directory.CreateDirectory(tempPath);

		Settings.ExportRootPath = tempPath;
		Settings.SetProjectSettings(gameData.ProjectVersion, BuildTarget.NoTarget, TransferInstructionFlags.NoTransferInstructionFlags);

		UnityPackageExporter packageExporter = new UnityPackageExporter(Settings, gameData.AssemblyManager);
		packageExporter.DoFinalOverrides(Settings);
		packageExporter.Export(gameData.GameBundle, Settings);

		Logger.Info(LogCategory.Export, "Finished exporting assets, creating archive");

		using var outFileStream = File.OpenWrite(outputPath);
		using var gzoStream = new GZipOutputStream(outFileStream);
		gzoStream.FileName = "archtemp.tar";
		using var tarArchive = new TarOutputStream(gzoStream, Encoding.UTF8);

		Stack<(string absoluteDir, string relativeDir)> directoriesToVisit = new();
		directoriesToVisit.Push((tempPath, string.Empty));
		while (directoriesToVisit.Count > 0)
		{
			(string currentDir, string relativeDir) = directoriesToVisit.Pop();
			foreach (string subDir in Directory.GetDirectories(currentDir))
			{
				directoriesToVisit.Push((subDir, string.IsNullOrEmpty(relativeDir) ? Path.GetFileName(subDir) : $"{relativeDir}/{Path.GetFileName(subDir)}"));
			}

			foreach (string metaFilePath in Directory.GetFiles(currentDir).Where(file => file.EndsWith(".meta")))
			{
				string assetPath = metaFilePath.Substring(0, metaFilePath.Length - ".meta".Length);
				if (!File.Exists(assetPath))
					continue;

				string? guidLine = File.ReadAllLines(metaFilePath).FirstOrDefault(line => line.StartsWith("guid: "));
				if (guidLine == null)
					continue;

				string guid = guidLine.Substring("guid: ".Length, guidLine.Length - "guid: ".Length).Trim();
				if (!Guid.TryParse(guid, out _))
					continue;

				var assetEntry = TarEntry.CreateEntryFromFile(assetPath);
				assetEntry.Name = $"{guid}/asset";
				tarArchive.PutNextEntry(assetEntry);
				using (FileStream assetStream = File.OpenRead(assetPath))
					assetStream.CopyTo(tarArchive);
				tarArchive.CloseEntry();

				var metaEntry = TarEntry.CreateEntryFromFile(metaFilePath);
				metaEntry.Name = $"{guid}/asset.meta";
				tarArchive.PutNextEntry(metaEntry);
				using (FileStream metaFileStream = File.OpenRead(metaFilePath))
					metaFileStream.CopyTo(tarArchive);
				tarArchive.CloseEntry();

				string assetRelativePath = (string.IsNullOrEmpty(relativeDir)) ? Path.GetFileName(assetPath) : $"{relativeDir}/{Path.GetFileName(assetPath)}";
				byte[] bytes = Encoding.UTF8.GetBytes(assetRelativePath);
				
				var pathEntry = TarEntry.CreateTarEntry($"{guid}/pathname");
				pathEntry.Size = bytes.Length;
				tarArchive.PutNextEntry(pathEntry);
				tarArchive.Write(bytes, 0, bytes.Length);
				tarArchive.CloseEntry();
			}
		}

		tarArchive.Finish();

		Logger.Info(LogCategory.Export, "Finished writing archive");

		static string GetListOfVersions(GameBundle gameBundle)
		{
			return string.Join(' ', gameBundle
				.FetchAssetCollections()
				.Select(c => c.Version)
				.Distinct()
				.Select(v => v.ToString()));
		}
	}

	protected virtual void BeforeExport(ProjectExporter projectExporter)
	{
	}

	protected virtual IEnumerable<IPostExporter> GetPostExporters()
	{
		yield return new ProjectVersionPostExporter();
		yield return new PackageManifestPostExporter();
		// yield return new StreamingAssetsPostExporter();
		// yield return new DllPostExporter();
		// yield return new PathIdMapExporter();
		yield return new DeleteSourceGeneratedScripts();
		yield return new CopyBaseProject();

		AccurateShaderExporter.alreadyExportedBinaries = false;

		if (AccurateShaderDefinition.AccurateShaders_2022_3_28f1.Export)
			yield return new AccurateShaderExporter(AccurateShaderDefinition.AccurateShaders_2022_3_28f1);

		if (AccurateShaderDefinition.AccurateShaders_2022_3_29f1.Export)
			yield return new AccurateShaderExporter(AccurateShaderDefinition.AccurateShaders_2022_3_29f1);
	}

	public GameData LoadAndProcess(IReadOnlyList<string> paths)
	{
		GameData gameData = Load(paths);
		Process(gameData);
		return gameData;
	}

	public void LoadProcessAndExport(IReadOnlyList<string> inputPaths, string outputPath)
	{
		GameData gameData = LoadAndProcess(inputPaths);
		Export(gameData, outputPath);
	}

	public void ThrowIfSettingsDontMatch(LibraryConfiguration settings)
	{
		if (Settings != settings)
		{
			throw new ArgumentException("Settings don't match");
		}
	}
}
