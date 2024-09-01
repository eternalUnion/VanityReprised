using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Linq;
using RudeLevelScripts.Essentials;
using System.Collections.Generic;
using RudeLevelScript;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using System.Text;
using System.Reflection;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Build;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEditor.Build.Pipeline.Utilities;
using System.Text.RegularExpressions;

public class RudeExporter : EditorWindow
{
	private static string[] forbiddenPaths = new string[]
	{
		"Assets/ULTRAKILL Addressables",
		"Assets/ULTRAKILL Others",
		"Assets/ULTRAKILL Prefabs",
		"Assets/Shaders",
	};

	private static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object
	{
		var guids = AssetDatabase.FindAssets($"t:{typeof(T)}");
		foreach (var t in guids)
		{
			var assetPath = AssetDatabase.GUIDToAssetPath(t);
			var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
			if (asset != null)
			{
				yield return asset;
			}
		}
	}

	// Unity style paths starting with 'Asset/', given folder path should NOT end with a slash.
	private static IEnumerable<string> GetFilesRecursive(string folderPath)
	{
		foreach (string file in Directory.GetFiles(folderPath))
			yield return $"{folderPath}/{Path.GetFileName(file)}";

		foreach (string folder in Directory.GetDirectories(folderPath))
			foreach (string subFile in GetFilesRecursive($"{folderPath}/{Path.GetFileName(folder)}"))
				yield return subFile;
	}

	private static void SetGuid(string filePath, string newGuid)
	{
		if (!File.Exists(filePath))
			return;

		string text = File.ReadAllText(filePath);
		StringBuilder sb = new StringBuilder(text);

		int index = text.IndexOf("guid: ");
		if (index == -1)
			return;

		for (int i = 0; i < 32; i++)
			sb[i + index + "guid: ".Length] = newGuid[i];

		File.WriteAllText(filePath, sb.ToString());
	}

	private static bool ApplyGuidChange(string filePath, Dictionary<string, string> guidMap)
	{
		StringBuilder text = new StringBuilder(File.ReadAllText(filePath));
		int currentIndex = SbIndexOf(text, "guid: ", 0, false);
		bool changed = false;

		while (currentIndex != -1)
		{
			string oldGuid = text.ToString(currentIndex + 6, 32);
			if (guidMap.TryGetValue(oldGuid, out string newGuid))
			{
				changed = true;
				int index = currentIndex + 6;
				for (int i = 0; i < 32; i++)
					text[i + index] = newGuid[i];
			}

			currentIndex = SbIndexOf(text, "guid: ", currentIndex + 1, false);
		}

		if (!changed)
			return false;

		File.WriteAllText(filePath, text.ToString());
		return true;
	}

	private static void RemoveAssetBundleLabel(string filePath)
	{
		if (!File.Exists(filePath))
			return;

		string text = File.ReadAllText(filePath);

		int startIndex = text.IndexOf("assetBundleName: ");
		if (startIndex == -1)
			return;
		startIndex += "assetBundleName: ".Length;

		int endIndex = text.IndexOf('\r', startIndex);
		if (endIndex == -1)
		{
			endIndex = text.IndexOf('\n', startIndex);
			if (endIndex == -1)
				return;
		}

		text = text.Remove(startIndex, endIndex - startIndex);
		File.WriteAllText(filePath, text);
	}

	private static T GetOrCreateSchemaFromTemplate<T>(AddressableAssetGroup group, T template, bool postEvents = true) where T : AddressableAssetGroupSchema
	{
		T schema = (T)group.GetSchema(typeof(T));
		if (schema == null)
			schema = (T)group.AddSchema(template, postEvents);

		return schema;
	}

	private static bool TryGetOpenScene(string path, out Scene scene)
	{
		for (int i = 0; i < EditorSceneManager.sceneCount; i++)
		{
			if (EditorSceneManager.GetSceneAt(i).path == path)
			{
				scene = EditorSceneManager.GetSceneAt(i);
				return true;
			}
		}

		scene = new Scene();
		return false;
	}

	private static string RemoveTags(string richText)
	{
		return string.IsNullOrEmpty(richText) ? richText : new Regex(@"<[^>]*>").Replace(richText, string.Empty);
	}

	private static int SbIndexOf(StringBuilder sb, string value, int startIndex, bool ignoreCase)
	{
		int index;
		int length = value.Length;
		int maxSearchLength = (sb.Length - length) + 1;

		if (ignoreCase)
		{
			for (int i = startIndex; i < maxSearchLength; ++i)
			{
				if (Char.ToLower(sb[i]) == Char.ToLower(value[0]))
				{
					index = 1;
					while ((index < length) && (Char.ToLower(sb[i + index]) == Char.ToLower(value[index])))
						++index;

					if (index == length)
						return i;
				}
			}

			return -1;
		}

		for (int i = startIndex; i < maxSearchLength; ++i)
		{
			if (sb[i] == value[0])
			{
				index = 1;
				while ((index < length) && (sb[i + index] == value[index]))
					++index;

				if (index == length)
					return i;
			}
		}

		return -1;
	}

	private static string GetSafeFileName(string fileName)
	{
		StringBuilder sb = new StringBuilder();

		int extIndx = fileName.IndexOf('.');
		bool space = false;
		foreach (char c in (extIndx == -1 ? fileName : fileName.Substring(0, extIndx)))
		{
			if (char.IsLetter(c) || char.IsNumber(c))
			{
				sb.Append(c);
				space = false;
			}
			else if (char.IsWhiteSpace(c) || c == '_')
			{
				if (!space)
					sb.Append('_');
				space = true;
			}
			else
			{
				space = false;
			}
		}

		string result = sb.ToString();
		result = result.TrimStart('_');
		result = result.TrimEnd('_');
		if (string.IsNullOrEmpty(result))
			result = "file";

		return extIndx == -1 ? result : result + fileName.Substring(extIndx);
	}

	private static string GetUniqueFilePath(string path)
	{
		string ext = Path.GetExtension(path);
		string original = path.Substring(0, path.Length - ext.Length);
		string rawFilePath = original;

		int i = 0;
		string result;
		while (File.Exists(result = rawFilePath + ext))
		{
			rawFilePath = $"{original}_{i++}";
		}

		return result;
	}

	private static RudeExporterSettings DefaultExporterSetting {
		get
		{
			RudeExporterSettings exporterSettings = AssetDatabase.LoadAssetAtPath<RudeExporterSettings>("Assets/Editor/Exporter/settings.asset");
			if (exporterSettings == null)
			{
				exporterSettings = ScriptableObject.CreateInstance<RudeExporterSettings>();
				AssetDatabase.CreateAsset(exporterSettings, "Assets/Editor/Exporter/settings.asset");
			}

			return exporterSettings;
		}
	}

	private static class OnlineCatalogManager
    {
		#region JSON Data for Catalog
        #pragma warning disable CS0649
		public class LevelInfo
		{
			public class UpdateInfo
			{
				public string Hash { get; set; }
				public string Message { get; set; }
			}

			public string Name { get; set; }
			public string Author { get; set; }
			public string Guid { get; set; }
			public int Size { get; set; }
			public string Hash { get; set; }
			public string ThumbnailHash { get; set; }

			public string ExternalLink { get; set; }
			public List<string> Parts;
			public long LastUpdate { get; set; }
			public List<UpdateInfo> Updates;
		}

		public class LevelCatalog
		{
			public List<LevelInfo> Levels;
		}
        #pragma warning restore CS0649
		#endregion

		public static LevelCatalog lastCatalog;

        private static int lastCatalogRequestTime = 0;
        private static UnityWebRequestAsyncOperation currentCatalogRequest;
        private static event Action<LevelCatalog> queuedCatalogCallbacks;

		public static void LoadCatalog(Action<LevelCatalog> callback)
        {
            if (lastCatalog != null && (DateTime.Now.Second - lastCatalogRequestTime) < 60)
            {
                callback(lastCatalog);
                return;
            }

            queuedCatalogCallbacks += callback;

            if (currentCatalogRequest != null && !currentCatalogRequest.isDone)
                return;

            lastCatalog = null;
			lastCatalogRequestTime = DateTime.Now.Second;

            UnityWebRequest catalogRequest = new UnityWebRequest("https://raw.githubusercontent.com/eternalUnion/AngryLevels/release/LevelCatalog.json");
			catalogRequest.downloadHandler = new DownloadHandlerBuffer();

            UnityWebRequestAsyncOperation handle = catalogRequest.SendWebRequest();
            currentCatalogRequest = handle;
            handle.completed += (e) =>
            {
                if (catalogRequest.isNetworkError || catalogRequest.isHttpError)
                {
					queuedCatalogCallbacks = null;
					return;
                }
                
                lastCatalog = JsonConvert.DeserializeObject<LevelCatalog>(catalogRequest.downloadHandler.text);
                
                if (queuedCatalogCallbacks != null)
                    queuedCatalogCallbacks.Invoke(lastCatalog);
                queuedCatalogCallbacks = null;
			};
        }
	
		private class OnlineIcon
		{
			public string hash;
			public Texture2D icon;

			public OnlineIcon(string hash, Texture2D icon)
			{
				this.hash = hash;
				this.icon = icon;
			}
		}

		private static Dictionary<string, OnlineIcon> iconCache = new Dictionary<string, OnlineIcon>();
		private static Dictionary<string, List<Action<Texture2D>>> queuedIconCallbacks = new Dictionary<string, List<Action<Texture2D>>>();
		private static Dictionary<string, UnityWebRequestAsyncOperation> currentIconRequest = new Dictionary<string, UnityWebRequestAsyncOperation>();

		public static void LoadIcon(string guid, Action<Texture2D> onLoad)
		{
			LoadCatalog((catalog) =>
			{
				var level = catalog.Levels.Where(l => l.Guid == guid).FirstOrDefault();
				if (level == null)
				{
					return;
				}

				if (iconCache.TryGetValue(guid, out OnlineIcon cachedIcon) && cachedIcon.hash == level.ThumbnailHash)
				{
					onLoad(cachedIcon.icon);
					return;
				}

				string iconsPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), "OnlineIconCache");
				if (!Directory.Exists(iconsPath))
					Directory.CreateDirectory(iconsPath);

				string filePath = Path.Combine(iconsPath, $"{guid}.png");
				if (File.Exists(filePath))
				{
					byte[] fileBytes = File.ReadAllBytes(filePath);

					MD5 md5 = MD5.Create();
					string hash = BitConverter.ToString(md5.ComputeHash(fileBytes)).Replace("-", "").ToLower();
					if (hash == level.ThumbnailHash)
					{
						Texture2D iconTex = new Texture2D(2, 2);
						ImageConversion.LoadImage(iconTex, fileBytes);

						OnlineIcon icon = new OnlineIcon(hash, iconTex);
						iconCache[guid] = icon;
						onLoad(iconTex);
						return;
					}
				}

				if (!queuedIconCallbacks.TryGetValue(guid, out var callbacks))
				{
					callbacks = new List<Action<Texture2D>>();
					queuedIconCallbacks.Add(guid, callbacks);
				}

				callbacks.Add(onLoad);

				if (currentIconRequest.TryGetValue(guid, out var currentHandle) && !currentHandle.isDone)
					return;

				UnityWebRequest iconRequest = new UnityWebRequest($"https://raw.githubusercontent.com/eternalUnion/AngryLevels/release/Levels/{guid}/thumbnail.png");
				if (File.Exists(filePath))
					File.Delete(filePath);
				iconRequest.downloadHandler = new DownloadHandlerFile(filePath);
				var handle = iconRequest.SendWebRequest();

				handle.completed += (req) =>
				{
					var callbackList = queuedIconCallbacks[guid];

					try
					{
						if (!iconRequest.isHttpError && !iconRequest.isNetworkError)
						{
							byte[] fileBytes = File.ReadAllBytes(filePath);
							MD5 md5 = MD5.Create();
							string hash = BitConverter.ToString(md5.ComputeHash(fileBytes)).Replace("-", "").ToLower();

							Texture2D iconTex = new Texture2D(2, 2);
							ImageConversion.LoadImage(iconTex, fileBytes);

							OnlineIcon icon = new OnlineIcon(hash, iconTex);
							iconCache[guid] = icon;

							if (callbackList != null)
								foreach (var callback in callbackList)
									callback(icon.icon);
						}
					}
					finally
					{
						if (callbackList != null)
							callbackList.Clear();
					}
				};

				currentIconRequest[guid] = handle;
			});
		}
		
		public static void PurgeIconCache()
		{
			foreach (var cachedIcon in iconCache.Values)
				DestroyImmediate(cachedIcon.icon);
		}
	}

    private class AngryServerManager
    {
		#region JSON Data for Server/Votes
        #pragma warning disable CS0649
		public class GetAllVotesBundleInfo
		{
			public int upvotes { get; set; }
			public int downvotes { get; set; }
		}

		public class GetAllVotesResponse
		{
			public string message { get; set; }
			public int status { get; set; }
			public Dictionary<string, GetAllVotesBundleInfo> bundles;
		}

		public class GetPlayCountResponse
		{
			public string message { get; set; }
			public int status { get; set; }
			public int postCount { get; set; }
		}
        #pragma warning restore CS0649
		#endregion

		public static GetAllVotesResponse lastResponse;

		private static int lastVoteRequestTime = 0;
		private static UnityWebRequestAsyncOperation currentRequest;
		private static event Action<GetAllVotesResponse> queuedCallbacks;

		public static void LoadVotes(Action<GetAllVotesResponse> callback)
		{
			if (lastResponse != null && (DateTime.Now.Second - lastVoteRequestTime) < 60)
			{
				callback(lastResponse);
				return;
			}

			queuedCallbacks += callback;

			if (currentRequest != null && !currentRequest.isDone)
				return;

			lastResponse = null;
			lastVoteRequestTime = DateTime.Now.Second;

			UnityWebRequest catalogRequest = new UnityWebRequest("https://angry.dnzsoft.com/votes");
			catalogRequest.downloadHandler = new DownloadHandlerBuffer();

			UnityWebRequestAsyncOperation handle = catalogRequest.SendWebRequest();
			currentRequest = handle;
			handle.completed += (e) =>
			{
				if (catalogRequest.isNetworkError || catalogRequest.isHttpError)
				{
					queuedCallbacks = null;
					return;
				}

				lastResponse = JsonConvert.DeserializeObject<GetAllVotesResponse>(catalogRequest.downloadHandler.text);

				if (queuedCallbacks != null)
					queuedCallbacks.Invoke(lastResponse);
				queuedCallbacks = null;
			};
		}
	
		public static void LoadPostCount(string guid, Action<int> callback)
		{
			if (callback == null)
				return;

			UnityWebRequest request = new UnityWebRequest($"https://angry.dnzsoft.com/leaderboards/bundlePostCount?bundleGuid={guid}");
			request.downloadHandler = new DownloadHandlerBuffer();

			UnityWebRequestAsyncOperation handle = request.SendWebRequest();
			handle.completed += (e) =>
			{
				if (request.isNetworkError || request.isHttpError)
				{
					callback(-1);
					return;
				}

				GetPlayCountResponse response = JsonConvert.DeserializeObject<GetPlayCountResponse>(request.downloadHandler.text);
				callback(response.postCount);
			};
		}
	}
	
    private static class InternalExporter
    {
		#region JSON Data for bundles
		public class BundleData
		{
			public string bundleName { get; set; }
			public string bundleAuthor { get; set; }
			public string bundleGuid { get; set; }
			public string buildHash { get; set; }
			public string bundleDataPath { get; set; }
			public int bundleVersion { get; set; }
			public List<string> levelDataPaths;
		}
		#endregion

		private static string[] builtInGroupNames = new string[]
	    {
		    "Built In Data",
		    "Default Group",
		    "Assets",
		    "Other",
		    "Music"
	    };

        private static AddressableAssetGroup GetGroupByGUID(string guid)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            return settings.groups.Where(group => group.Guid == guid).FirstOrDefault();
        }

        private static FieldInfo AddressableAssetGroup_m_GUID = typeof(AddressableAssetGroup).GetField("m_GUID", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		private static AddressableAssetGroup CreateGroupByGUID(string groupName, string guid)
        {
            var group = GetGroupByGUID(guid);
            if (group != null)
                return group;

			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string newGroupName = groupName;
            int i = 1;
            while (settings.groups.Where(g => g.Name == newGroupName).Any())
                newGroupName = $"{groupName}_{i++}";
            groupName = newGroupName;

			group = settings.CreateGroup(groupName, false, false, false, new List<AddressableAssetGroupSchema>());
            AddressableAssetGroup_m_GUID.SetValue(group, guid);

            return group;
		}

        public static AddressableAssetGroup MakeAddressable(BundleFolderAsset bundle)
        {
            bundle.Refresh();
            string bundleGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bundle.bundleData));

			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
			var bundleGroup = GetGroupByGUID(bundleGuid);
            if (bundleGroup == null)
                bundleGroup = CreateGroupByGUID("Bundle", bundleGuid);

            foreach (var entry in bundleGroup.entries.ToArray())
                settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(entry.AssetPath));

            string aaPath = $"{AssetDatabase.GUIDToAssetPath(bundle.folderGuid)}/Addressables";
			string dataPath = $"{AssetDatabase.GUIDToAssetPath(bundle.folderGuid)}/Data";
			foreach (string asset in GetFilesRecursive(aaPath).Where(f => !f.EndsWith(".meta")).Concat(GetFilesRecursive(dataPath).Where(f => !f.EndsWith(".meta"))))
            {
                string assetGuid = AssetDatabase.AssetPathToGUID(asset);
				var entry = settings.CreateOrMoveEntry(assetGuid, bundleGroup);
                entry.address = assetGuid;
			}

            return bundleGroup;
        }

		public static string GetBackupFolderSizeMB(RudeExporterSettings settings = null)
		{
			if (settings == null)
				settings = DefaultExporterSetting;

			string backupFolderPath = settings.backupPath;
			if (string.IsNullOrWhiteSpace(settings.backupPath))
			{
				backupFolderPath = settings.backupPath = RudeExporterSettings.DefaultBackupFolderPath;
				EditorUtility.SetDirty(settings);
			}

			if (!Directory.Exists(backupFolderPath))
			{
				Directory.CreateDirectory(backupFolderPath);
				return "0.00";
			}

			try
			{
				long totalSizeBytes = GetFilesRecursive(backupFolderPath).Select(file => new FileInfo(file).Length).Sum();
				return (totalSizeBytes / (float)(1024 * 1024)).ToString("0.00");
			}
			catch (Exception)
			{
				return "err";
			}
		}

		public static void ClearBackupFolder(RudeExporterSettings settings = null)
		{
			if (settings == null)
				settings = DefaultExporterSetting;

			if (settings.maxBackupFolderSizeMB <= 0)
				return;

			string backupFolderPath = settings.backupPath;
			if (!Directory.Exists(backupFolderPath))
				return;

			List<(string, FileInfo)> allBackups = new List<(string, FileInfo)>();
			foreach (string validSubFolder in Directory.GetDirectories(backupFolderPath).Where(subFolder => GUID.TryParse(Path.GetFileName(subFolder), out GUID _)))
			{
				foreach (string validSubFile in Directory.GetFiles(validSubFolder).Where(file => file.EndsWith(".zip")))
					allBackups.Add((validSubFile, new FileInfo(validSubFile)));
			}
			allBackups.Sort((l, r) => (int)(l.Item2.CreationTime.ToFileTime() - r.Item2.CreationTime.ToFileTime()));

			do
			{
				if (allBackups.Count == 0)
					break;

				long allBackupTotalSize = 0;
				foreach (var file in allBackups)
					allBackupTotalSize += file.Item2.Length;
				allBackupTotalSize /= (1024 * 1024);

				if (allBackupTotalSize <= settings.maxBackupFolderSizeMB)
					break;

				File.Delete(allBackups[0].Item1);
				allBackups.RemoveAt(0);
			}
			while (true);
		}

		public static void ClearBundleBackupFolder(BundleFolderAsset bundle, RudeExporterSettings settings = null)
		{
			if (settings == null)
				settings = DefaultExporterSetting;

			if (settings.maxSizePerBundleMB <= 0)
				return;

			string backupFolderPath = settings.backupPath;
			if (string.IsNullOrWhiteSpace(settings.backupPath))
			{
				backupFolderPath = settings.backupPath = RudeExporterSettings.DefaultBackupFolderPath;
				EditorUtility.SetDirty(settings);
			}

			string bundleBackupPath = Path.Combine(backupFolderPath, bundle.bundleGuid);
			if (!Directory.Exists(bundleBackupPath))
				return;
			List<(string, FileInfo)> bundleBackups = Directory.GetFiles(bundleBackupPath).Where(file => file.EndsWith(".zip")).Select(file => (file, new FileInfo(file))).OrderBy(e => e.Item2.CreationTime.ToFileTime()).ToList();
			
			do
			{
				if (bundleBackups.Count == 0)
					break;

				long bundleBackupFolderSize = 0;
				foreach (var file in bundleBackups)
					bundleBackupFolderSize += file.Item2.Length;
				bundleBackupFolderSize /= (1024 * 1024);

				if (bundleBackupFolderSize <= settings.maxSizePerBundleMB)
					break;

				File.Delete(bundleBackups[0].Item1);
				bundleBackups.RemoveAt(0);
			}
			while (true);
		}

		public class BackupResult
		{
			public bool success;
			public bool canceled;
			public string filePath;
		}

		public static BackupResult BackupBundle(BundleFolderAsset bundle, RudeExporterSettings settings = null)
		{
			if (settings == null)
				settings = DefaultExporterSetting;

			string backupFolderPath = settings.backupPath;
			if (string.IsNullOrWhiteSpace(backupFolderPath))
			{
				backupFolderPath = settings.backupPath = RudeExporterSettings.DefaultBackupFolderPath;
				EditorUtility.SetDirty(settings);
			}

			if (!Directory.Exists(backupFolderPath))
				Directory.CreateDirectory(backupFolderPath);

			string bundleBackupPath = Path.Combine(backupFolderPath, bundle.bundleGuid);
			if (!Directory.Exists(bundleBackupPath))
				Directory.CreateDirectory(bundleBackupPath);

			string backupFileName = GetSafeFileName($"{bundle.bundleData.bundleName}_{DateTime.Now.ToFileTime()}.zip");
			string backupFilePath = Path.Combine(bundleBackupPath, backupFileName);

			bool aborted = false;

			try
			{
				using (FileStream backupFS = File.Open(backupFilePath, FileMode.Create, FileAccess.ReadWrite))
				using (ZipArchive backup = new ZipArchive(backupFS, ZipArchiveMode.Create))
				{
					string bundleFolder = AssetDatabase.GUIDToAssetPath(bundle.folderGuid);
					foreach (string file in GetFilesRecursive(bundleFolder))
					{
						if (EditorUtility.DisplayCancelableProgressBar("Backing up", file, 1f))
						{
							EditorUtility.ClearProgressBar();
							EditorUtility.DisplayDialog("Aborted", "Export canceled during backup process", "Close");
							aborted = true;
							break;
						}

						using (FileStream assetStream = File.Open(file, FileMode.Open, FileAccess.Read))
						using (Stream entry = backup.CreateEntry(file).Open())
							assetStream.CopyTo(entry);
					}
				}

				if (aborted)
				{
					try { if (File.Exists(backupFilePath)) File.Delete(backupFilePath); }
					catch (Exception) { }

					return new BackupResult()
					{
						success = false,
						canceled = true,
						filePath = backupFilePath
					};
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Exception thrown while taking backup");
				Debug.LogException(e);

				try { if (File.Exists(backupFilePath)) File.Delete(backupFilePath); }
				catch (Exception) { }

				EditorUtility.DisplayDialog("Error", "Encountered an error while taking backup. Check console for more details.", "Close");

				return new BackupResult()
				{
					success = false,
					canceled = false,
					filePath = backupFilePath
				};
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			return new BackupResult()
			{
				success = true,
				canceled = false,
				filePath = backupFilePath
			};
		}

		public static void BuildBundle(BundleFolderAsset bundle, RudeExporterSettings exporterSettings)
		{
			BuildBundle(bundle, exporterSettings, bundle.levels);
		}

		public static void BuildBundle(BundleFolderAsset bundle, RudeExporterSettings exporterSettings, IEnumerable<RudeLevelData> levelsToExport)
        {
			bool prompt = exporterSettings.promptBeforeExport;
			bool summary = exporterSettings.summaryAfterExport;
			string destination = exporterSettings.buildPath;
			bool unchangedWarning = exporterSettings.checkForChanges;

			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

			bundle.Refresh();
			if (prompt)
			{
				if (!EditorUtility.DisplayDialog("Rude Exporter", $"Do you want to export this bundle?\n\nBundle name: {bundle.bundleData.bundleName}\nAuthor: {bundle.bundleData.author}\nNumber of levels: {levelsToExport.Count()}", "Export", "Cancel"))
					return;
			}

			string currentBuildHash = unchangedWarning ? bundle.GetHash(progressBar: true) : GUID.Generate().ToString();

			if (unchangedWarning)
			{
				foreach (string angryFile in GetFilesRecursive(destination).Where(f => f.EndsWith(".angry")))
				{
					try
					{
						using (ZipArchive angryArchive = new ZipArchive(File.Open(angryFile, FileMode.Open, FileAccess.Read)))
						{
							var dataEntry = angryArchive.GetEntry("data.json");
							if (dataEntry == null)
								continue;

							using (StreamReader dataStr = new StreamReader(dataEntry.Open()))
							{
								BundleData data = JsonConvert.DeserializeObject<BundleData>(dataStr.ReadToEnd());
								if (data.bundleGuid == bundle.bundleGuid)
								{
									string previousHash = data.buildHash;
									
									if (previousHash == currentBuildHash)
									{
										if (!EditorUtility.DisplayDialog("Warning", "No changes were made since last export, do you still want to continue building?", "Continue", "Cancel"))
											return;
										break;
									}
								}
							}
						}
					}
					catch (Exception) { }
				}
			}

			if (exporterSettings.enableBackups)
			{
				try
				{
					EditorUtility.DisplayProgressBar("Backing up", "Clearing old bundle backups", 0f);
					ClearBundleBackupFolder(bundle);
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}

				try
				{
					EditorUtility.DisplayProgressBar("Backing up", "Clearing old backups", 0.5f);
					ClearBackupFolder();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}

				BackupBundle(bundle);
			}

			AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            BundledAssetGroupSchema templateSchema = (BundledAssetGroupSchema)AssetDatabase.LoadAssetAtPath<AddressableAssetGroupTemplate>("Assets/AddressableAssetsData/AssetGroupTemplates/Packed Assets.asset").GetSchemaByType(typeof(BundledAssetGroupSchema));
            var bundleGroup = MakeAddressable(bundle);
			var defGroup = settings.DefaultGroup;

            // Setup
			void ProcessBuiltInSchema(BundledAssetGroupSchema schema)
            {
				schema.IncludeInBuild = true;
				schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
				schema.BuildPath.SetVariableByName(settings, "Local.BuildPath");
				schema.LoadPath.SetVariableByName(settings, "Local.LoadPath");
				schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
				schema.InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuid;
				schema.UseAssetBundleCrcForCachedBundles = false;
				schema.UseAssetBundleCrc = false;
			}

			void ProcessCustomSchema(BundledAssetGroupSchema schema)
			{
                if (schema.Group == defGroup)
                    return;

				schema.IncludeInBuild = false;
				schema.IncludeAddressInCatalog = true;
				schema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
				schema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");
				schema.BundleNaming = BundledAssetGroupSchema.BundleNamingStyle.NoHash;
				schema.InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuid;
				schema.InternalIdNamingMode = BundledAssetGroupSchema.AssetNamingMode.GUID;
				schema.UseAssetBundleCrcForCachedBundles = false;
				schema.UseAssetBundleCrc = false;
			}

			foreach (var entry in defGroup.entries.ToArray())
				settings.RemoveAssetEntry(AssetDatabase.AssetPathToGUID(entry.AssetPath));

            ProcessBuiltInSchema(GetOrCreateSchemaFromTemplate(defGroup, templateSchema));
			GetOrCreateSchemaFromTemplate(defGroup, templateSchema).InternalBundleIdMode = BundledAssetGroupSchema.BundleInternalIdMode.GroupGuidProjectIdHash;

			foreach (var builtInGroup in settings.groups.Where(g => builtInGroupNames.Contains(g.Name)))
			{
				if (builtInGroup.GetSchema<PlayerDataGroupSchema>() != null)
					continue;

                ProcessBuiltInSchema(GetOrCreateSchemaFromTemplate(builtInGroup, templateSchema));
			}

            foreach (var customGroup in settings.groups.Where(g => !builtInGroupNames.Contains(g.Name)))
            {
                var schema = GetOrCreateSchemaFromTemplate(customGroup, templateSchema);
				ProcessCustomSchema(schema);

                if (schema.Group == bundleGroup)
                    schema.IncludeInBuild = true;
            }

			settings.MonoScriptBundleCustomNaming = bundleGroup.Guid.Substring(0, 16);
			settings.profileSettings.SetValue(settings.activeProfileId, "Remote.BuildPath", "BuiltBundles");
			settings.profileSettings.SetValue(settings.activeProfileId, "Remote.LoadPath", @"{AngryLevelLoader.Plugin.tempFolderPath}\\" + bundleGroup.Guid);

			int indexOfBuilder = -1;
			for (int i = 0; i < settings.DataBuilders.Count; i++)
				if (settings.DataBuilders[i] is BuildScriptPackedMode)
				{
					indexOfBuilder = i;
					break;
				}

			if (indexOfBuilder == -1)
			{
				BuildScriptPackedMode asset = new BuildScriptPackedMode();

				string path = "Assets/AddressableAssetsData/DataBuilders";
				string fileName = "BuildScriptPackedMode.asset";
				string newName = fileName;
				int i = 0;
				while (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(Path.Combine(path, newName))))
					newName = fileName.Replace(".asset", $"_{i++}.asset");
				fileName = newName;

				string assetPath = Path.Combine(path, fileName);
				AssetDatabase.CreateAsset(asset, assetPath);

				settings.DataBuilders.Add(asset);
				indexOfBuilder = settings.DataBuilders.Count - 1;
			}

			settings.ActivePlayerDataBuilderIndex = indexOfBuilder;

            // Process assets
            bool modifiedAssets = false;
			foreach (RudeLevelData level in bundle.levels)
            {
                if (level.targetScene is SceneAsset sceneAsset)
                {
					string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
					string sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
                    if (level.scenePath != sceneGuid)
                    {
                        level.scenePath = sceneGuid;
                        modifiedAssets = true;
						EditorUtility.SetDirty(level);
                    }

					if (!levelsToExport.Contains(level))
					{
						string levelDataGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(level));
						var levelEntry = settings.FindAssetEntry(levelDataGuid);
						if (levelEntry != null)
						{
							settings.RemoveAssetEntry(levelDataGuid);
							modifiedAssets = true;
						}

						var sceneEntry = settings.FindAssetEntry(sceneGuid);
						if (sceneEntry != null)
						{
							settings.RemoveAssetEntry(sceneGuid);
							modifiedAssets = true;
						}

						continue;
					}

					// Get dll dependencies
					bool sceneAlreadyOpen = TryGetOpenScene(scenePath, out Scene editorScene);
                    if (!sceneAlreadyOpen)
                        editorScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

					IEnumerable<Transform> GetAllObjects(Transform t)
					{
						foreach (Transform child in t)
						{
							yield return child;
							foreach (Transform childChild in GetAllObjects(child))
								yield return childChild;
						}
					}

					List<string> GetAllRequiredDlls(Scene scene)
					{
						void GetAllAssemblyInfo(GameObject o, List<string> alreadyAdded, List<string> requiredDlls)
						{
							foreach (MonoBehaviour behaviour in o.GetComponents<MonoBehaviour>())
							{
								if (behaviour == null)
									continue;

								string path = behaviour.GetType().Assembly.Location;
								if (alreadyAdded.Contains(path))
									continue;

								alreadyAdded.Add(path);
								string name = Path.GetFileName(path);
								if (name == "RudeLevelScripts.Essentials.dll")
									continue;
								else if (name == "RudeLevelScripts.dll")
									continue;

								string rudeScriptsPath = Path.Combine(Application.dataPath, "RudeScripts");
								string dllDir = Path.GetDirectoryName(path);

								bool found = false;
								while (Directory.Exists(dllDir))
								{
									if (Path.GetFullPath(dllDir) == Path.GetFullPath(rudeScriptsPath))
									{
										found = true;
										break;
									}

									dllDir = Path.GetDirectoryName(dllDir);
								}

								if (found)
									requiredDlls.Add(name);
							}
						}

						List<string> added = new List<string>();
						List<string> dlls = new List<string>();

						foreach (GameObject o in scene.GetRootGameObjects())
						{
							GetAllAssemblyInfo(o, added, dlls);
							foreach (Transform t in GetAllObjects(o.transform))
								GetAllAssemblyInfo(t.gameObject, added, dlls);
						}

						return dlls;
					}

					int GetSecretCount(Scene scene)
					{
						int cnt = 0;
						foreach (GameObject o in scene.GetRootGameObjects())
						{
							cnt += o.GetComponentsInChildren<Bonus>(true).Length;
						}

						return cnt;
					}

					List<string> levelDlls = GetAllRequiredDlls(editorScene);
                    levelDlls.Sort();
                    if (level.requiredDllNames == null || !level.requiredDllNames.SequenceEqual(levelDlls))
                    {
                        level.requiredDllNames = levelDlls.ToArray();
						modifiedAssets = true;
						EditorUtility.SetDirty(level);
                    }

					int secretCount = GetSecretCount(editorScene);
					if (level.secretCount != secretCount)
					{
						level.secretCount = secretCount;
						modifiedAssets = true;
						EditorUtility.SetDirty(level);
					}

					if (!sceneAlreadyOpen)
                        EditorSceneManager.CloseScene(editorScene, true);
                }
            }
        
            if (modifiedAssets)
            {
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			// Build
			AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);
			if (result == null || !string.IsNullOrEmpty(result.Error))
			{
				EditorUtility.DisplayDialog("Error", "Encountered an error while building content. Please try to reopen the project. If the issue persists send error log", "Ok");
				return;
			}

            // Create data for angry
			string tempBuildDir = Path.Combine(Application.dataPath, "../", "TempBundle");
			if (Directory.Exists(tempBuildDir))
				Directory.Delete(tempBuildDir, true);
			Directory.CreateDirectory(tempBuildDir);

			string monoPath = "";
			foreach (var builtBundle in result.AssetBundleBuildResults)
			{
				string realPath = builtBundle.FilePath.Substring(0, builtBundle.FilePath.Length - 40) + ".bundle";

				if (builtBundle.SourceAssetGroup != bundleGroup)
				{
					if (Path.GetFileName(builtBundle.FilePath).StartsWith(settings.MonoScriptBundleCustomNaming))
					{
						monoPath = realPath;
						File.Copy(monoPath, Path.Combine(tempBuildDir, Path.GetFileName(monoPath)));
					}

					continue;
				}

				File.Copy(realPath, Path.Combine(tempBuildDir, Path.GetFileName(realPath)));
			}

			string sourcePath = @"{UnityEngine.AddressableAssets.Addressables.RuntimePath}\\StandaloneWindows64\\" + Path.GetFileName(monoPath);
			string destinationPath = @"{AngryLevelLoader.Plugin.tempFolderPath}\\" + bundleGroup.Guid + @"\\" + Path.GetFileName(monoPath);
			string catalog = File.ReadAllText(Path.Combine(result.OutputPath, "../", "catalog.json"));
			File.WriteAllText(Path.Combine(tempBuildDir, "catalog.json"), catalog.Replace(sourcePath, destinationPath));

			BundleData bundleData = new BundleData();
			bundleData.bundleName = bundle.bundleData.bundleName;
			bundleData.bundleAuthor = bundle.bundleData.author;
			bundleData.bundleVersion = 5;
			bundleData.buildHash = currentBuildHash;
			bundleData.bundleGuid = bundleGroup.Guid;
			bundleData.bundleDataPath = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bundle.bundleData));
			bundleData.levelDataPaths = bundle.levels.Where(level => levelsToExport.Contains(level)).Select(l => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(l))).ToList();

			Sprite levelIcon = bundle.bundleData.levelIcon;
			if (levelIcon != null && levelIcon.texture != null)
			{
				Texture2D duplicateTexture(Texture2D source)
				{
					RenderTexture renderTex = RenderTexture.GetTemporary(
								source.width,
								source.height,
								0,
								RenderTextureFormat.Default,
								RenderTextureReadWrite.Linear);

					Graphics.Blit(source, renderTex);
					RenderTexture previous = RenderTexture.active;
					RenderTexture.active = renderTex;
					Texture2D readableText = new Texture2D(source.width, source.height);
					readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
					readableText.Apply();
					RenderTexture.active = previous;
					RenderTexture.ReleaseTemporary(renderTex);
					return readableText;
				}

				Texture2D sourceTexture = levelIcon.texture;
				Texture2D decompressedTexture = duplicateTexture(sourceTexture);

				byte[] pngBytes = ImageConversion.EncodeToPNG(decompressedTexture);
				DestroyImmediate(decompressedTexture);
				string iconDestinationPath = Path.Combine(tempBuildDir, "icon.png");
				File.WriteAllBytes(iconDestinationPath, pngBytes);
			}

			File.WriteAllText(Path.Combine(tempBuildDir, "data.json"), JsonConvert.SerializeObject(bundleData));

            // Find destination file
            string outputPath = "";
            foreach (string angryFile in GetFilesRecursive(destination).Where(f => f.EndsWith(".angry")))
            {
                try
                {
                    using (ZipArchive angryArchive = new ZipArchive(File.Open(angryFile, FileMode.Open, FileAccess.Read)))
                    {
                        var dataEntry = angryArchive.GetEntry("data.json");
                        if (dataEntry == null)
                            continue;

						using (StreamReader dataStr = new StreamReader(dataEntry.Open()))
                        {
                            BundleData data = JsonConvert.DeserializeObject<BundleData>(dataStr.ReadToEnd());
                            if (data.bundleGuid == bundleGroup.Guid)
                            {
                                outputPath = angryFile;
                                break;
                            }
                        }
                    }
                }
                catch (Exception) { }
            }

			string GetPathSafeName(string name)
			{
				StringBuilder newName = new StringBuilder();
				for (int i = 0; i < name.Length; i++)
				{
					char c = name[i];

					if (char.IsLetterOrDigit(c) || c == '-' || c == '_')
					{
						newName.Append(c);
					}
					else if (c == ' ')
					{
						if (i > 0 && name[i - 1] != ' ')
							newName.Append('_');
					}
				}

				string finalName = newName.ToString();
				if (string.IsNullOrEmpty(finalName))
					return "file";
				return finalName;
			}

			if (string.IsNullOrEmpty(outputPath))
			{
				string fileName = GetPathSafeName(bundle.bundleData.bundleName);
				string newFileName = fileName;
				int i = 0;
				while (File.Exists(Path.Combine(destination, $"{newFileName}.angry")))
					newFileName = $"{fileName}_{i}";

				outputPath = Path.Combine(destination, $"{newFileName}.angry");
			}

			string zipDir = Path.Combine(tempBuildDir, $"{bundleGroup.Name}.angry");
			using (ZipArchive archive = new ZipArchive(File.Open(zipDir, FileMode.Create, FileAccess.ReadWrite), ZipArchiveMode.Create))
			{
				foreach (string file in Directory.GetFiles(tempBuildDir).Where(dir => dir != zipDir))
				{
					ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(file));
					using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read))
					{
						using (Stream entryStream = entry.Open())
						{
							fs.CopyTo(entryStream);
						}
					}
				}
			}

			File.Copy(zipDir, outputPath, true);

			if (summary)
			{
				long fileLengthBytes = -1;
				try
				{
					using (FileStream fs = File.Open(outputPath, FileMode.Open, FileAccess.Read))
						fileLengthBytes = fs.Length;
				}
				catch (Exception) { }

				string fileLengthStr = $"{fileLengthBytes} B";
				if (fileLengthBytes >= 1024)
				{
					double fileLengthKilobytes = fileLengthBytes / 1024d;
					fileLengthStr = $"{fileLengthKilobytes:#.#} KB";

					if (fileLengthKilobytes >= 1024)
					{
						double fileLengthMegabytes = fileLengthKilobytes / 1024d;
						fileLengthStr = $"{fileLengthMegabytes:#.#} MB";

						if (fileLengthMegabytes >= 1024)
						{
							double fileLengthGigabytes = fileLengthMegabytes / 1024d;
							fileLengthStr = $"{fileLengthGigabytes:#.#} GB";
						}
					}
				}

				if (fileLengthBytes == -1)
					fileLengthStr = "<error>";

				EditorUtility.DisplayDialog("Rude Exporter", $"Exported successfully!\n\nBundle name: {bundle.bundleData.bundleName}\nAuthor: {bundle.bundleData.author}\nLevel count: {levelsToExport.Count()}\n\nTime taken: {result.Duration:#.#} seconds\nFile size: {fileLengthStr}\nOutput path: '{outputPath}'", "Close");
			}
        }

		public static void TryExport(BundleFolderAsset bundle, RudeExporterSettings exporterSettings)
		{
			bundle.Refresh();
			TryExport(bundle, exporterSettings, bundle.levels);
		}

		public static void TryExport(BundleFolderAsset bundle, RudeExporterSettings exporterSettings, IEnumerable<RudeLevelData> levelsToBuild)
		{
			bundle.Refresh();

			if (!Directory.Exists(exporterSettings.buildPath))
			{
				EditorUtility.DisplayDialog("Error", "Cannot build the bundle because angry levels folder is not set! Open exporter settings to set the destination.", "Close");
				RudeExporter wnd = GetWindow<RudeExporter>();
				wnd.titleContent = new GUIContent("Rude Exporter");
				wnd.DisplaySettingsWindow(true);
				return;
			}

			foreach (RudeLevelData level in bundle.levels)
			{
				if (string.IsNullOrEmpty(level.uniqueIdentifier))
				{
					EditorUtility.DisplayDialog("Error", $"Level '{level.levelName}' has invalid id! Set a valid id before exporting.", "Close");
					RudeExporter wnd = GetWindow<RudeExporter>();
					wnd.titleContent = new GUIContent("Rude Exporter");
					wnd.DisplayLevelModifyWindow(bundle, level);
					return;
				}
			}

			BuildBundle(bundle, exporterSettings, levelsToBuild);
		}
	}

	private static class InternalPorter
	{
		private const string PORT_EXT = "universalport";

		// Porter paths
		private static string PORTER_TEMP_PATH => Path.Combine(Application.dataPath, "../", "UniversalPorter", "Temp");
		private static string PORTER_CACHE_PATH => Path.Combine(Application.dataPath, "../", "UniversalPorter", "Cache");
		private static string PORTER_SCRIPT_CACHE_PATH => Path.Combine(PORTER_CACHE_PATH, "ScriptInfoCache.json");
		private static string PORTER_PREFAB_CACHE_PATH => Path.Combine(PORTER_CACHE_PATH, "PrefabInfoCache", "PrefabInfoCache.json");
		private static string PORTER_PREFAB_CACHE_CONTAINER_PATH => Path.Combine(PORTER_CACHE_PATH, "PrefabInfoCache", "PrefabInfos");

		// Zip entry paths
		private const string PORTER_DATA_PATH = "porterData.json";
		private const string SCRIPT_INFO_PATH = "scriptInfo.json";
		private const string PREFAB_INFO_PATH = "prefabInfo.json";
		private const string DEPENDENCY_INFO_PATH = "dependencyInfo.json";
		private const string PACKAGE_PATH = "package.unitypackage";

		private static string[] ValidGuidFileExtensions = new string[]
		{
			".meta",
			".mat",
			".anim",
			".prefab",
			".unity",
			".guiskin",
			".fontsettings",
			".controller",
		};

		private static string GetMD5(string path)
		{
			MD5 md5 = MD5.Create();
			using (FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read))
			{
				return BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "");
			}
		}

		private static string GetMD5FromString(string text)
		{
			MD5 md5 = MD5.Create();
			return BitConverter.ToString(md5.ComputeHash(Encoding.ASCII.GetBytes(text))).Replace("-", "");
		}

		#region Porter Information
		private const string PORTER_VERSION = "1.0.0";

		private class PorterDataJson
		{
			public string version { get; set; }

			public string bundleFolderPath { get; set; }
			public string bundleGuid { get; set; }
			public string folderGuid { get; set; }

			public string bundleName { get; set; }
			public string bundleAuthor { get; set; }
		}
		#endregion

		#region Script Information
		private class ScriptInfo
		{
			public string fullClassName { get; set; }
			public string guid { get; set; }
		}

		private class ScriptInfoJson
		{
			public ScriptInfo[] scripts;
		}

		private class ScriptCacheInfo
		{
			public string MD5 { get; set; }
			public string FullName { get; set; }
		}

		private class ScriptCacheJson
		{
			public Dictionary<string, ScriptCacheInfo> scriptPathToCache;
		}

		private static IEnumerable<ScriptInfo> GetAllScripts()
		{
			HashSet<string> foundNames = new HashSet<string>();
			ScriptCacheJson scriptCache;

			bool dirtyScriptCache = false;
			if (File.Exists(PORTER_SCRIPT_CACHE_PATH))
			{
				try
				{
					scriptCache = JsonConvert.DeserializeObject<ScriptCacheJson>(File.ReadAllText(PORTER_SCRIPT_CACHE_PATH));
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Failed to deserialize script cache. Creating a new one.\n{e.StackTrace}");

					scriptCache = new ScriptCacheJson();
					scriptCache.scriptPathToCache = new Dictionary<string, ScriptCacheInfo>();
					dirtyScriptCache = true;
				}
			}
			else
			{
				scriptCache = new ScriptCacheJson();
				scriptCache.scriptPathToCache = new Dictionary<string, ScriptCacheInfo>();
				dirtyScriptCache = true;
			}

			foreach (string scriptPath in AssetDatabase.GetAllAssetPaths().Where(path => AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(MonoScript)))
			{
				if (!scriptPath.StartsWith("Assets/"))
					continue;

				if (!File.Exists(scriptPath))
					continue;

				string md5 = GetMD5(scriptPath);
				if (scriptCache.scriptPathToCache.TryGetValue(scriptPath, out ScriptCacheInfo cachedScriptInfo))
				{
					if (md5 == cachedScriptInfo.MD5)
					{
						if (foundNames.Contains(cachedScriptInfo.FullName))
							continue;

						foundNames.Add(cachedScriptInfo.FullName);
						yield return new ScriptInfo() { fullClassName = cachedScriptInfo.FullName, guid = AssetDatabase.AssetPathToGUID(scriptPath) };
						continue;
					}
				}

				Regex namespaceRegex = new Regex(@"^[^\/]*namespace\s+([^\s]+)");
				Regex declarationRegex = new Regex(@"^[^\/]*(class|struct|interface)\s+([^\s\n<]+(<[^>]+>)*)");

				string namespaceString = null;
				string declarationString = null;

				using (StreamReader scriptStream = new StreamReader(File.Open(scriptPath, FileMode.Open, FileAccess.Read)))
				{
					while (!scriptStream.EndOfStream && (namespaceString == null || declarationString == null))
					{
						string line = scriptStream.ReadLine();

						if (namespaceString == null)
						{
							var namespaceMatch = namespaceRegex.Match(line);
							if (namespaceMatch.Success)
								namespaceString = namespaceMatch.Groups[1].Value;
						}

						if (declarationString == null)
						{
							var declarationMatch = declarationRegex.Match(line);
							if (declarationMatch.Success)
							{
								declarationString = declarationMatch.Groups[2].Value;
								break;
							}
						}
					}
				}

				if (declarationString == null)
					continue;

				string fullName = declarationString;
				if (namespaceString != null)
					fullName = $"{namespaceString}.{fullName}";

				if (foundNames.Contains(fullName))
					continue;

				foundNames.Add(fullName);
				yield return new ScriptInfo() { fullClassName = fullName, guid = AssetDatabase.AssetPathToGUID(scriptPath) };

				scriptCache.scriptPathToCache[scriptPath] = new ScriptCacheInfo() { MD5 = md5, FullName = fullName };
				dirtyScriptCache = true;
			}

			if (dirtyScriptCache)
			{
				if (!Directory.Exists(Path.GetDirectoryName(PORTER_SCRIPT_CACHE_PATH)))
					Directory.CreateDirectory(Path.GetDirectoryName(PORTER_SCRIPT_CACHE_PATH));
				File.WriteAllText(PORTER_SCRIPT_CACHE_PATH, JsonConvert.SerializeObject(scriptCache));
			}
		}
		#endregion

		#region Prefab Information
		private class ComponentInfo
		{
			public string id { get; set; }
			public string type { get; set; }
		}

		private class PrefabNode
		{
			public string gameObjectName { get; set; }
			public string gameObjectId { get; set; }
			public string transformId { get; set; }
			public int rootOrder { get; set; }

			public List<ComponentInfo> components = new List<ComponentInfo>();
			public List<PrefabNode> children = new List<PrefabNode>();
		}

		private class PrefabContainerJson
		{
			public Dictionary<string, PrefabNode> prefabs;
		}

		private class PrefabInfoCache
		{
			public string PrefabMD5 { get; set; }
			public string InfoMD5 { get; set; }
		}

		private class PrefabInfoCacheJson
		{
			public Dictionary<string, PrefabInfoCache> prefabGuidToCache;
		}

		private static PrefabNode MakePrefabInfo(GameObject prefab)
		{
			PrefabNode node = new PrefabNode();
			long id;

			if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab, out _, out id))
				node.gameObjectId = id.ToString();
			if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(prefab.transform, out _, out id))
				node.transformId = id.ToString();
			node.gameObjectName = prefab.name;
			node.rootOrder = prefab.transform.GetSiblingIndex();

			foreach (var comp in prefab.GetComponents<Component>())
			{
				if (comp == null)
				{
					node.components.Add(new ComponentInfo() { id = "", type = "" });
					continue;
				}

				if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(comp, out _, out id))
					node.components.Add(new ComponentInfo() { id = id.ToString(), type = comp.GetType().FullName });
			}

			foreach (Transform child in prefab.transform)
			{
				node.children.Add(MakePrefabInfo(child.gameObject));
			}

			return node;
		}

		private static PrefabNode ProcessPrefab(string path, PrefabInfoCacheJson cache, out bool cacheDirty)
		{
			cacheDirty = false;

			string guid = AssetDatabase.AssetPathToGUID(path);
			string prefabMD5 = GetMD5(path);
			string cachedInfoPath = Path.Combine(PORTER_PREFAB_CACHE_CONTAINER_PATH, guid + ".json");

			if (cache.prefabGuidToCache.TryGetValue(guid, out PrefabInfoCache prefabCache) && prefabCache.PrefabMD5 == prefabMD5 && File.Exists(cachedInfoPath))
			{
				string cachedInfoMD5 = GetMD5(cachedInfoPath);
				if (prefabCache.InfoMD5 == cachedInfoMD5)
				{
					PrefabNode cachedPrefabNode;
					try
					{
						cachedPrefabNode = JsonConvert.DeserializeObject<PrefabNode>(File.ReadAllText(cachedInfoPath), new JsonSerializerSettings() { MaxDepth = 256 });
						return cachedPrefabNode;
					}
					catch (Exception e)
					{
						Debug.LogWarning($"Failed to deserialize prefab cache. Creating a new one.\n{e.StackTrace}");
					}
				}
			}

			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			PrefabNode node = MakePrefabInfo(prefab);

			cacheDirty = true;
			if (!Directory.Exists(PORTER_PREFAB_CACHE_CONTAINER_PATH))
				Directory.CreateDirectory(PORTER_PREFAB_CACHE_CONTAINER_PATH);
			string nodeString = JsonConvert.SerializeObject(node);
			File.WriteAllText(cachedInfoPath, nodeString);
			cache.prefabGuidToCache[guid] = new PrefabInfoCache() { PrefabMD5 = prefabMD5, InfoMD5 = GetMD5FromString(nodeString) };
			return node;
		}
		#endregion

		#region Dependency Calculations
		private class DependencyInfo
		{
			public string path { get; set; }
			public string guid { get; set; }
		}

		private class DependencyContainerJson
		{
			public List<DependencyInfo> dependencies;
		}
		#endregion

		public static void PortBundleOut(BundleFolderAsset bundle)
		{
			bundle.Refresh();

			string bundleFolderPath = AssetDatabase.GUIDToAssetPath(bundle.folderGuid);
			if (string.IsNullOrEmpty(bundleFolderPath))
			{
				EditorUtility.DisplayDialog("Porter Error", "Bundle folder not found", "Close");
				return;
			}

			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				return;

			string exportPath = EditorUtility.SaveFilePanel("Export bundle", Application.dataPath, RemoveTags(bundle.bundleData.bundleName), PORT_EXT);
			if (string.IsNullOrEmpty(exportPath))
				return;
			if (File.Exists(exportPath))
			{
				if (!EditorUtility.DisplayDialog("Warning", "File already exists, overwrite anyways?", "Overwrite", "Abort"))
					return;
			}

			try
			{
				// Create data
				PorterDataJson porterData = new PorterDataJson();
				porterData.version = PORTER_VERSION;
				porterData.bundleFolderPath = bundleFolderPath;
				porterData.bundleName = bundle.bundleData.bundleName;
				porterData.bundleAuthor = bundle.bundleData.author;
				porterData.bundleGuid = bundle.bundleGuid;
				porterData.folderGuid = bundle.folderGuid;

				const float stepCount = 4;
				int currentStep = 0;

				// Collect project script information
				EditorUtility.DisplayProgressBar("Step 1", "Collecting script information", currentStep / stepCount);
				ScriptInfo[] projectScripts = GetAllScripts().ToArray();
				currentStep += 1;

				// Collect prefab id information
				EditorUtility.DisplayProgressBar("Step 2", "Collecting prefab information", currentStep / stepCount);

				PrefabInfoCacheJson prefabCache;
				bool dirtyPrefabCache = false;
				if (File.Exists(PORTER_PREFAB_CACHE_PATH))
				{
					try
					{
						prefabCache = JsonConvert.DeserializeObject<PrefabInfoCacheJson>(File.ReadAllText(PORTER_PREFAB_CACHE_PATH));
					}
					catch(Exception e)
					{
						Debug.LogWarning($"Failed to deserialize cache. Creating a new one.\n{e.StackTrace}");
						prefabCache = new PrefabInfoCacheJson();
						prefabCache.prefabGuidToCache = new Dictionary<string, PrefabInfoCache>();
						dirtyPrefabCache = true;
					}
				}
				else
				{
					prefabCache = new PrefabInfoCacheJson();
					prefabCache.prefabGuidToCache = new Dictionary<string, PrefabInfoCache>();
					dirtyPrefabCache = true;
				}

				PrefabContainerJson prefabContainer = new PrefabContainerJson();
				prefabContainer.prefabs = new Dictionary<string, PrefabNode>();
				foreach (string prefabPath in AssetDatabase.GetAllAssetPaths().Where(path => (path.EndsWith(".prefab") || path.EndsWith(".fbx") || path.EndsWith(".obj")) && !path.StartsWith("Assets/ULTRAKILL Others")))
				{
					GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
					if (prefab == null)
						continue;

					prefabContainer.prefabs[AssetDatabase.AssetPathToGUID(prefabPath)] = ProcessPrefab(prefabPath, prefabCache, out bool makeCacheDirty);
					if (makeCacheDirty)
					{
						dirtyPrefabCache = true;
						EditorUtility.DisplayProgressBar("Step 2", $"Collecting prefab information: {Path.GetFileNameWithoutExtension(prefabPath)}", currentStep / stepCount);
					}
					else
					{
						EditorUtility.DisplayProgressBar("Step 2", $"Collecting prefab information: (Cached) {Path.GetFileNameWithoutExtension(prefabPath)}", currentStep / stepCount);
					}
				}

				if (dirtyPrefabCache)
				{
					if (!Directory.Exists(Path.GetDirectoryName(PORTER_PREFAB_CACHE_PATH)))
						Directory.CreateDirectory(Path.GetDirectoryName(PORTER_PREFAB_CACHE_PATH));
					File.WriteAllText(PORTER_PREFAB_CACHE_PATH, JsonConvert.SerializeObject(prefabCache));
				}
				currentStep += 1;

				// Collect dependency information
				EditorUtility.DisplayProgressBar("Step 3", "Collecting dependency information", currentStep / stepCount);
				string[] dependencies = AssetDatabase.GetDependencies(GetFilesRecursive(bundleFolderPath).ToArray(), true);
				var addressableSettings = AddressableAssetSettingsDefaultObject.Settings;
				DependencyContainerJson dependencyContainer = new DependencyContainerJson();
				dependencyContainer.dependencies = new List<DependencyInfo>();
				foreach (string dependencyPath in dependencies)
				{
					Type dependencyType = AssetDatabase.GetMainAssetTypeAtPath(dependencyPath);
					if (dependencyType == typeof(GameObject) || dependencyType == typeof(MonoScript))
						continue;

					string dependencyGuid = AssetDatabase.AssetPathToGUID(dependencyPath);
					if (addressableSettings.FindAssetEntry(dependencyGuid) != null)
						continue;

					dependencyContainer.dependencies.Add(new DependencyInfo() { path = dependencyPath, guid = dependencyGuid });
				}
				currentStep += 1;

				// Export bundle as unity package
				EditorUtility.DisplayProgressBar("Step 4", "Exporting bundle", currentStep / stepCount);
				if (!Directory.Exists(PORTER_TEMP_PATH))
					Directory.CreateDirectory(PORTER_TEMP_PATH);
				string tempPackagePath = Path.Combine(PORTER_TEMP_PATH, PACKAGE_PATH);
				if (File.Exists(tempPackagePath))
					File.Delete(tempPackagePath);
				AssetDatabase.ExportPackage(bundleFolderPath, tempPackagePath, ExportPackageOptions.Recurse);
				currentStep += 1;

				// Save file
				if (File.Exists(exportPath))
					File.Delete(exportPath);

				using (ZipArchive zip = new ZipArchive(File.Open(exportPath, FileMode.Create, FileAccess.ReadWrite), ZipArchiveMode.Create))
				{
					// Add porter data
					using (var porterDataEntry = new StreamWriter(zip.CreateEntry(PORTER_DATA_PATH).Open()))
						porterDataEntry.Write(JsonConvert.SerializeObject(porterData));

					// Add script info
					using (var scriptInfoEntry = new StreamWriter(zip.CreateEntry(SCRIPT_INFO_PATH).Open()))
						scriptInfoEntry.Write(JsonConvert.SerializeObject(new ScriptInfoJson() { scripts = projectScripts }));

					// Add prefab info
					using (var prefabInfoEntry = new StreamWriter(zip.CreateEntry(PREFAB_INFO_PATH).Open()))
						prefabInfoEntry.Write(JsonConvert.SerializeObject(prefabContainer));

					// Add dependency info
					using (var dependencyInfoEntry = new StreamWriter(zip.CreateEntry(DEPENDENCY_INFO_PATH).Open()))
						dependencyInfoEntry.Write(JsonConvert.SerializeObject(dependencyContainer));

					// Add package
					using (var packageEntry = zip.CreateEntry(PACKAGE_PATH).Open())
					using (var packageFileStream = File.Open(tempPackagePath, FileMode.Open, FileAccess.Read))
						packageFileStream.CopyTo(packageEntry);
				}

				if (File.Exists(tempPackagePath))
					File.Delete(tempPackagePath);
				EditorUtility.DisplayDialog("Success", "Successfully ported the bundle", "Close");
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				EditorUtility.DisplayDialog("Error", $"Error thrown while porting\n\n{e.Message}", "Close");
				return;
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}
		}

		#region Port Scripts
		static void PortGuids(IEnumerable<string> assetsToPort, Dictionary<string, string> guidConversionMap)
		{
			AssetDatabase.StartAssetEditing();
			try
			{
				List<string> filesToChange = assetsToPort.Where(path => ValidGuidFileExtensions.Contains(Path.GetExtension(path))).ToList();
				for (int i = 0; i < filesToChange.Count; i++)
				{
					EditorUtility.DisplayProgressBar("Porting scripts", filesToChange[i], (float)i / filesToChange.Count);
					ApplyGuidChange(filesToChange[i], guidConversionMap);
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
				AssetDatabase.StopAssetEditing();
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
		#endregion

		#region Port Prefab Ids
		private static void PortIDs(string path, Dictionary<string, Dictionary<string, string>> conversionMap)
		{
			string ReadID(StringBuilder builder, int index)
			{
				StringBuilder num = new StringBuilder();
				while (char.IsDigit(builder[index]))
					num.Append(builder[index++]);
				return num.ToString();
			}

			StringBuilder sb = new StringBuilder(File.ReadAllText(path));

			int chunkCursor = 0;
			List<string> chunks = new List<string>();

			const string fileIdText = "{fileID: ";
			int nextIdIndex = SbIndexOf(sb, fileIdText, 0, false);
			while (nextIdIndex != -1)
			{
				nextIdIndex += fileIdText.Length;
				int idChunkIndex = nextIdIndex;
				string id = ReadID(sb, nextIdIndex);
				nextIdIndex += id.Length;
				if (sb.Length > (nextIdIndex + ", guid: ".Length) && sb.ToString(nextIdIndex, ", guid: ".Length) == ", guid: ")
				{
					int rightChunkIndex = nextIdIndex;
					nextIdIndex += ", guid: ".Length;
					string guid = sb.ToString(nextIdIndex, 32);

					if (conversionMap.TryGetValue(guid, out Dictionary<string, string> idConversionMap))
					{
						if (idConversionMap.TryGetValue(id, out string newId))
						{
							// Create chunk
							string leftChunk = sb.ToString(chunkCursor, idChunkIndex - chunkCursor);
							string middleChunk = newId;
							chunkCursor = rightChunkIndex;

							chunks.Add(leftChunk);
							chunks.Add(middleChunk);
						}
					}
				}

				nextIdIndex = SbIndexOf(sb, fileIdText, nextIdIndex + 1, false);
			}

			// Add the last chunk
			if (chunkCursor != sb.Length)
				chunks.Add(sb.ToString(chunkCursor, sb.Length - chunkCursor));

			// Overwrite the file
			using (StreamWriter writer = new StreamWriter(File.Open(path, FileMode.Open, FileAccess.Write)))
			{
				writer.BaseStream.Seek(0, SeekOrigin.Begin);
				writer.BaseStream.SetLength(0);

				foreach (string chunk in chunks)
					writer.Write(chunk);
			}
		}

		private static void _CreateIdMap(PrefabNode localNode, PrefabNode remoteNode, Dictionary<string, string> map)
		{
			if (!map.ContainsKey(remoteNode.transformId))
				map[remoteNode.transformId] = localNode.transformId;
			if (!map.ContainsKey(remoteNode.gameObjectId))
				map[remoteNode.gameObjectId] = localNode.gameObjectId;

			// Component mapping
			Dictionary<string, List<string>> localComponentMap = new Dictionary<string, List<string>>();
			Dictionary<string, List<string>> remoteComponentMap = new Dictionary<string, List<string>>();
			for (int i = 0; i < localNode.components.Count; i++)
			{
				string compType = localNode.components[i].type;
				if (compType == "")
					continue;

				List<string> compMap = null;
				if (!localComponentMap.TryGetValue(compType, out compMap))
				{
					compMap = new List<string>();
					localComponentMap[compType] = compMap;
				}

				compMap.Add(localNode.components[i].id);
			}
			for (int i = 0; i < remoteNode.components.Count; i++)
			{
				string compType = remoteNode.components[i].type;
				if (compType == "")
					continue;

				List<string> compMap = null;
				if (!remoteComponentMap.TryGetValue(compType, out compMap))
				{
					compMap = new List<string>();
					remoteComponentMap[compType] = compMap;
				}

				compMap.Add(remoteNode.components[i].id);
			}

			foreach (var remoteComps in remoteComponentMap)
			{
				if (localComponentMap.TryGetValue(remoteComps.Key, out var localComps))
				{
					int limit = Math.Min(localComps.Count, remoteComps.Value.Count);
					for (int i = 0; i < limit; i++)
					{
						map[remoteComps.Value[i]] = localComps[i];
					}
				}
			}

			// Map objects based on name and order
			Dictionary<string, int> objectNameSkipCount = new Dictionary<string, int>();
			for (int i = 0; i < remoteNode.children.Count; i++)
			{
				PrefabNode remoteChild = remoteNode.children[i];
				int skipCount = 0;
				objectNameSkipCount.TryGetValue(remoteChild.gameObjectName, out skipCount);

				PrefabNode localChild = localNode.children.Where(o => o.gameObjectName == remoteChild.gameObjectName).Skip(skipCount).FirstOrDefault();
				if (localChild != null)
					_CreateIdMap(localChild, remoteChild, map);

				objectNameSkipCount[remoteChild.gameObjectName] = skipCount + 1;
			}
		}

		private static void CreateIdMap(PrefabNode localNode, PrefabNode remoteNode, Dictionary<string, string> map)
		{
			_CreateIdMap(localNode, remoteNode, map);
		}

		private static void PortPrefabs(IEnumerable<string> assetsToPort, Dictionary<string, PrefabNode> sourceProjectPrefabs, Dictionary<string, PrefabNode> otherProjectPrefabs)
		{
			Dictionary<string, Dictionary<string, string>> conversionMap = new Dictionary<string, Dictionary<string, string>>();

			foreach (var otherPrefab in otherProjectPrefabs)
			{
				if (!sourceProjectPrefabs.TryGetValue(otherPrefab.Key, out PrefabNode sourcePrefab))
					continue;

				var idMap = new Dictionary<string, string>();
				CreateIdMap(sourcePrefab, otherPrefab.Value, idMap);
				conversionMap[otherPrefab.Key] = idMap;
			}

			assetsToPort = assetsToPort.Where(path => ValidGuidFileExtensions.Contains(Path.GetExtension(path)));
			int len = assetsToPort.Count();
			float i = 0;
			try
			{
				foreach (string assetPath in assetsToPort)
				{
					EditorUtility.DisplayProgressBar($"Porting IDs", Path.GetFileNameWithoutExtension(assetPath), i++ / len);
					PortIDs(assetPath, conversionMap);
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
		}
		#endregion

		public static void PortBundleIn()
		{
			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				return;

			string portFilePath = EditorUtility.OpenFilePanel("Open port file", Application.dataPath, PORT_EXT);
			if (string.IsNullOrEmpty(portFilePath) || !File.Exists(portFilePath))
				return;

			PorterDataJson data = null;
			try
			{
				using (ZipArchive zip = new ZipArchive(File.Open(portFilePath, FileMode.Open)))
				using (StreamReader dataReader = new StreamReader(zip.GetEntry(PORTER_DATA_PATH).Open()))
					data = JsonConvert.DeserializeObject<PorterDataJson>(dataReader.ReadToEnd());
			}
			catch (Exception)
			{
				EditorUtility.DisplayDialog("Error", "Not a valid port file", "Close");
				return;
			}

			if (data.version != PORTER_VERSION)
			{
				if (!EditorUtility.DisplayDialog("Warning", $"Port file is built with {(new Version(data.version) < new Version(PORTER_VERSION) ? "an older version of exporter" : "a newer version of exporter")} (current exporter: {PORTER_VERSION}, file: {data.version}). Continue anyways?", "Continue", "Abort"))
					return;
			}

			if (Directory.Exists(data.bundleFolderPath))
			{
				EditorUtility.DisplayDialog("Error", $"Bundle {data.bundleName} already exists, the bundle must be deleted before it can be ported.", "Close");
				return;
			}

			string currentDataPath = AssetDatabase.GUIDToAssetPath(data.bundleGuid);
			string currentFolderPath = AssetDatabase.GUIDToAssetPath(data.folderGuid);
			if ((!string.IsNullOrEmpty(currentDataPath) && File.Exists(currentDataPath)) || (!string.IsNullOrEmpty(currentFolderPath) && Directory.Exists(currentFolderPath)))
			{
				EditorUtility.DisplayDialog("Error", $"Bundle {data.bundleName} already exists (same GUID), the bundle must be deleted before it can be ported.", "Close");
				return;
			}

			if (!EditorUtility.DisplayDialog("Ready to port", $"Bundle name: {data.bundleName}\nAuthor: {data.bundleAuthor}", "Port", "Cancel"))
				return;

			// Extract data from the bundle
			ScriptInfoJson otherScripts = null;
			PrefabContainerJson otherPrefabs = null;
			DependencyContainerJson otherDependencies = null;

			if (!Directory.Exists(PORTER_TEMP_PATH))
				Directory.CreateDirectory(PORTER_TEMP_PATH);
			string packagePath = Path.Combine(PORTER_TEMP_PATH, PACKAGE_PATH);
			if (File.Exists(packagePath))
				File.Delete(packagePath);

			using (ZipArchive zip = new ZipArchive(File.Open(portFilePath, FileMode.Open)))
			{
				using (var scriptInfoEntry = new StreamReader(zip.GetEntry(SCRIPT_INFO_PATH).Open()))
					otherScripts = JsonConvert.DeserializeObject<ScriptInfoJson>(scriptInfoEntry.ReadToEnd());

				using (var prefabContainerEntry = new StreamReader(zip.GetEntry(PREFAB_INFO_PATH).Open()))
					otherPrefabs = JsonConvert.DeserializeObject<PrefabContainerJson>(prefabContainerEntry.ReadToEnd(), new JsonSerializerSettings() { MaxDepth = 256 });
				
				using (var dependencyInfoEntry = new StreamReader(zip.GetEntry(DEPENDENCY_INFO_PATH).Open()))
					otherDependencies = JsonConvert.DeserializeObject<DependencyContainerJson>(dependencyInfoEntry.ReadToEnd());

				using (var packageEntry = zip.GetEntry(PACKAGE_PATH).Open())
				using (var packageFileStream = File.Open(packagePath, FileMode.Create, FileAccess.Write))
					packageEntry.CopyTo(packageFileStream);
			}

			// AssetDatabase.importPackageCompleted += PostImport;
			// AssetDatabase.ImportPackage(packagePath, false);

			MethodInfo importImmediately = typeof(AssetDatabase).GetMethod("ImportPackageImmediately", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
			importImmediately.Invoke(null, new object[] { packagePath });
			AssetDatabase.Refresh();

			File.Delete(packagePath);
			string folderPath = AssetDatabase.GUIDToAssetPath(data.folderGuid);
			if (string.IsNullOrEmpty(folderPath))
				folderPath = data.bundleFolderPath;
			while (folderPath.EndsWith("/"))
				folderPath = folderPath.Substring(0, folderPath.Length - 1);
			List<string> assetsToPort = GetFilesRecursive(folderPath).ToList();

			const float stepCount = 2;
			int currentStep = 0;

			try
			{
				// Port scripts and dependencies
				{
					EditorUtility.DisplayProgressBar("Step 1", $"Porting scripts and dependencies", currentStep / stepCount);

					Dictionary<string, string> sourceScriptNameToGuid = GetAllScripts().ToDictionary(scriptInfo => scriptInfo.fullClassName, scriptInfo => scriptInfo.guid);
					Dictionary<string, string> otherScriptNameToGuid = otherScripts.scripts.ToDictionary(scriptInfo => scriptInfo.fullClassName, scriptInfo => scriptInfo.guid);
					Dictionary<string, string> guidMap = new Dictionary<string, string>();

					foreach (var sourceScriptInfo in sourceScriptNameToGuid)
					{
						if (otherScriptNameToGuid.TryGetValue(sourceScriptInfo.Key, out string otherGuid) && sourceScriptInfo.Value != otherGuid)
							guidMap[otherGuid] = sourceScriptInfo.Value;
					}

					int i = 0;
					foreach (var dependencyInfo in otherDependencies.dependencies)
					{
						EditorUtility.DisplayProgressBar("Step 1", $"Porting scripts and dependencies {Path.GetFileName(dependencyInfo.path)}", i++ / (float)otherDependencies.dependencies.Count);

						if (!File.Exists(dependencyInfo.path))
							continue;

						string sourceGuid = AssetDatabase.AssetPathToGUID(dependencyInfo.path);
						if (string.IsNullOrEmpty(sourceGuid) || sourceGuid == dependencyInfo.guid)
							continue;

						guidMap[dependencyInfo.guid] = sourceGuid;
					}

					PortGuids(assetsToPort, guidMap);
				}
				currentStep += 1;

				// Port prefabs
				{
					PrefabInfoCacheJson prefabCache;
					bool dirtyPrefabCache = false;
					if (File.Exists(PORTER_PREFAB_CACHE_PATH))
					{
						try
						{
							prefabCache = JsonConvert.DeserializeObject<PrefabInfoCacheJson>(File.ReadAllText(PORTER_PREFAB_CACHE_PATH));
						}
						catch (Exception e)
						{
							Debug.LogWarning($"Failed to deserialize cache. Creating a new one.\n{e.StackTrace}");
							prefabCache = new PrefabInfoCacheJson();
							prefabCache.prefabGuidToCache = new Dictionary<string, PrefabInfoCache>();
							dirtyPrefabCache = true;
						}
					}
					else
					{
						prefabCache = new PrefabInfoCacheJson();
						prefabCache.prefabGuidToCache = new Dictionary<string, PrefabInfoCache>();
						dirtyPrefabCache = true;
					}

					PrefabContainerJson sourcePrefabContainer = new PrefabContainerJson();
					sourcePrefabContainer.prefabs = new Dictionary<string, PrefabNode>();
					foreach (string prefabPath in AssetDatabase.GetAllAssetPaths().Where(path => (path.EndsWith(".prefab") || path.EndsWith(".fbx") || path.EndsWith(".obj")) && !path.StartsWith("Assets/ULTRAKILL Others")))
					{
						GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
						if (prefab == null)
							continue;

						sourcePrefabContainer.prefabs[AssetDatabase.AssetPathToGUID(prefabPath)] = ProcessPrefab(prefabPath, prefabCache, out bool makeDirty);
						if (makeDirty)
						{
							dirtyPrefabCache = true;
							EditorUtility.DisplayProgressBar("Step 2", $"Collecting prefab information: {Path.GetFileNameWithoutExtension(prefabPath)}", currentStep / stepCount);
						}
						else
						{
							EditorUtility.DisplayProgressBar("Step 2", $"Collecting prefab information: (Cached) {Path.GetFileNameWithoutExtension(prefabPath)}", currentStep / stepCount);
						}
					}

					if (dirtyPrefabCache)
					{
						if (!Directory.Exists(Path.GetDirectoryName(PORTER_PREFAB_CACHE_PATH)))
							Directory.CreateDirectory(Path.GetDirectoryName(PORTER_PREFAB_CACHE_PATH));
						File.WriteAllText(PORTER_PREFAB_CACHE_PATH, JsonConvert.SerializeObject(prefabCache));
					}

					EditorUtility.DisplayProgressBar("Step 2", $"Porting prefabs", currentStep / stepCount);
					PortPrefabs(assetsToPort, sourcePrefabContainer.prefabs, otherPrefabs.prefabs);
				}
				currentStep += 1;
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				EditorUtility.DisplayDialog("Error", $"Error thrown while porting\n\n{e.Message}", "Close");
				return;
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Success", "Successfully ported bundle", "Close");

			RudeExporter wnd = GetWindow<RudeExporter>();
			if (wnd != null)
			{
				wnd.titleContent = new GUIContent("Rude Exporter");

				BundleFolderAsset folderAsset = new BundleFolderAsset(data.folderGuid);
				if (folderAsset.bundleData != null)
					wnd.DisplayBundleModifyWindow(data.folderGuid);
				else
					wnd.DisplayBundleWindow();
			}
		}
	}

	// References
	private static VisualTreeAsset bundleWindow;
	private static VisualTreeAsset bundleElement;
	private static VisualTreeAsset createBundleWindow;
	private static VisualTreeAsset bundleModifyWindow;
	private static VisualTreeAsset levelElement;
	private static VisualTreeAsset levelModifyWindow;
	private static VisualTreeAsset requiredLevelField;
	private static VisualTreeAsset onlineGuidSetWindow;
	private static VisualTreeAsset onlineGuidSetElement;
	private static VisualTreeAsset settingsWindow;
	private static Texture2D standardVariant;
	private static Texture2D secretVariant;

	// Visual tree containers
	private class BundleElement
    {
        public readonly VisualElement root;
        public readonly Label bundleName;
		public readonly Label bundleAuthor;
        public readonly VisualElement icon;
		public readonly VisualElement onlineIcon;
		public readonly VisualElement controlContainer;
		public readonly Button deleteButton;
        public readonly Button modifyButton;
        public readonly Button exportButton;

		public BundleElement(VisualElement target)
        {
            TemplateContainer tempContainer = bundleElement.CloneTree();

            root = tempContainer.contentContainer[0];
            target.Add(root);

            bundleName = root.Q<Label>("bundle-name");
            bundleAuthor = root.Q<Label>("bundle-author");
            icon = root.Q("icon");
            onlineIcon = root.Q("online-icon");
            controlContainer = root.Q("control-container");
            deleteButton = root.Q<Button>("delete-bundle");
            modifyButton = root.Q<Button>("modify-bundle");
            exportButton = root.Q<Button>("export-bundle");

            controlContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            modifyButton.RegisterCallback<MouseEnterEvent>((e) =>
            {
				controlContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
			});
			modifyButton.RegisterCallback<MouseLeaveEvent>((e) =>
			{
				controlContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
			});
		}
    }

    private class CreateBundleWindow
    {
        public readonly VisualElement root;
		public readonly TextField bundleFolder;
        public readonly Toggle bundleFolderToggle;
		public readonly TextField bundleName;
		public readonly TextField bundleAuthor;
        public readonly ObjectField bundleIcon;
        public readonly Button createBundle;
        public readonly Button cancelBottom;
        public readonly Button cancelTop;

		public CreateBundleWindow(VisualElement target)
        {
            root = createBundleWindow.CloneTree().contentContainer[0];
            target.Add(root);

            bundleFolder = root.Q<TextField>("folder-name");
            bundleFolderToggle = root.Q<Toggle>("folder-name-toggle");
            bundleName = root.Q<TextField>("bundle-name");
            bundleAuthor = root.Q<TextField>("author");
            bundleIcon = root.Q<ObjectField>("bundle-icon");
            bundleIcon.objectType = typeof(Sprite);
            createBundle = root.Q<Button>("create-button");
            cancelBottom = root.Q<Button>("cancel-bottom");
            cancelTop = root.Q<Button>("cancel-top");
        }
    }

    private class BundleModifyWindow
    {
		public readonly VisualElement root;
        public readonly VisualElement icon;
        public readonly Label bundleName;
        public readonly Label author;
		public readonly Button goBack;
		public readonly Button openFolder;
        public readonly Button export;
		public readonly Button port;
		public readonly Button addressables;
        public readonly TextField bundleNameInput;
        public readonly TextField authorInput;
        public readonly ObjectField iconInput;
        public readonly VisualElement levelContainer;
        public readonly Button createLevel;
        public readonly Button importScene;

		public readonly Button backupButton;
		public readonly Label guidInfo;
		public readonly Button regenGuid;
		public readonly Button setGuidFromOnline;

        public readonly VisualElement onlineInfo;
        public readonly Label rating;
        public readonly VisualElement onlineFoldout;
        public readonly VisualElement onlineChangelogContainer;

		public BundleModifyWindow(VisualElement target)
		{
			root = bundleModifyWindow.CloneTree().contentContainer[0];
			target.Add(root);

            icon = root.Q("icon");
            bundleName = root.Q<Label>("bundle-name");
            author = root.Q<Label>("bundle-author");
            goBack = root.Q<Button>("go-back");
            openFolder = root.Q<Button>("open-folder");
            export = root.Q<Button>("export");
			port = root.Q<Button>("port");
            addressables = root.Q<Button>("addressables");
			bundleNameInput = root.Q<TextField>("bundle-name-input");
            bundleNameInput.isDelayed = true;
			authorInput = root.Q<TextField>("author-input");
			authorInput.isDelayed = true;
			iconInput = root.Q<ObjectField>("icon-input");
            iconInput.objectType = typeof(Sprite);
            levelContainer = root.Q("level-container");
            createLevel = root.Q<Button>("create-level");
            importScene = root.Q<Button>("import-scene");

			backupButton = root.Q<Button>("backup");
			guidInfo = root.Q<Label>("guid-info");
			regenGuid = root.Q<Button>("regen-guid");
			setGuidFromOnline = root.Q<Button>("set-guid");

            onlineInfo = root.Q("online-info");
            onlineFoldout = root.Q("online-foldout");
            onlineChangelogContainer = onlineFoldout.Q("unity-content");
            rating = root.Q<Label>("rating");
		}
	}

	private class LevelElement
	{
		public readonly VisualElement root;
        public readonly Label levelName;
        public readonly VisualElement controlContainer;
        public readonly VisualElement orderContainer;
        public readonly VisualElement icon;
        public readonly Button delete;
        public readonly Button modify;
        public readonly Button open;
        public readonly Button up;
        public readonly Button down;

		public LevelElement(VisualElement target)
		{
			root = levelElement.CloneTree().contentContainer[0];
			target.Add(root);

            levelName = root.Q<Label>("level-name");
            controlContainer = root.Q("control-container");
            orderContainer = root.Q("order-container");
            icon = root.Q("icon");
            delete = root.Q<Button>("delete-level");
            modify = root.Q<Button>("modify-level");
            open = root.Q<Button>("open-level");
            up = root.Q<Button>("move-up");
            down = root.Q<Button>("move-down");

            controlContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            orderContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
			root.RegisterCallback<MouseEnterEvent>((e) =>
			{
				controlContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
				orderContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
			});
			root.RegisterCallback<MouseLeaveEvent>((e) =>
			{
				controlContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
				orderContainer.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
			});
		}
	}

	private class BundleFolderAsset
    {
        public readonly string folderGuid;
        public string bundleGuid { get; private set; }

        public RudeBundleData bundleData;
        public List<RudeLevelData> levels = new List<RudeLevelData>();

        public void Refresh()
        {
            bundleData = null;
            levels.Clear();

            if (string.IsNullOrEmpty(folderGuid))
                return;

            string path = AssetDatabase.GUIDToAssetPath(folderGuid);
            if (string.IsNullOrEmpty(path))
                return;

			if (!AssetDatabase.IsValidFolder(path + "/Data"))
				AssetDatabase.CreateFolder(path, "Data");
			if (!AssetDatabase.IsValidFolder(path + "/Addressables"))
				AssetDatabase.CreateFolder(path, "Addressables");

			string[] files = Directory.GetFiles(path + "/Data");
			
			bundleData = files
                .Where(pth => AssetDatabase.GetMainAssetTypeAtPath(pth) == typeof(RudeBundleData))
                .Select(pth => AssetDatabase.LoadAssetAtPath<RudeBundleData>(pth))
                .FirstOrDefault();

			if (bundleData == null)
			{
                bundleData = ScriptableObject.CreateInstance<RudeBundleData>();
				bundleData.bundleName = "Unnamed bundle";
				bundleData.author = "Unknown";
                
				AssetDatabase.CreateAsset(bundleData, AssetDatabase.GenerateUniqueAssetPath(path + "/Data/bundleData.asset"));
			}

            bundleGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bundleData));

            levels.AddRange(files
				.Where(pth => AssetDatabase.GetMainAssetTypeAtPath(pth) == typeof(RudeLevelData))
				.Select(pth => AssetDatabase.LoadAssetAtPath<RudeLevelData>(pth)));

            foreach (var level in levels)
                if (level.requiredCompletedLevelIdsForUnlock == null)
                    level.requiredCompletedLevelIdsForUnlock = new string[0];
		}

        public BundleFolderAsset(string folderGuid)
        {
            this.folderGuid = folderGuid;

            Refresh();
        }

		public string GetHash(bool progressBar = false)
		{
			Refresh();
			if (bundleData == null)
				return "deadbeef000000000000000000000000";

			if (progressBar)
				EditorUtility.DisplayProgressBar("Calculating hash", "...", 0f);

			MD5 hash = MD5.Create();

			string folderPath = AssetDatabase.GUIDToAssetPath(folderGuid);
			byte[] buffer = new byte[4096];
			foreach (string file in GetFilesRecursive(folderPath).OrderBy(e => e))
			{
				/*using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read))
				{
					int num;
					do
					{
						num = fs.Read(buffer, 0, 4096);
						if (num > 0)
							hash.TransformBlock(buffer, 0, num, null, 0);
					}
					while (num > 0);
				}*/

				FileInfo fileInfo = new FileInfo(file);
				if (!fileInfo.Exists)
					continue;

				hash.TransformBlock(BitConverter.GetBytes(fileInfo.LastWriteTime.ToFileTime()), 0, 8, null, 0);
			}

			hash.TransformFinalBlock(new byte[0], 0, 0);
			byte[] finalHashArr = hash.Hash;
			string finalHash = BitConverter.ToString(finalHashArr).Replace("-", "").ToLower();

			if (progressBar)
				EditorUtility.ClearProgressBar();
			return finalHash;
		}
	}

	private class LevelModifyWindow
	{
		public readonly VisualElement root;

        public readonly TextField levelId;
		public readonly VisualElement levelIdInfo;
		public readonly TextField levelName;
        public readonly ObjectField icon;
        public readonly Toggle challenge;
        public readonly TextField challengeText;
        public readonly Toggle secretLevel;
        public readonly Toggle hideIfNotPlayed;
        public readonly Label hideIfNotPlayedInfo;
        public readonly VisualElement requirementContainer;
        public readonly Button addRequirement;
		public readonly VisualElement secretPreview;

		public readonly Label levelNamePreview;
        public readonly VisualElement iconPreview;

        public readonly Button goBack;
        public readonly Button open;

		public LevelModifyWindow(VisualElement target)
		{
			root = levelModifyWindow.CloneTree().contentContainer[0];
			target.Add(root);

            levelId = root.Q<TextField>("level-id");
            levelId.isDelayed = true;
			levelIdInfo = root.Q("level-id-info");
			levelName = root.Q<TextField>("level-name");
            levelName.isDelayed = true;
            icon = root.Q<ObjectField>("icon");
            icon.objectType = typeof(Sprite);
            challenge = root.Q<Toggle>("challenge");
            challengeText = root.Q<TextField>("challenge-text");
            challengeText.isDelayed = true;
            secretLevel = root.Q<Toggle>("secret-level");
            hideIfNotPlayed = root.Q<Toggle>("hide-from-list");
            hideIfNotPlayedInfo = root.Q<Label>("hide-from-list-info");
            requirementContainer = root.Q("requirement-container");
            addRequirement = root.Q<Button>("add-requirement");
			secretPreview = root.Q("secret-preview");

            levelNamePreview = root.Q<Label>("level-name-preview");
            iconPreview = root.Q("icon-preview");

            goBack = root.Q<Button>("go-back");
            open = root.Q<Button>("open-scene");
		}
	}

	private class RequiredLevelField
	{
		public readonly VisualElement root;

        public readonly PopupField<string> levels;
        public readonly Button remove;
		
		public RequiredLevelField(VisualElement target, List<string> choices, int defaultIndex)
		{
			root = requiredLevelField.CloneTree().contentContainer[0];
			target.Add(root);

            levels = new PopupField<string>("Level", choices, defaultIndex);
            levels.style.flexGrow = new StyleFloat(1f);
            levels.style.flexShrink = new StyleFloat(1f);
            root.Add(levels);

            remove = root.Q<Button>("remove-button");
		}
	}

	private class OnlineGuidSetWindow
	{
		public readonly VisualElement root;
		public readonly Button goBack;
		public readonly PopupField<string> sortBy;
		public readonly Toggle reverse;
		public readonly VisualElement container;

		public OnlineGuidSetWindow(VisualElement target)
		{
			root = onlineGuidSetWindow.CloneTree().contentContainer[0];
			target.Add(root);

			goBack = root.Q<Button>("go-back");

			VisualElement filters = root.Q("filters");
			sortBy = new PopupField<string>("Sort by", new List<string>() { "Last update", "Upload date", "Name", "Author" }, 0);
			filters.Add(sortBy);
			reverse = new Toggle("Reverse sort");
			filters.Add(reverse);

			container = root.Q("unity-content-container");
		}
	}

	private class OnlineGuidSetElement
	{
		public readonly VisualElement root;
		public readonly Label bundleName;
		public readonly Label author;
		public readonly VisualElement icon;
		public readonly Button select;

		public OnlineGuidSetElement(VisualElement target)
		{
			root = onlineGuidSetElement.CloneTree().contentContainer[0];
			target.Add(root);

			bundleName = root.Q<Label>("bundle-name");
			author = root.Q<Label>("bundle-author");
			icon = root.Q("icon");
			select = root.Q<Button>("select-bundle");
		}
	}

	private class SettingsWindow
	{
		public readonly VisualElement root;
		public readonly Button goBack;

		public readonly TextField output;
		public readonly Button openOutput;
		public readonly Toggle confirmExport;
		public readonly Toggle exportSummary;
		public readonly Toggle warningUnchanged;

		public readonly TextField backupPath;
		public readonly Button openBackup;
		public readonly Toggle backupOnExport;
		public readonly VisualElement backupDiv;
		public readonly IntegerField backupMaxSizeMB;
		public readonly IntegerField backupBundleMaxSizeMB;
		public readonly Label backupInfo;

		public readonly Button clearLevelCache;
		public readonly Button clearAddrCache;

		public SettingsWindow(VisualElement target)
		{
			root = settingsWindow.CloneTree().contentContainer[0];
			target.Add(root);

			goBack = root.Q<Button>("go-back");

			output = root.Q<TextField>("output");
			output.isDelayed = true;
			openOutput = root.Q<Button>("open-output");
			confirmExport = root.Q<Toggle>("confirm-export");
			exportSummary = root.Q<Toggle>("export-summary");

			warningUnchanged = root.Q<Toggle>("warn-unchanged");

			backupPath = root.Q<TextField>("backup-path");
			openBackup = root.Q<Button>("open-backup-folder");
			backupOnExport = root.Q<Toggle>("backup-on-export");
			backupDiv = root.Q<VisualElement>("backup-div");
			backupMaxSizeMB = root.Q<IntegerField>("backup-size");
			backupBundleMaxSizeMB = root.Q<IntegerField>("backup-bundle-size");
			backupInfo = root.Q<Label>("backup-info");

			clearLevelCache = root.Q<Button>("clear-levels");
			clearAddrCache = root.Q<Button>("clear-addr");
		}
	}

    private void RemoveCurrentWindow()
    {
        while (rootVisualElement.childCount != 0)
            rootVisualElement.RemoveAt(0);
	}

    private void DisplayBundleWindow()
    {
		RudeExporterSettings exporterSettings = DefaultExporterSetting;

        RemoveCurrentWindow();
        VisualElement window = bundleWindow.CloneTree()[0];
        rootVisualElement.Add(window);

        window.Q<Button>("go-back").SetEnabled(false);

        VisualElement list = window.Q("bundle-container");

        void RefreshBundles()
        {
            if (window == null || window.parent == null)
            {
                Undo.undoRedoPerformed -= RefreshBundles;
                return;
			}

			if (!AssetDatabase.IsValidFolder("Assets/Maps"))
				AssetDatabase.CreateFolder("Assets", "Maps");

            while (list.childCount != 0)
                list.RemoveAt(0);

			foreach (string bundleFolderPath in AssetDatabase.GetSubFolders("Assets/Maps"))
			{
				string guid = AssetDatabase.AssetPathToGUID(bundleFolderPath);
				BundleFolderAsset bundle = new BundleFolderAsset(guid);

				BundleElement bundleElement = new BundleElement(list);
				bundleElement.bundleName.text = RemoveTags(bundle.bundleData.bundleName);
				bundleElement.bundleAuthor.text = RemoveTags(bundle.bundleData.author);
				if (bundle.bundleData.levelIcon != null)
					bundleElement.icon.style.backgroundImage = new StyleBackground(bundle.bundleData.levelIcon.texture);

				bundleElement.modifyButton.clicked += () =>
				{
					Undo.undoRedoPerformed -= RefreshBundles;

					if (!DisplayBundleModifyWindow(guid))
						DisplayBundleWindow();
				};

                bundleElement.deleteButton.clicked += () =>
                {
                    if (!EditorUtility.DisplayDialog("Warning", $"Do you want to delete bundle '{bundle.bundleData.bundleName}'?", "Yes", "No"))
                        return;

                    string folderPath = AssetDatabase.GUIDToAssetPath(bundle.folderGuid);
                    if (!string.IsNullOrEmpty(folderPath))
                    {
                        if (AssetDatabase.MoveAssetToTrash(folderPath))
                            EditorUtility.DisplayDialog("Rude Exporter", "Moved bundle to the trash", "Close");
                        else
							EditorUtility.DisplayDialog("Error", "Failed to move the bundle to the trash", "Close");
					}

                    RefreshBundles();
                };

                bundleElement.exportButton.clicked += () =>
                {
					InternalExporter.TryExport(bundle, exporterSettings);
				};

                OnlineCatalogManager.LoadCatalog((catalog) =>
                {
					if (window == null || window.parent == null)
						return;

					if (catalog.Levels.Where(onlineLevel => onlineLevel.Guid == bundle.bundleGuid).Any())
						bundleElement.onlineIcon.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
				});
            }
        }

        Undo.undoRedoPerformed += RefreshBundles;
        RefreshBundles();

		Button createBundleButton = window.Q<Button>("bundle-add");
        createBundleButton.clicked += () =>
        {
			Undo.undoRedoPerformed -= RefreshBundles;
			DisplayBundleCreateWindow();
        };

		Button portButton = window.Q<Button>("port");
		portButton.clicked += () =>
		{
			InternalPorter.PortBundleIn();
		};

		Button refreshButton = window.Q<Button>("refresh");
		refreshButton.clicked += () =>
		{
            RefreshBundles();
		};

		Button settingsButton = window.Q<Button>("settings");
		settingsButton.clicked += () =>
		{
			DisplaySettingsWindow(false);
		};
	}

    private void DisplayBundleCreateWindow()
    {
        RemoveCurrentWindow();
        CreateBundleWindow window = new CreateBundleWindow(rootVisualElement);

        window.cancelTop.clicked += DisplayBundleWindow;
        window.cancelBottom.clicked += DisplayBundleWindow;

        window.bundleName.RegisterValueChangedCallback((bundleName) =>
        {
            if (!window.bundleFolderToggle.value)
                window.bundleFolder.SetValueWithoutNotify(RemoveTags(bundleName.newValue));
        });

        window.bundleFolderToggle.RegisterValueChangedCallback((e) =>
        {
            window.bundleFolder.SetEnabled(e.newValue);

            if (!e.newValue)
                window.bundleFolder.SetValueWithoutNotify(window.bundleName.value);
        });

        window.bundleFolder.SetEnabled(window.bundleFolderToggle.value);

        window.createBundle.clicked += () =>
        {
            string folderName = RemoveTags(window.bundleName.value);
            if (window.bundleFolderToggle.value)
                folderName = window.bundleFolder.value;

            if (string.IsNullOrEmpty(folderName))
            {
                folderName = window.bundleName.value;
                if (string.IsNullOrEmpty(folderName))
                {
                    folderName = "Unknown bundle";
                }
            }

            {
                string newFolderName = folderName;
                int i = 1;
                while (AssetDatabase.IsValidFolder($"Assets/Maps/{newFolderName}"))
                {
                    newFolderName = $"{folderName} {i++}";
                }

                folderName = newFolderName;
            }

            string bundlePath = $"Assets/Maps/{folderName}";
            AssetDatabase.CreateFolder("Assets/Maps", folderName);  
            string dataPath = $"Assets/Maps/{folderName}/Data";
			AssetDatabase.CreateFolder($"Assets/Maps/{folderName}", "Data");

			RudeBundleData bundleData = ScriptableObject.CreateInstance<RudeBundleData>();

            bundleData.bundleName = window.bundleName.value;
            if (string.IsNullOrEmpty(bundleData.bundleName))
                bundleData.bundleName = "Unnamed bundle";
            bundleData.author = window.bundleAuthor.value;
            if (string.IsNullOrEmpty(bundleData.author))
                bundleData.author = "Unknown";
            if (window.bundleIcon.value != null)
            {
				bundleData.levelIcon = (Sprite)window.bundleIcon.value;
            }

            AssetDatabase.CreateAsset(bundleData, $"{dataPath}/bundleData.asset");

            DisplayBundleModifyWindow(AssetDatabase.AssetPathToGUID(bundlePath));
        };
    }

    private bool DisplayBundleModifyWindow(string bundleFolderGuid)
    {
        if (string.IsNullOrEmpty(bundleFolderGuid))
            return false;

        BundleFolderAsset bundle = new BundleFolderAsset(bundleFolderGuid);
        if (bundle.bundleData == null)
            return false;

        RemoveCurrentWindow();
        BundleModifyWindow window = new BundleModifyWindow(rootVisualElement);

		void RefreshLevels()
		{
            bundle.Refresh();

			while (window.levelContainer.childCount != 0)
				window.levelContainer.RemoveAt(0);

			foreach (RudeLevelData level in bundle.levels.OrderBy(l => l.prefferedLevelOrder))
			{
				LevelElement levelElement = new LevelElement(window.levelContainer);

				levelElement.levelName.text = RemoveTags(level.levelName);
				if (level.levelPreviewImage != null)
					levelElement.icon.style.backgroundImage = new StyleBackground(level.levelPreviewImage.texture);

				levelElement.up.clicked += () =>
				{
					bundle.Refresh();

					List<RudeLevelData> levels = new List<RudeLevelData>(bundle.levels.OrderBy(l => l.prefferedLevelOrder));
					int levelIndex = levels.IndexOf(level);

					if (levelIndex != -1 && levelIndex != 0)
					{
						RudeLevelData tmp = levels[levelIndex - 1];
						levels[levelIndex - 1] = level;
						levels[levelIndex] = tmp;
					}

                    Undo.RecordObjects(levels.ToArray(), "Change Level Order");

					for (int i = 0; i < levels.Count; i++)
					{
						levels[i].prefferedLevelOrder = i;
						EditorUtility.SetDirty(levels[i]);
					}

					RefreshLevels();
				};

				levelElement.down.clicked += () =>
				{
					bundle.Refresh();

					List<RudeLevelData> levels = new List<RudeLevelData>(bundle.levels.OrderBy(l => l.prefferedLevelOrder));
					int levelIndex = levels.IndexOf(level);

					if (levelIndex != -1 && levelIndex < levels.Count - 1)
					{
						RudeLevelData tmp = levels[levelIndex + 1];
						levels[levelIndex + 1] = level;
						levels[levelIndex] = tmp;
					}

					Undo.RecordObjects(levels.ToArray(), "Change Level Order");

					for (int i = 0; i < levels.Count; i++)
					{
						levels[i].prefferedLevelOrder = i;
						EditorUtility.SetDirty(levels[i]);
					}

					RefreshLevels();
				};

                levelElement.modify.clicked += () =>
                {
                    DisplayLevelModifyWindow(bundle, level);
                };

                levelElement.open.clicked += () =>
                {
                    if (level.targetScene == null || !(level.targetScene is SceneAsset))
                    {
                        if (!EditorUtility.DisplayDialog("Error", "Scene not found for this level. Do you want to create a new scene now?", "Yes", "No"))
                            return;

						string folderPath = AssetDatabase.GUIDToAssetPath(bundle.folderGuid);
						string dataPath = $"{folderPath}/Data";

						string newScenePath = AssetDatabase.GenerateUniqueAssetPath($"{dataPath}/Level {level.prefferedLevelOrder}.unity");
						SceneAsset templateScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Custom/essentials.unity");
						if (templateScene == null)
							templateScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Editor/Exporter/templateScene.unity");

						if (templateScene != null)
						{
							Scene templateSceneObj = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(templateScene), OpenSceneMode.Additive);
							EditorSceneManager.SaveScene(templateSceneObj, newScenePath, true);
							EditorSceneManager.CloseScene(templateSceneObj, true);
							level.targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
						}
						else
						{
							Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
							EditorSceneManager.SaveScene(newScene, newScenePath);
							EditorSceneManager.CloseScene(newScene, true);
							SceneAsset newSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
							if (newScene == null)
								throw new Exception("Rude exported failed to create a new scene for the level");
							level.targetScene = newSceneAsset;
						}

                        EditorUtility.SetDirty(level);
					}

                    SceneAsset targetScene = level.targetScene as SceneAsset;
                    if (targetScene == null)
                    {
                        EditorUtility.DisplayDialog("Error", "Failed to open scene", "Ok");
                        return;
                    }

                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        return;

                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(targetScene), OpenSceneMode.Single);
                };

				levelElement.delete.clicked += () =>
				{
					if (!EditorUtility.DisplayDialog("Warning", $"Do you want to delete level '{level.levelName}'?", "Yes", "No"))
						return;

                    if (level.targetScene != null && level.targetScene is SceneAsset sceneToDelete)
                    {
                        string scenePathToDelete = AssetDatabase.GetAssetPath(sceneToDelete);

                        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                        {
                            Scene s = EditorSceneManager.GetSceneAt(i);
                            if (scenePathToDelete == s.path)
                            {
                                if (EditorSceneManager.sceneCount == 1)
                                    EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

                                EditorSceneManager.SaveScene(s);
                                EditorSceneManager.CloseScene(s, true);
                                break;
                            }
                        }

                        AssetDatabase.MoveAssetToTrash(scenePathToDelete);
                    }

                    AssetDatabase.MoveAssetToTrash(AssetDatabase.GetAssetPath(level));
					EditorUtility.DisplayDialog("Rude Exporter", "Moved level to the trash", "Close");
					RefreshLevels();
				};
			}
		}

		void UpdateUI()
        {
            if (window == null || window.root == null || window.root.parent == null)
            {
                Undo.undoRedoPerformed -= UpdateUI;
                return;
            }

            bundle.Refresh();
			window.guidInfo.text = $"Bundle GUID: {AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(bundle.bundleData))}";

			window.bundleName.text = RemoveTags(bundle.bundleData.bundleName);
			window.author.text = "by " + RemoveTags(bundle.bundleData.author);
			if (bundle.bundleData.levelIcon != null)
			{
				window.icon.style.backgroundImage = new StyleBackground(bundle.bundleData.levelIcon.texture);
			}

			window.bundleNameInput.value = bundle.bundleData.bundleName;
			window.authorInput.value = bundle.bundleData.author;
			if (bundle.bundleData.levelIcon != null)
			{
				window.iconInput.value = bundle.bundleData.levelIcon;
			}

			RefreshLevels();
		}

        UpdateUI();
        Undo.undoRedoPerformed += UpdateUI;

        window.bundleNameInput.RegisterValueChangedCallback((e) =>
        {
            window.bundleName.text = RemoveTags(e.newValue);

            if (e.newValue != e.previousValue)
            {
                bundle.Refresh();
                if (bundle.bundleData != null)
                {
                    Undo.RecordObject(bundle.bundleData, "Change Bundle Name");

                    bundle.bundleData.bundleName = e.newValue;
                    EditorUtility.SetDirty(bundle.bundleData);
                }
            }
		});

        window.authorInput.RegisterValueChangedCallback((e) =>
		{
			window.author.text = "by " + RemoveTags(e.newValue);

			if (e.newValue != e.previousValue)
			{
				bundle.Refresh();
				if (bundle.bundleData != null)
				{
					Undo.RecordObject(bundle.bundleData, "Change Bundle Author");

					bundle.bundleData.author = e.newValue;
					EditorUtility.SetDirty(bundle.bundleData);
				}
			}
		});

		window.iconInput.RegisterValueChangedCallback((e) =>
		{
            if (e.newValue == null)
                window.icon.style.backgroundImage = new StyleBackground();
            else
				window.icon.style.backgroundImage = new StyleBackground(((Sprite)e.newValue).texture);

			if (e.newValue != e.previousValue)
			{
				bundle.Refresh();
				if (bundle.bundleData != null)
				{
					Undo.RecordObject(bundle.bundleData, "Change Bundle Icon");

					bundle.bundleData.levelIcon = e.newValue as Sprite;
					EditorUtility.SetDirty(bundle.bundleData);
				}
			}
		});

		window.backupButton.clicked += () =>
		{
			var result = InternalExporter.BackupBundle(bundle);
			if (result.success)
				EditorUtility.DisplayDialog("Success", "Backup file saved to " + result.filePath, "Close");
		};

        window.goBack.clicked += DisplayBundleWindow;

        window.openFolder.clicked += () =>
        {
            string folderPath = AssetDatabase.GUIDToAssetPath(bundleFolderGuid);
            if (string.IsNullOrEmpty(folderPath))
                return;

            UnityEngine.Object folderObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(folderPath);
            if (folderObj == null)
                return;

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = folderObj;
            EditorGUIUtility.PingObject(folderObj);
		};

        window.createLevel.clicked += () =>
        {
            bundle.Refresh();

            int order = 0;
            if (bundle.levels.Count != 0)
                order = bundle.levels.OrderByDescending(l => l.prefferedLevelOrder).First().prefferedLevelOrder + 1;

            string folderPath = AssetDatabase.GUIDToAssetPath(bundle.folderGuid);
            string dataPath = $"{folderPath}/Data";

            RudeLevelData newLevelData = ScriptableObject.CreateInstance<RudeLevelData>();

			string newScenePath = AssetDatabase.GenerateUniqueAssetPath($"{dataPath}/Level {order}.unity");
			SceneAsset templateScene= AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Custom/essentials.unity");
            if (templateScene == null)
				templateScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Editor/Exporter/templateScene.unity");
            
			if (templateScene != null)
            {
                Scene templateSceneObj = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(templateScene), OpenSceneMode.Additive);
				EditorSceneManager.SaveScene(templateSceneObj, newScenePath, true);
				EditorSceneManager.CloseScene(templateSceneObj, true);
                newLevelData.targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
            }
            else
            {
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SaveScene(newScene, newScenePath);
                EditorSceneManager.CloseScene(newScene, true);
                SceneAsset newSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
                if (newScene == null)
                    throw new Exception("Rude exported failed to create a new scene for the level");
                newLevelData.targetScene = newSceneAsset;
            }

            newLevelData.levelName = Path.GetFileNameWithoutExtension(newScenePath);
			
            string levelDataPath = AssetDatabase.GenerateUniqueAssetPath($"{dataPath}/Level {order}.asset");
            AssetDatabase.CreateAsset(newLevelData, levelDataPath);
            RefreshLevels();
        };

        window.importScene.clicked += () =>
        {
            string scenePath = EditorUtility.OpenFilePanel("Open scene", Application.dataPath, "unity");
            if (string.IsNullOrEmpty(scenePath))
                return;

			string projDir = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
            if (scenePath.StartsWith(projDir))
            {
                scenePath = scenePath.Substring(projDir.Length);
            }
            else
            {
				EditorUtility.DisplayDialog("Error", $"Scene must be inside the project.", "Close");
				return;
			}

            if (AssetDatabase.GetMainAssetTypeAtPath(scenePath) != typeof(SceneAsset))
            {
                EditorUtility.DisplayDialog("Error", $"Open a valid scene file. Selected file was {AssetDatabase.GetMainAssetTypeAtPath(scenePath)}.", "Close");
                return;
            }

            if (scenePath == "Assets/Custom/essentials.unity")
            {
				EditorUtility.DisplayDialog("Error", "Cannot import special scene 'essentials'.", "Close");
				return;
			}

			if (scenePath == "Assets/Editor/Exporter/templateScene.unity")
			{
				EditorUtility.DisplayDialog("Error", "Cannot import special scene 'templateScene'.", "Close");
				return;
			}

			SceneAsset targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

			List<RudeLevelData> allLevels = new List<RudeLevelData>(FindAssetsByType<RudeLevelData>());
            RudeLevelData sceneOwner = allLevels.Where(l => l.targetScene == targetScene).FirstOrDefault();

            if (sceneOwner != null)
            {
				if (!EditorUtility.DisplayDialog("Error", $"This scene is already owned by {(string.IsNullOrEmpty(sceneOwner.levelName) ? "<unnamed level>" : sceneOwner.levelName)}. Do you want to import it anyway? (Other level will have an empty scene)", "Move", "Cancel"))
					return;

				sceneOwner.targetScene = null;
				EditorUtility.SetDirty(sceneOwner);
			}

			bundle.Refresh();

			int order = 0;
			if (bundle.levels.Count != 0)
				order = bundle.levels.OrderByDescending(l => l.prefferedLevelOrder).First().prefferedLevelOrder + 1;

			string folderPath = AssetDatabase.GUIDToAssetPath(bundle.folderGuid);
			string dataPath = $"{folderPath}/Data";

			RudeLevelData newLevelData = ScriptableObject.CreateInstance<RudeLevelData>();
			newLevelData.levelName = Path.GetFileNameWithoutExtension(scenePath);

			string newScenePath = AssetDatabase.GenerateUniqueAssetPath($"{dataPath}/{Path.GetFileNameWithoutExtension(scenePath)}.unity");
			string levelDataPath = AssetDatabase.GenerateUniqueAssetPath($"{dataPath}/Level {order}.asset");

            string moveErr = AssetDatabase.MoveAsset(scenePath, newScenePath);
            if (!string.IsNullOrEmpty(moveErr))
            {
                EditorUtility.DisplayDialog("Error", $"Could not move asset.\n\n{moveErr}", "Close");
                return;
            }

            newLevelData.targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
			AssetDatabase.CreateAsset(newLevelData, levelDataPath);
			RefreshLevels();
		};

        window.export.clicked += () =>
        {
			InternalExporter.TryExport(bundle, DefaultExporterSetting);
		};

		window.port.clicked += () =>
		{
			InternalPorter.PortBundleOut(bundle);
		};

        window.addressables.clicked += () =>
        {
            InternalExporter.MakeAddressable(bundle);
        };

		window.regenGuid.clicked += () =>
		{
			if (!EditorUtility.DisplayDialog("Warning", "Do you want to regenerate bundle GUID? Not suggested unless you know what you are doing.", "Regenerate", "Cancel"))
				return;

			bundle.Refresh();
			string dataPath = AssetDatabase.GetAssetPath(bundle.bundleData);
			SetGuid(dataPath + ".meta", GUID.Generate().ToString());
			EditorUtility.SetDirty(bundle.bundleData);
			AssetDatabase.Refresh();
			UpdateUI();
		};

		window.setGuidFromOnline.clicked += () =>
		{
			DisplaySetOnlineGuidWindow(bundle);
		};

        OnlineCatalogManager.LoadCatalog((catalog) =>
        {
            if (window == null || window.root == null || window.root.parent == null)
                return;

            var level = catalog.Levels.Where(onlineLevel => onlineLevel.Guid == bundle.bundleGuid).FirstOrDefault();
            if (level != null)
            {
                window.onlineInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                window.onlineFoldout.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                
                window.rating.text = "Rating: loading...";
				while (window.onlineChangelogContainer.childCount != 0)
					window.onlineChangelogContainer.RemoveAt(0);

				foreach (string updateText in level.Updates.Select(u => u.Message))
				{
					Label updateLabel = new Label("Update");
					updateLabel.style.color = new StyleColor(Color.green);
					if (window.onlineChangelogContainer.childCount == 0)
					{
						updateLabel.text = "Release";
						updateLabel.style.color = new StyleColor(Color.yellow);
					}

					window.onlineChangelogContainer.Add(updateLabel);
					window.onlineChangelogContainer.Add(new Label(updateText + "\n\n"));
				}
				if (level.Updates.Count > 1)
					((Label)window.onlineChangelogContainer.ElementAt(window.onlineChangelogContainer.childCount - 2)).text = "Latest Update";

				AngryServerManager.LoadVotes((response) =>
                {
					if (window == null || window.root == null || window.root.parent == null)
						return;

                    if (response.status != 0)
                    {
						window.rating.text = "Rating: error";
                        return;
					}

                    if (response.bundles.TryGetValue(bundle.bundleGuid, out var voteInfo))
						window.rating.text = $"Rating: {voteInfo.upvotes - voteInfo.downvotes}\nTotal plays: Loading...";
                    else
						window.rating.text = "Rating: unknown\nTotal plays: Loading...";

					AngryServerManager.LoadPostCount(bundle.bundleGuid, (postCount) =>
					{
						if (window == null || window.root == null || window.root.parent == null)
							return;

						string result = postCount == -1 ? "error" : postCount.ToString();

						if (voteInfo != null)
							window.rating.text = $"Rating: {voteInfo.upvotes - voteInfo.downvotes}\nTotal plays: {result}";
						else
							window.rating.text = $"Rating: unknown\nTotal plays: {result}";
					});
				});
            }
            else
            {
                window.onlineInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                window.onlineFoldout.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
        });

		return true;
    }

    private bool DisplayLevelModifyWindow(BundleFolderAsset bundle, RudeLevelData levelData)
    {
        if (bundle == null || levelData == null)
            return false;

        bundle.Refresh();
        if (!bundle.levels.Contains(levelData))
            return false;

        RemoveCurrentWindow();
        LevelModifyWindow window = new LevelModifyWindow(rootVisualElement);
		StyleColor originalLabelColor = window.levelId.labelElement.style.color;
		StyleColor highlightedLabelColor = new StyleColor(Color.yellow);

		void RefreshUI()
        {
            if (window == null || window.root == null || window.root.parent == null)
            {
				Undo.undoRedoPerformed -= RefreshUI;
                return;
			}

            bool emptyId = string.IsNullOrEmpty(levelData.uniqueIdentifier);

			window.levelNamePreview.text = RemoveTags(levelData.levelName);
            if (levelData.levelPreviewImage == null)
                window.iconPreview.style.backgroundImage = new StyleBackground();
            else
				window.iconPreview.style.backgroundImage = new StyleBackground(levelData.levelPreviewImage.texture);

            window.levelId.SetValueWithoutNotify(levelData.uniqueIdentifier);
			window.levelId.labelElement.style.color = string.IsNullOrEmpty(levelData.uniqueIdentifier) ? highlightedLabelColor : originalLabelColor;

			window.levelName.SetValueWithoutNotify(levelData.levelName);
            window.icon.SetValueWithoutNotify(levelData.levelPreviewImage);

            window.challenge.SetValueWithoutNotify(levelData.levelChallengeEnabled);
            window.challengeText.SetValueWithoutNotify(levelData.levelChallengeText);
            window.challengeText.SetEnabled(levelData.levelChallengeEnabled);

			window.secretLevel.SetValueWithoutNotify(levelData.isSecretLevel);
			window.secretPreview.style.backgroundImage = new StyleBackground(levelData.isSecretLevel ? secretVariant : standardVariant);

			window.hideIfNotPlayed.SetValueWithoutNotify(levelData.hideIfNotPlayed);
            if (levelData.hideIfNotPlayed)
                window.hideIfNotPlayedInfo.text = "Level will be hidden from the list unless played before. Useful for secret levels which should not be visible until the level is entered from a secret room in another level.";
            else
				window.hideIfNotPlayedInfo.text = "Level will always be visible on the list";

			while (window.requirementContainer.childCount != 0)
				window.requirementContainer.RemoveAt(0);

            List<string> ids = bundle.levels.Where(l => !string.IsNullOrEmpty(l.uniqueIdentifier)).Select(l => l.uniqueIdentifier).ToList();
            ids.Insert(0, "");
            List<string> choices = bundle.levels.Where(l => !string.IsNullOrEmpty(l.uniqueIdentifier)).Select(l => $"(Level {l.prefferedLevelOrder + 1}) {RemoveTags(l.levelName)}").ToList();
            choices.Insert(0, "None");

            bool modified = false;
            for (int i = 0; i < levelData.requiredCompletedLevelIdsForUnlock.Length; i++)
            {
                int requirementIndex = i;
                string currentId = levelData.requiredCompletedLevelIdsForUnlock[i];
                int currentIdIndex = ids.IndexOf(currentId);

				if (currentIdIndex == -1)
                {
                    currentId = "";
                    levelData.requiredCompletedLevelIdsForUnlock[i] = "";
                    modified = true;
				}

                RequiredLevelField field = new RequiredLevelField(window.requirementContainer, choices, currentIdIndex);
                field.levels.RegisterValueChangedCallback((e) =>
                {
                    if (e.newValue == e.previousValue)
                        return;

                    int newIndex = choices.IndexOf(e.newValue);
                    
					Undo.RecordObject(levelData, "Change Required Level For Unlock");
					levelData.requiredCompletedLevelIdsForUnlock[requirementIndex] = ids[newIndex];
					EditorUtility.SetDirty(levelData);
					RefreshUI();
				});
                field.remove.clicked += () =>
                {
					Undo.RecordObject(levelData, "Remove Required Level For Unlock");
                    List<string> newRequirements = new List<string>(levelData.requiredCompletedLevelIdsForUnlock);
					newRequirements.RemoveAt(requirementIndex);
					levelData.requiredCompletedLevelIdsForUnlock = newRequirements.ToArray();
					EditorUtility.SetDirty(levelData);
					RefreshUI();
                };

                if (emptyId)
                {
                    field.levels.SetEnabled(false);
                    field.remove.SetEnabled(false);
				}
            }

            if (modified)
                EditorUtility.SetDirty(levelData);

			window.levelIdInfo.style.display = new StyleEnum<DisplayStyle>(emptyId ? DisplayStyle.Flex : DisplayStyle.None);
			window.levelName.SetEnabled(!emptyId);
			window.challenge.SetEnabled(!emptyId);
            window.challengeText.SetEnabled(!emptyId && levelData.levelChallengeEnabled);
			window.icon.SetEnabled(!emptyId);
            window.secretLevel.SetEnabled(!emptyId);
            window.hideIfNotPlayed.SetEnabled(!emptyId);
            window.addRequirement.SetEnabled(!emptyId);
		}

        RefreshUI();
        Undo.undoRedoPerformed += RefreshUI;

		window.addRequirement.clicked += () =>
		{
            Undo.RecordObject(levelData, "Add Required Level For Unlock");

            List<string> newRequirements = new List<string>(levelData.requiredCompletedLevelIdsForUnlock);
            newRequirements.Add("");

            levelData.requiredCompletedLevelIdsForUnlock = newRequirements.ToArray();
            EditorUtility.SetDirty(levelData);
            RefreshUI();
		};

        window.goBack.clicked += () =>
        {
            if (!DisplayBundleModifyWindow(bundle.folderGuid))
                DisplayBundleWindow();
        };

        window.open.clicked += () =>
		{
			if (levelData.targetScene == null || !(levelData.targetScene is SceneAsset))
			{
				if (!EditorUtility.DisplayDialog("Error", "Scene not found for this level. Do you want to create a new scene now?", "Yes", "No"))
					return;

				string folderPath = AssetDatabase.GUIDToAssetPath(bundle.folderGuid);
				string dataPath = $"{folderPath}/Data";

				string newScenePath = AssetDatabase.GenerateUniqueAssetPath($"{dataPath}/Level {levelData.prefferedLevelOrder}.unity");
				SceneAsset templateScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Custom/essentials.unity");
				if (templateScene == null)
					templateScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Editor/Exporter/templateScene.unity");

				if (templateScene != null)
				{
					Scene templateSceneObj = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(templateScene), OpenSceneMode.Additive);
					EditorSceneManager.SaveScene(templateSceneObj, newScenePath, true);
					EditorSceneManager.CloseScene(templateSceneObj, true);
					levelData.targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
				}
				else
				{
					Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
					EditorSceneManager.SaveScene(newScene, newScenePath);
					EditorSceneManager.CloseScene(newScene, true);
					SceneAsset newSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(newScenePath);
					if (newScene == null)
						throw new Exception("Rude exported failed to create a new scene for the level");
					levelData.targetScene = newSceneAsset;
				}

				EditorUtility.SetDirty(levelData);
			}

			SceneAsset targetScene = levelData.targetScene as SceneAsset;
			if (targetScene == null)
			{
				EditorUtility.DisplayDialog("Error", "Failed to open scene", "Ok");
				return;
			}

			if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
				return;

			EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(targetScene), OpenSceneMode.Single);
		};

		// Values
		window.levelId.RegisterValueChangedCallback((e) =>
        {
            if (e.newValue == e.previousValue)
                return;

            string previousId = levelData.uniqueIdentifier;

            List<RudeLevelData> allLevels = new List<RudeLevelData>(FindAssetsByType<RudeLevelData>().Where(l => l != levelData));
            if (!string.IsNullOrEmpty(e.newValue) && allLevels.Where(l => l.uniqueIdentifier == e.newValue).Any())
            {
                EditorUtility.DisplayDialog("Error", $"Could not change level id to {e.newValue} because another level already has it", "Close");
                window.levelId.SetValueWithoutNotify(e.previousValue);
                return;
            }

            List<RudeLevelData> dirtyLevels = new List<RudeLevelData>();
            dirtyLevels.Add(levelData);
            if (!string.IsNullOrEmpty(previousId))
            {
                dirtyLevels.AddRange(allLevels.Where(l => l.requiredCompletedLevelIdsForUnlock.Contains(previousId)));
            }

            Undo.RecordObjects(dirtyLevels.ToArray(), "Change Level Id");
            levelData.uniqueIdentifier = e.newValue;
			EditorUtility.SetDirty(levelData);
			foreach (var otherLevel in dirtyLevels)
            {
                List<string> newRequirements = new List<string>(otherLevel.requiredCompletedLevelIdsForUnlock);
                for (int i = 0; i < newRequirements.Count; i++)
                    if (newRequirements[i] == previousId)
                        newRequirements[i] = e.newValue;

                otherLevel.requiredCompletedLevelIdsForUnlock = newRequirements.ToArray();
                EditorUtility.SetDirty(otherLevel);
            }

            RefreshUI();
        });

        window.levelName.RegisterValueChangedCallback((e) =>
        {
            if (e.previousValue == e.newValue)
                return;

            window.levelNamePreview.text = RemoveTags(e.newValue);

            Undo.RecordObject(levelData, "Change Level Name");
            levelData.levelName = e.newValue;
            EditorUtility.SetDirty(levelData);

            RefreshUI();
        });

		window.icon.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

            Sprite newValue = e.newValue == null ? null : (Sprite)e.newValue;
			if (newValue == null)
				window.iconPreview.style.backgroundImage = new StyleBackground();
			else
				window.iconPreview.style.backgroundImage = new StyleBackground(newValue.texture);

			Undo.RecordObject(levelData, "Change Level Icon");
			levelData.levelPreviewImage = newValue;
			EditorUtility.SetDirty(levelData);
		});

		window.challenge.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			window.challengeText.SetEnabled(e.newValue);

			Undo.RecordObject(levelData, "Change Level Challenge Toggle");
			levelData.levelChallengeEnabled = e.newValue;
			EditorUtility.SetDirty(levelData);
		});

		window.challengeText.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(levelData, "Change Level Challenge Text");
			levelData.levelChallengeText = e.newValue;
			EditorUtility.SetDirty(levelData);
		});

		window.secretLevel.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			window.secretPreview.style.backgroundImage = new StyleBackground(e.newValue ? secretVariant : standardVariant);

			Undo.RecordObject(levelData, "Change Level Secret Level Toggle");
			levelData.isSecretLevel = e.newValue;
			EditorUtility.SetDirty(levelData);
		});

		window.hideIfNotPlayed.RegisterValueChangedCallback((e) =>
        {
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(levelData, "Change Hide If Not Played Toggle");
			levelData.hideIfNotPlayed = e.newValue;
			EditorUtility.SetDirty(levelData);

			if (e.newValue)
				window.hideIfNotPlayedInfo.text = "Level will be hidden from the list unless played before. Useful for secret levels which should not be visible until the level is entered from a secret room in another level.";
			else
				window.hideIfNotPlayedInfo.text = "Level will always be visible on the list";
		});

		return true;
    }

	private void DisplaySetOnlineGuidWindow(BundleFolderAsset bundle)
	{
		RemoveCurrentWindow();
		OnlineGuidSetWindow window = new OnlineGuidSetWindow(rootVisualElement);

		window.goBack.clicked += () =>
		{
			DisplayBundleModifyWindow(bundle.folderGuid);
		};

		void RefreshBundles()
		{
			OnlineCatalogManager.LoadCatalog((catalog) =>
			{
				if (window == null || window.root == null || window.root.parent == null)
					return;

				while (window.container.childCount != 0)
					window.container.RemoveAt(0);

				// "Last update", "Upload date", "Name", "Author"
				IEnumerable<OnlineCatalogManager.LevelInfo> orderedLevels;
				if (window.sortBy.index == 1)
					orderedLevels = catalog.Levels.AsEnumerable();
				else if (window.sortBy.index == 2)
					orderedLevels = catalog.Levels.OrderBy(l => l.Name);
				else if (window.sortBy.index == 3)
					orderedLevels = catalog.Levels.OrderBy(l => l.Author);
				else
					orderedLevels = catalog.Levels.OrderByDescending(l => l.LastUpdate);

				if (window.reverse.value)
					orderedLevels = orderedLevels.Reverse();

				foreach (var level in orderedLevels)
				{
					OnlineGuidSetElement elem = new OnlineGuidSetElement(window.container);
					elem.bundleName.text = RemoveTags(level.Name);
					elem.author.text = "by " + RemoveTags(level.Author);

					OnlineCatalogManager.LoadIcon(level.Guid, (icon) =>
					{
						if (elem == null || elem.icon == null)
							return;

						elem.icon.style.backgroundImage = new StyleBackground(icon);
					});

					elem.select.clicked += () =>
					{
						bundle.Refresh();
						if (!EditorUtility.DisplayDialog("Warning", $"Do you want to set GUID of bundle '{bundle.bundleData.bundleName}' to the GUID of bundle '{level.Name}'?", "Yes", "Cancel"))
							return;

						string newGuid = level.Guid;
						if (!string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(newGuid)))
						{
							EditorUtility.DisplayDialog("Error", $"Asset at path '{AssetDatabase.GUIDToAssetPath(newGuid)}' already has this GUID.", "Close");
							return;
						}

						string dataPath = AssetDatabase.GetAssetPath(bundle.bundleData);
						SetGuid(dataPath + ".meta", newGuid);
						EditorUtility.SetDirty(bundle.bundleData);
						AssetDatabase.Refresh();
						EditorUtility.DisplayDialog("Rude Exporter", "Successfully set the GUID.", "Close");
						DisplayBundleModifyWindow(bundle.folderGuid);
					};
				}
			});
		}

		window.sortBy.RegisterValueChangedCallback((newVal) =>
		{
			RefreshBundles();
		});

		window.reverse.RegisterValueChangedCallback((newVal) =>
		{
			RefreshBundles();
		});

		RefreshBundles();
	}

    private void DisplaySettingsWindow(bool indicateOutput)
    {
		RudeExporterSettings exporterSettings = DefaultExporterSetting;

		RemoveCurrentWindow();
		SettingsWindow window = new SettingsWindow(rootVisualElement);

		if (indicateOutput)
			window.output.ElementAt(0).style.color = new StyleColor(Color.red);

		window.goBack.clicked += () =>
		{
			DisplayBundleWindow();
		};

		window.clearLevelCache.clicked += () =>
		{
			if (!EditorUtility.DisplayDialog("Clear build cache", "Delete custom levels cache?", "Yes", "No"))
				return;

			string customSceneCachePath = Path.Combine(Application.dataPath, "../", "BuiltBundles");
			if (Directory.Exists(customSceneCachePath))
				Directory.Delete(customSceneCachePath, true);
			string tempBundleDir = Path.Combine(Application.dataPath, "../", "TempBundle");
			if (Directory.Exists(tempBundleDir))
				Directory.Delete(tempBundleDir, true);
		};

		window.clearAddrCache.clicked += () =>
		{
			if (!EditorUtility.DisplayDialog("Clear build cache", "Delete all ultrakill bundle cache? Note: It will take a long time to regenerate all the bundles on the next build", "Yes", "No"))
				return;

			AddressableAssetSettings.CleanPlayerContent(null);
			BuildCache.PurgeCache(false);
		};

		window.output.SetValueWithoutNotify(exporterSettings.buildPath);
		window.output.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(exporterSettings, "Changed Output Path");
			exporterSettings.buildPath = e.newValue;
			EditorUtility.SetDirty(exporterSettings);
		});

		window.confirmExport.SetValueWithoutNotify(exporterSettings.promptBeforeExport);
		window.confirmExport.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(exporterSettings, "Toggled Export Confirm");
			exporterSettings.promptBeforeExport = e.newValue;
			EditorUtility.SetDirty(exporterSettings);
		});

		window.exportSummary.SetValueWithoutNotify(exporterSettings.summaryAfterExport);
		window.exportSummary.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(exporterSettings, "Toggled Export Confirm");
			exporterSettings.summaryAfterExport = e.newValue;
			EditorUtility.SetDirty(exporterSettings);
		});

		window.warningUnchanged.SetValueWithoutNotify(exporterSettings.checkForChanges);
		window.warningUnchanged.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(exporterSettings, "Toggled Unchanged Warning");
			exporterSettings.checkForChanges = e.newValue;
			EditorUtility.SetDirty(exporterSettings);
		});

		window.backupPath.SetValueWithoutNotify(exporterSettings.backupPath);
		window.backupPath.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(exporterSettings, "Changed backup path");
			exporterSettings.backupPath = e.newValue;
			EditorUtility.SetDirty(exporterSettings);
		});

		window.backupOnExport.SetValueWithoutNotify(exporterSettings.enableBackups);
		window.backupOnExport.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(exporterSettings, "Toggled Backup On Export");
			exporterSettings.enableBackups = e.newValue;
			EditorUtility.SetDirty(exporterSettings);

			window.backupDiv.style.display = e.newValue ? DisplayStyle.Flex : DisplayStyle.None;
		});
		window.backupDiv.style.display = exporterSettings.enableBackups ? DisplayStyle.Flex : DisplayStyle.None;

		window.backupMaxSizeMB.SetValueWithoutNotify(exporterSettings.maxBackupFolderSizeMB);
		window.backupMaxSizeMB.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(exporterSettings, "Modified Backup Folder Size");
			exporterSettings.maxBackupFolderSizeMB = e.newValue;
			EditorUtility.SetDirty(exporterSettings);
		});

		window.backupBundleMaxSizeMB.SetValueWithoutNotify(exporterSettings.maxSizePerBundleMB);
		window.backupBundleMaxSizeMB.RegisterValueChangedCallback((e) =>
		{
			if (e.previousValue == e.newValue)
				return;

			Undo.RecordObject(exporterSettings, "Modified Bundle Backup Folder Size");
			exporterSettings.maxSizePerBundleMB = e.newValue;
			EditorUtility.SetDirty(exporterSettings);
		});

		window.backupInfo.text = $"Currently, backup folder is {InternalExporter.GetBackupFolderSizeMB()} MB";
		window.backupPath.SetValueWithoutNotify(exporterSettings.backupPath);

		void ReloadUI()
		{
			if (window == null || window.root == null || window.root.parent == null)
			{
				Undo.undoRedoPerformed -= ReloadUI;
				return;
			}

			window.output.SetValueWithoutNotify(exporterSettings.buildPath);
			window.confirmExport.SetValueWithoutNotify(exporterSettings.promptBeforeExport);
			window.exportSummary.SetValueWithoutNotify(exporterSettings.summaryAfterExport);

			window.warningUnchanged.SetValueWithoutNotify(exporterSettings.checkForChanges);
			
			window.backupPath.SetValueWithoutNotify(exporterSettings.backupPath);
			window.backupOnExport.SetValueWithoutNotify(exporterSettings.enableBackups);
			window.backupDiv.style.display = exporterSettings.enableBackups ? DisplayStyle.Flex : DisplayStyle.None;
			window.backupMaxSizeMB.SetValueWithoutNotify(exporterSettings.maxBackupFolderSizeMB);
			window.backupBundleMaxSizeMB.SetValueWithoutNotify(exporterSettings.maxSizePerBundleMB);
		};

		Undo.undoRedoPerformed += ReloadUI;

		window.openBackup.clicked += () =>
		{
			string defaultPath = RudeExporterSettings.DefaultBackupFolderPath;
			if (!Directory.Exists(defaultPath))
				Directory.CreateDirectory(defaultPath);

			bool useDefaultPath = string.IsNullOrEmpty(exporterSettings.backupPath) || !Directory.Exists(exporterSettings.backupPath);

			string newOutput = EditorUtility.OpenFolderPanel("Open backup path", useDefaultPath ? Path.GetDirectoryName(defaultPath) : Path.GetDirectoryName(exporterSettings.backupPath), useDefaultPath ? "Backups" : Path.GetFileName(exporterSettings.backupPath));
			if (!string.IsNullOrEmpty(newOutput))
			{
				Undo.RecordObject(exporterSettings, "Changed Backup Path");
				exporterSettings.backupPath = newOutput;
				EditorUtility.SetDirty(exporterSettings);
				window.backupPath.SetValueWithoutNotify(newOutput);
				window.backupInfo.text = $"Currently, backup folder is {InternalExporter.GetBackupFolderSizeMB()} MB";
			}
		};

		window.openOutput.clicked += () =>
		{
			string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string defaultPath = Path.Combine(appData, "AngryLevelLoader");
			if (!Directory.Exists(defaultPath))
				defaultPath = Application.dataPath;

			string newOutput = EditorUtility.OpenFolderPanel("Open output path", defaultPath, "Levels");
			if (!string.IsNullOrEmpty(newOutput))
			{
				Undo.RecordObject(exporterSettings, "Changed Output Path");
				exporterSettings.buildPath = newOutput;
				EditorUtility.SetDirty(exporterSettings);
				window.output.SetValueWithoutNotify(newOutput);
			}
		};
    }

    public void CreateGUI()
    {
		RudeExporterSettings exporterSettings = DefaultExporterSetting;

        // Legacy converter
        try
		{
			if (File.Exists("Assets/Editor/BundleWindow.cs") && !exporterSettings.skipLegacyExporterRemoval)
			{
				int choice = EditorUtility.DisplayDialogComplex("Legacy converter", "Old exporter script found. Do you want to delete it?", "Delete", "Don't ask again", "Ignore");
				if (choice == 0)
				{
					AssetDatabase.StartAssetEditing();
					AssetDatabase.DeleteAsset("Assets/Editor/BundleWindow.cs");
					AssetDatabase.DeleteAsset("Assets/Editor/BundleWindowData.cs");
					AssetDatabase.DeleteAsset("Assets/Editor/CreateBundleWindow.cs");
					AssetDatabase.DeleteAsset("Assets/Editor/BundleWindowData.asset");
					AssetDatabase.StopAssetEditing();
					AssetDatabase.Refresh();
				}
				else if (choice == 1)
				{
					exporterSettings.skipLegacyExporterRemoval = true;
					EditorUtility.SetDirty(exporterSettings);
				}
			}

			foreach (string label in AssetDatabase.GetAllAssetBundleNames())
			{
				List<string> assets = new List<string>(AssetDatabase.GetAssetPathsFromAssetBundle(label));
				if (!assets.Select(a => AssetDatabase.GetMainAssetTypeAtPath(a)).Where(t => t == typeof(RudeLevelData)).Any())
					continue;

				if (assets
					.Where(a => AssetDatabase.GetMainAssetTypeAtPath(a) == typeof(RudeLevelData))
					.Select(a => AssetDatabase.LoadAssetAtPath<RudeLevelData>(a))
					.Where(l => l.targetScene != null && AssetDatabase.GetAssetPath(l.targetScene) == "Assets/Custom/essentials.unity")
					.Any())
					continue;

				if (exporterSettings.legacyBundleLabelsToSkip.Contains(label))
					continue;

				int choice = EditorUtility.DisplayDialogComplex("Legacy converter", $"Legacy bundle found with label '{label}'. Do you want to convert this bundle?", "Convert", "Don't ask again", "Skip");
				if (choice == 2)
					continue;

				if (choice == 1)
				{
					exporterSettings.legacyBundleLabelsToSkip.Add(label);
					EditorUtility.SetDirty(exporterSettings);
					continue;
				}

				if (choice == 0)
				{
					Debug.Log($"Found {assets.Count} assets\n\n{string.Join("\n", assets)}");

					RudeBundleData bundleData = null;
					List<RudeBundleData> allBundles = assets.Where(a => AssetDatabase.GetMainAssetTypeAtPath(a) == typeof(RudeBundleData)).Select(a => AssetDatabase.LoadAssetAtPath<RudeBundleData>(a)).ToList();
					if (allBundles.Count >= 1)
					{
						bundleData = allBundles[0];
						foreach (var bundle in allBundles)
						{
							string path = AssetDatabase.GetAssetPath(bundle);
							RemoveAssetBundleLabel(path + ".meta");
							assets.Remove(path);
						}
					}

					string folderName = "Converted Bundle";
					if (bundleData != null && !string.IsNullOrEmpty(bundleData.bundleName))
						folderName = bundleData.bundleName;

					Debug.Log($"Bundle data: {(bundleData == null ? "Not found" : "Found")}");

					{
						string newFolderName = folderName;
						int i = 1;
						while (AssetDatabase.IsValidFolder($"Assets/Maps/{newFolderName}"))
						{
							newFolderName = $"{folderName} {i++}";
						}

						folderName = newFolderName;
					}

					if (!AssetDatabase.IsValidFolder("Assets/Maps"))
						AssetDatabase.CreateFolder("Assets", "Maps");

					string bundlePath = $"Assets/Maps/{folderName}";
					AssetDatabase.CreateFolder("Assets/Maps", folderName);
					string dataPath = $"Assets/Maps/{folderName}/Data";
					AssetDatabase.CreateFolder($"Assets/Maps/{folderName}", "Data");
					string addrPath = $"Assets/Maps/{folderName}/Addressables";
					AssetDatabase.CreateFolder($"Assets/Maps/{folderName}", "Addressables");

					if (bundleData == null)
					{
						bundleData = ScriptableObject.CreateInstance<RudeBundleData>();
						bundleData.bundleName = "Unnamed Bundle";
						bundleData.author = "Unknown";
						AssetDatabase.CreateAsset(bundleData, dataPath + "/bundleData.asset");
					}
					else
					{
						Debug.Log("Bundle move err: " + AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(bundleData), dataPath + "/bundleData.asset"));
					}

					AddressableAssetSettings aSettings = AddressableAssetSettingsDefaultObject.Settings;
					foreach (var group in aSettings.groups)
					{
						if (group.Name == label)
						{
							SetGuid(dataPath + "/bundleData.asset.meta", group.Guid);

							break;
						}
					}

					foreach (RudeLevelData level in assets.Where(a => AssetDatabase.GetMainAssetTypeAtPath(a) == typeof(RudeLevelData)).Select(a => AssetDatabase.LoadAssetAtPath<RudeLevelData>(a)).ToArray())
					{
						string levelPath = AssetDatabase.GetAssetPath(level);
						string newLevelPath = AssetDatabase.GenerateUniqueAssetPath(dataPath + $"/{Path.GetFileName(levelPath)}");
						RemoveAssetBundleLabel(levelPath + ".meta");
						assets.Remove(levelPath);

						Debug.Log("Level move err: " + AssetDatabase.MoveAsset(levelPath, newLevelPath));

						if (level.targetScene != null && level.targetScene is SceneAsset)
						{
							string scenePath = AssetDatabase.GetAssetPath(level.targetScene);
							string newScenePath = AssetDatabase.GenerateUniqueAssetPath(dataPath + $"/{Path.GetFileName(scenePath)}");
							RemoveAssetBundleLabel(scenePath + ".meta");
							assets.Remove(scenePath);

							Debug.Log("Scene move err: " + AssetDatabase.MoveAsset(scenePath, newScenePath));
						}
					}

					foreach (string otherAsset in assets)
					{
						RemoveAssetBundleLabel(otherAsset + ".meta");

						if (forbiddenPaths.Where(p => otherAsset.StartsWith(p)).Any())
							continue;

						AssetDatabase.MoveAsset(otherAsset, AssetDatabase.GenerateUniqueAssetPath(addrPath + $"/{Path.GetFileName(otherAsset)}"));
					}

					BundleFolderAsset bundleAsset = new BundleFolderAsset(AssetDatabase.AssetPathToGUID(bundlePath));
					InternalExporter.MakeAddressable(bundleAsset);

					AssetDatabase.RemoveAssetBundleName(label, true);
				}
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}

		// Initialize references
		bundleWindow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/BundleWindow.uxml");
        bundleElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/BundleElement.uxml");
        createBundleWindow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/CreateBundleWindow.uxml");
		bundleModifyWindow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/BundleModifyWindow.uxml");
		levelElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/LevelElement.uxml");
		levelModifyWindow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/LevelModifyWindow.uxml");
		requiredLevelField = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/RequiredLevelField.uxml");
		onlineGuidSetWindow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/OnlineGuidSetWindow.uxml");
		onlineGuidSetElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/OnlineGuidSetElement.uxml");
		settingsWindow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/Exporter/SettingsWindow.uxml");

		standardVariant = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Exporter/standard-variant.png");
		secretVariant = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor/Exporter/secret-variant.png");

		DisplayBundleWindow();
	}

	private void OnDestroy()
	{
		OnlineCatalogManager.PurgeIconCache();
	}

	[MenuItem("RUDE/Rude Exporter", false, 0)]
	public static void DisplayWindow()
	{
		RudeExporter wnd = GetWindow<RudeExporter>();
		wnd.titleContent = new GUIContent("Rude Exporter");
	}

	[MenuItem("RUDE/Modify current bundle", false, 1)]
	public static void DisplayBundle()
	{
		Scene currentScene = EditorSceneManager.GetActiveScene();
		string scenePath = currentScene.path;
		if (string.IsNullOrEmpty(scenePath))
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
		RudeLevelData sceneOwner = FindAssetsByType<RudeLevelData>().Where(l => l.targetScene == sceneAsset).FirstOrDefault();
		if (sceneOwner == null)
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		foreach (string bundleFolder in Directory.GetDirectories("Assets/Maps"))
		{
			BundleFolderAsset bundle = new BundleFolderAsset(AssetDatabase.AssetPathToGUID(bundleFolder));
			if (!bundle.levels.Contains(sceneOwner))
				continue;

			RudeExporter wnd = GetWindow<RudeExporter>();
			wnd.titleContent = new GUIContent("Rude Exporter");
			wnd.DisplayBundleModifyWindow(AssetDatabase.AssetPathToGUID(bundleFolder));

			return;
		}

		EditorUtility.DisplayDialog("Error", "Current scene not owned by any bundle", "Close");
	}

	[MenuItem("RUDE/Modify current level", false, 2)]
	public static void DisplayLevel()
	{
		Scene currentScene = EditorSceneManager.GetActiveScene();
		string scenePath = currentScene.path;
		if (string.IsNullOrEmpty(scenePath))
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
		RudeLevelData sceneOwner = FindAssetsByType<RudeLevelData>().Where(l => l.targetScene == sceneAsset).FirstOrDefault();
		if (sceneOwner == null)
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		foreach (string bundleFolder in Directory.GetDirectories("Assets/Maps"))
		{
			BundleFolderAsset bundle = new BundleFolderAsset(AssetDatabase.AssetPathToGUID(bundleFolder));
			if (!bundle.levels.Contains(sceneOwner))
				continue;

			RudeExporter wnd = GetWindow<RudeExporter>();
			wnd.titleContent = new GUIContent("Rude Exporter");
			wnd.DisplayLevelModifyWindow(bundle, sceneOwner);

			return;
		}

		EditorUtility.DisplayDialog("Error", "Current scene not owned by any bundle", "Close");
	}

	[MenuItem("RUDE/Export current scene", false, 100)]
	public static void QuickExport()
	{
		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			return;

		Scene currentScene = EditorSceneManager.GetActiveScene();
		string scenePath = currentScene.path;
		if (string.IsNullOrEmpty(scenePath))
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
		RudeLevelData sceneOwner = FindAssetsByType<RudeLevelData>().Where(l => l.targetScene == sceneAsset).FirstOrDefault();
		if (sceneOwner == null)
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		foreach (string bundleFolder in Directory.GetDirectories("Assets/Maps"))
		{
			BundleFolderAsset bundle = new BundleFolderAsset(AssetDatabase.AssetPathToGUID(bundleFolder));
			if (!bundle.levels.Contains(sceneOwner))
				continue;

			InternalExporter.TryExport(bundle, DefaultExporterSetting);
			return;
		}

		EditorUtility.DisplayDialog("Error", "Current level not owned by any bundle", "Close");
	}

	[MenuItem("RUDE/Export current scene only", false, 101)]
	public static void QuickExportCurrentSceneOnly()
	{
		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			return;

		Scene currentScene = EditorSceneManager.GetActiveScene();
		string scenePath = currentScene.path;
		if (string.IsNullOrEmpty(scenePath))
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
		RudeLevelData sceneOwner = FindAssetsByType<RudeLevelData>().Where(l => l.targetScene == sceneAsset).FirstOrDefault();
		if (sceneOwner == null)
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		foreach (string bundleFolder in Directory.GetDirectories("Assets/Maps"))
		{
			BundleFolderAsset bundle = new BundleFolderAsset(AssetDatabase.AssetPathToGUID(bundleFolder));
			if (!bundle.levels.Contains(sceneOwner))
				continue;

			InternalExporter.TryExport(bundle, DefaultExporterSetting, new RudeLevelData[] { sceneOwner });
			return;
		}

		EditorUtility.DisplayDialog("Error", "Current level not owned by any bundle", "Close");
	}

	/*[MenuItem("RUDE/Dev/Get hash of current bundle", false, 10000)]
	public static void GetHashDev()
	{
		if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
			return;

		Scene currentScene = EditorSceneManager.GetActiveScene();
		string scenePath = currentScene.path;
		if (string.IsNullOrEmpty(scenePath))
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
		RudeLevelData sceneOwner = FindAssetsByType<RudeLevelData>().Where(l => l.targetScene == sceneAsset).FirstOrDefault();
		if (sceneOwner == null)
		{
			EditorUtility.DisplayDialog("Error", "Active scene is not located in any level", "Close");
			return;
		}

		foreach (string bundleFolder in Directory.GetDirectories("Assets/Maps"))
		{
			BundleFolderAsset bundle = new BundleFolderAsset(AssetDatabase.AssetPathToGUID(bundleFolder));
			if (!bundle.levels.Contains(sceneOwner))
				continue;

			Debug.Log(bundle.GetHash(true));
			return;
		}

		EditorUtility.DisplayDialog("Error", "Current level not owned by any bundle", "Close");
	}*/
}