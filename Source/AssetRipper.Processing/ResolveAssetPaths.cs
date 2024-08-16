using AssetRipper.Addressables;
using AssetRipper.Assets;
using AssetRipper.IO.Files.Utils;
using AssetRipper.Mining.PredefinedAssets;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_1032;
using AssetRipper.SourceGenerated.Classes.ClassID_241;
using AssetRipper.SourceGenerated.Classes.ClassID_89;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.Asset;
using AssetRipper.SourceGenerated.Subclasses.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Processing
{
	public sealed class ResolveAssetPaths : IAssetProcessor
	{
		public void Process(GameData gameData)
		{
			string? streamingAssetsPath = gameData.PlatformStructure?.StreamingAssetsPath;
			if (streamingAssetsPath == null)
			{
				throw new DirectoryNotFoundException("Could not locate the streaming assets folder");
			}

			string catalogFilePath = Path.Combine(streamingAssetsPath, "aa", "catalog.json");
			if (!File.Exists(catalogFilePath))
			{
				throw new FileNotFoundException("Could not locate the catalog file", catalogFilePath);
			}

			Catalog? catalog = Catalog.FromJsonFile(catalogFilePath);
			if (catalog == null)
			{
				throw new IOException("Invalid content catalog");
			}

			Dictionary<string, HashSet<string>> pathToOccuranceCount = new Dictionary<string, HashSet<string>>();
			Dictionary<string, string> idToOriginalPath = new Dictionary<string, string>();
			Dictionary<string, List<Entry>> idToEntries = new Dictionary<string, List<Entry>>();

			static bool isGuid(string guid)
			{
				return Guid.TryParse(guid, out _);
			}

			foreach (Entry entry in catalog.Entries)
			{
				string id = catalog.InternalIds[entry.InternalId];
				
				object keyObj = catalog.Keys.ElementAt(entry.PrimaryKey).Value;
				if (keyObj is not string key)
					continue;

				string type = catalog.ResourceTypes[entry.ResourceType].ClassName;

				idToOriginalPath[id] = key;

				if (!string.IsNullOrEmpty(Path.GetExtension(key)) && type != "UnityEngine.Sprite")
				{
					string extensionlessPath = key.Substring(0, key.Length - Path.GetExtension(key).Length);

					if (!pathToOccuranceCount.TryGetValue(extensionlessPath, out HashSet<string> occuranceIds))
						occuranceIds = pathToOccuranceCount[extensionlessPath] = new HashSet<string>();
					occuranceIds.Add(id);
				}

				if (!idToEntries.TryGetValue(id, out List<Entry> entries))
				{
					entries = new List<Entry>();
					idToEntries[id] = entries;
				}
				entries.Add(entry);
			}

			void SetDefaultGuid(IUnityObjectBase asset)
			{
				MD5 md5 = MD5.Create();
				byte[] hashArr = md5.ComputeHash(Encoding.UTF8.GetBytes($"{asset.Collection.Name}/{asset.PathID}"));
				GameData.ObjectGuids[asset] = new UnityGuid(hashArr.AsSpan());
			}

			static void SetDefaultPath(IUnityObjectBase asset)
			{
				if (asset.Collection.IsScene)
					asset.OriginalPath = Path.Combine("Assets", "ULTRAKILL Others", "SceneAssets", asset.Collection.Scene.Name, asset.ClassName, FileUtils.FixInvalidNameCharacters($"{asset.TryGetName() ?? asset.ClassName}_{asset.PathID}"));
				else
					asset.OriginalPath = Path.Combine("Assets", "ULTRAKILL Others", asset.ClassName, FileUtils.FixInvalidNameCharacters($"{asset.TryGetName() ?? asset.ClassName}_{asset.PathID}"));
			}

			foreach (var bundle in gameData.GameBundle.Bundles)
			{
				foreach (var collection in bundle.Collections)
				{
					if (collection.IsScene)
					{
						if (idToOriginalPath.TryGetValue(collection.Scene.Name, out string realPath))
						{
							string sceneName = Path.GetFileNameWithoutExtension(realPath);
							collection.Scene.GUID = UnityGuid.Parse(collection.Scene.Name);
							collection.Scene.Name = sceneName;
							collection.Scene.Path = $"Assets/Scenes/{sceneName}";
						}
					}

					foreach (var asset in collection.Assets.Select(pair => pair.Value))
					{
						SetDefaultGuid(asset);

						if (asset.OriginalName != null)
						{
							if (idToOriginalPath.TryGetValue(asset.OriginalName, out string realPath))
							{
								if (isGuid(asset.OriginalName))
									GameData.ObjectGuids[asset] = UnityGuid.Parse(asset.OriginalName);

								if (!string.IsNullOrEmpty(Path.GetExtension(realPath)))
								{
									string extension = Path.GetExtension(realPath);
									string extensionlessPath = realPath.Substring(0, realPath.Length - extension.Length);

									if (pathToOccuranceCount.TryGetValue(extensionlessPath, out HashSet<string> occuranceIds) && occuranceIds.Count > 1)
										realPath = $"{extensionlessPath}_{extension.Substring(1)}{extension}";
								}

								if (!realPath.StartsWith("Assets"))
									realPath = Path.Combine("Assets", realPath);
								realPath = realPath.Substring("Assets".Length + 1);
								realPath = Path.Combine("Assets", "ULTRAKILL Assets", realPath);

								if (idToEntries.TryGetValue(asset.OriginalName, out List<Entry> entries))
								{
									if (entries.Count > 1)
									{
										string mainType = catalog.ResourceTypes[entries[0].ResourceType].ClassName;
										bool isSubAsset = false;

										if (mainType == "UnityEngine.GameObject")
										{
											if (asset.ClassName != "GameObject" && asset.ClassName != "PrefabInstance")
												isSubAsset = true;
										}
										else if (mainType == "UnityEngine.Texture2D")
										{
											if (asset.ClassName != "Texture2D")
												isSubAsset = true;
										}
										else if (mainType == "TMPro.TMP_FontAsset")
										{
											if (asset.ClassName != "MonoBehaviour")
												isSubAsset = true;
										}
										else if (mainType == "UnityEngine.Font")
										{
											if (asset.ClassName != "Font")
												isSubAsset = true;
										}
										else if (mainType == "UnityEngine.Audio.AudioMixer")
										{
											if (asset.ClassName != "AudioMixerController")
												isSubAsset = true;
										}

										if (isSubAsset)
										{
											SetDefaultGuid(asset);
											SetDefaultPath(asset);
											continue;
										}
									}
								}

								asset.OriginalPath = realPath;
							}
							else
							{
								MergePackageAssets.TryMerge(asset);
								SetDefaultPath(asset);
							}
						}
						else
						{
							MergePackageAssets.TryMerge(asset);
							SetDefaultPath(asset);
						}
					}
				}
			}
			
			// Merge assets from BootStrap (Level0)
			foreach (var collection in gameData.GameBundle.Collections)
			{
				foreach (var asset in collection.Assets.Values)
				{
					if (asset is IAudioMixerController audioMixer)
					{
						string audioMixerName = audioMixer.Name;
						Entry originalEntry = catalog.Entries.Where(entry => Path.GetFileNameWithoutExtension((string)(catalog.Keys.ElementAt(entry.PrimaryKey).Value)) == audioMixerName).FirstOrDefault();

						if (originalEntry != null)
						{
							string id = catalog.InternalIds[originalEntry.InternalId];
							GameData.ObjectGuids[asset] = UnityGuid.Parse(id);
							GameData.ObjectsToMerge[asset] = UnityGuid.Parse(id);

							continue;
						}
					}
					else if (asset is IGameObject && asset.MainAsset is PrefabHierarchyObject hierarchy)
					{
						int catalogGameObjectTypeIndex = catalog.ResourceTypes.IndexOf(fullyQualifiedName => fullyQualifiedName.ClassName == "UnityEngine.GameObject");
						string prefabName = hierarchy.Name;

						if (catalogGameObjectTypeIndex != -1)
						{
							Entry originalEntry = catalog.Entries
								.Where(entry => entry.ResourceType == catalogGameObjectTypeIndex)
								.Where(entry => Path.GetFileNameWithoutExtension((string)(catalog.Keys.ElementAt(entry.PrimaryKey).Value)) == prefabName)
								.FirstOrDefault();

							if (originalEntry.InternalId != 0)
							{
								string id = catalog.InternalIds[originalEntry.InternalId];
								GameData.ObjectGuids[asset] = UnityGuid.Parse(id);
								GameData.ObjectsToMerge[asset] = UnityGuid.Parse(id);
								GameData.ObjectGuids[hierarchy] = UnityGuid.Parse(id);
								GameData.ObjectsToMerge[hierarchy] = UnityGuid.Parse(id);
								GameData.ObjectGuids[hierarchy.Prefab] = UnityGuid.Parse(id);
								GameData.ObjectsToMerge[hierarchy.Prefab] = UnityGuid.Parse(id);

								continue;
							}
						}
					}

					SetDefaultPath(asset);
					SetDefaultGuid(asset);
				}
			}
		}
	}
}
