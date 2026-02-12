using AssetRipper.Assets;
using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Structure;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Platforms;

namespace AssetRipper.Processing;

public record GameData(
	GameBundle GameBundle,
	UnityVersion ProjectVersion,
	IAssemblyManager AssemblyManager,
	PlatformGameStructure? PlatformStructure)
{
	public static readonly Dictionary<IUnityObjectBase, UnityGuid> ObjectGuids = new Dictionary<IUnityObjectBase, UnityGuid>();
	public static readonly Dictionary<IUnityObjectBase, UnityGuid> ObjectsToMerge = new Dictionary<IUnityObjectBase, UnityGuid>();
	
	public enum BaseProject
	{
		Rude,
	}
	public static BaseProject ProjectToExport;

	public ProcessedAssetCollection AddNewProcessedCollection(string name)
	{
		return GameBundle.AddNewProcessedCollection(name, ProjectVersion);
	}

	public static GameData FromGameStructure(GameStructure gameStructure)
	{
		return new(gameStructure.FileCollection, gameStructure.FileCollection.GetMaxUnityVersion(), gameStructure.AssemblyManager, gameStructure.PlatformStructure);
	}
}
