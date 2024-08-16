using AssetRipper.Assets;
using AssetRipper.IO.Files;
using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.Processing;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_468431735;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.MarkerInterfaces;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AssetRipper.Export.UnityProjects.Project
{
	public class PrefabExportCollection : AssetsExportCollection<IPrefabInstance>
	{
		public PrefabExportCollection(IAssetExporter assetExporter, PrefabHierarchyObject prefabHierarchyObject)
			: base(assetExporter, prefabHierarchyObject.Prefab)
		{
			RootGameObject = prefabHierarchyObject.Root;
			Prefab = prefabHierarchyObject.Prefab;
			Hierarchy = prefabHierarchyObject;

			// AddAssets(prefabHierarchyObject.Assets);
			// AddAsset(prefabHierarchyObject);

			static long StringToId(string str)
			{
				return BitConverter.ToInt64(MD5.HashData(Encoding.UTF8.GetBytes(str).AsSpan()).AsSpan(0, 8));
			}

			long hierarchyId = StringToId("HierarchyId");
			m_exportAssetIds.Add(hierarchyId);
			m_exportIDs.Add(prefabHierarchyObject, hierarchyId);

			void SetIdRecursive(IGameObject gameObject, StringBuilder currentPath, int objectNameIndex)
			{
				int originalLength = currentPath.Length;
				currentPath.AppendFormat("/{0}[{1}]", gameObject.Name, objectNameIndex);

				long gameObjectId = StringToId(currentPath.ToString());
				m_exportAssetIds.Add(gameObjectId);
				m_exportIDs.Add(gameObject, gameObjectId);

				Dictionary<string, int> componentTypeToIndex = new Dictionary<string, int>();
				foreach (IComponent comp in gameObject.GetComponentAccessList())
				{
					string name = comp.ClassName;
					if (comp is IMonoBehaviour monoBehaviour && monoBehaviour.ScriptP != null)
						name = monoBehaviour.ScriptP.Name;

					if (!componentTypeToIndex.TryGetValue(name, out int componentIndex))
					{
						componentIndex = 0;
						componentTypeToIndex[name] = 1;
					}
					else
					{
						componentTypeToIndex[name] = componentIndex + 1;
					}

					int preComponentLength = currentPath.Length;
					currentPath.AppendFormat(".{0}[{1}]", name, componentIndex);

					long componentId = StringToId(currentPath.ToString());
					m_exportAssetIds.Add(componentId);
					m_exportIDs.Add(comp, componentId);

					currentPath.Remove(preComponentLength, currentPath.Length - preComponentLength);
				}

				Dictionary<string, int> childNameToIndex = new Dictionary<string, int>();
				foreach (IGameObject child in gameObject.GetTransform().Children_C4P.Select(c => c.GameObject_C4P))
				{
					if (!childNameToIndex.TryGetValue(child.Name, out int childNameIndex))
					{
						childNameIndex = 0;
						childNameToIndex[child.Name] = 1;
					}
					else
					{
						childNameToIndex[child.Name] = childNameIndex + 1;
					}

					SetIdRecursive(child, currentPath, childNameIndex);
				}

				currentPath.Remove(originalLength, currentPath.Length - originalLength);
			}

			SetIdRecursive(prefabHierarchyObject.Root, new StringBuilder(), 0);

			// For debug purposes
			/*IEnumerable<IUnityObjectBase> requiredObjects = prefabHierarchyObject.Assets.Append(prefabHierarchyObject);
			IEnumerable<IUnityObjectBase> currentObjects = m_exportIDs.Select(e => e.Key).Append(Asset);
			List<IUnityObjectBase> missing = requiredObjects.Where(required => !currentObjects.Contains(required)).ToList();
			List<IUnityObjectBase> extra = currentObjects.Where(current => !requiredObjects.Contains(current)).ToList();

			if (missing.Count != 0)
				throw new Exception();
			if (extra.Count != 0)
				;*/
		}

		protected override string GetExportExtension(IUnityObjectBase asset) => PrefabKeyword;

		public override TransferInstructionFlags Flags => base.Flags | TransferInstructionFlags.SerializeForPrefabSystem;
		public IGameObject RootGameObject { get; }
		public IPrefabInstance Prefab { get; }
		public PrefabHierarchyObject Hierarchy { get; }
		/// <summary>
		/// Prior to 2018.3, Prefab was an actual asset inside "*.prefab" files.
		/// After that, PrefabImporter and PrefabInstance were introduced as a replacement.
		/// </summary>
		public bool EmitPrefabAsset => Prefab is IPrefabMarker;
		public override string Name => RootGameObject.Name;

		protected override IUnityObjectBase CreateImporter(IExportContainer container)
		{
			if (EmitPrefabAsset)
			{
				return base.CreateImporter(container);
			}
			else
			{
				IPrefabImporter importer = PrefabImporter.Create(container.File, container.ExportVersion);
				if (RootGameObject.AssetBundleName is not null)
				{
					importer.AssetBundleName_R = RootGameObject.AssetBundleName;
				}
				return importer;
			}
		}

		public override IEnumerable<IUnityObjectBase> ExportableAssets
		{
			get
			{
				if (EmitPrefabAsset)
				{
					foreach (IUnityObjectBase asset in Hierarchy.Assets)
					{
						m_file = asset.Collection;
						yield return asset;
					}
				}
				else
				{
					Debug.Assert(Hierarchy.Assets.Last() is IPrefabInstanceMarker);
					foreach (IUnityObjectBase asset in Hierarchy.Assets.SkipLast(1))//Skip the PrefabInstance asset
					{
						m_file = asset.Collection;
						yield return asset;
					}
				}
			}
		}

		/// <summary>
		/// Used for <see cref="IPrefabInstance.SourcePrefab"/>
		/// </summary>
		/// <returns></returns>
		public MetaPtr GenerateMetaPtrForPrefab()
		{
			return new MetaPtr(
				ExportIdHandler.GetMainExportID((int)ClassIDType.PrefabInstance),
				GUID,
				EmitPrefabAsset ? AssetType.Serialized : AssetType.Meta);
		}

		public const string PrefabKeyword = "prefab";
	}
}
