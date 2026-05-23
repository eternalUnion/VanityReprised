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
	// Used for accurate shaders
	public static readonly Dictionary<IUnityObjectBase, string> OriginalGuids = new();
	public static readonly IReadOnlyList<string> GlobalShaderKeywords = new List<string>()
	{
		"SPOT",
		"DIRECTIONAL",
		"DIRECTIONAL_COOKIE",
		"POINT",
		"POINT_COOKIE",
		"SHADOWS_DEPTH",
		"SHADOWS_SCREEN",
		"SHADOWS_CUBE",
		"SHADOWS_SOFT",
		"SHADOWS_SPLIT_SPHERES",
		"SHADOWS_SINGLE_CASCADE",
		"LIGHTMAP_ON",
		"DIRLIGHTMAP_COMBINED",
		"DYNAMICLIGHTMAP_ON",
		"LIGHTMAP_SHADOW_MIXING",
		"SHADOWS_SHADOWMASK",
		"LIGHTPROBE_SH",
		"FOG_LINEAR",
		"FOG_EXP",
		"FOG_EXP2",
		"_EMISSION",
		"VERTEXLIGHT_ON",
		"SOFTPARTICLES_ON",
		"UNITY_HDR_ON",
		"LOD_FADE_CROSSFADE",
		"INSTANCING_ON",
		"PROCEDURAL_INSTANCING_ON",
		"DOTS_INSTANCING_ON",
		"UNITY_SINGLE_PASS_STEREO",
		"ETC1_EXTERNAL_ALPHA",
		"STEREO_INSTANCING_ON",
		"STEREO_MULTIVIEW_ON",
		"STEREO_CUBEMAP_RENDER_ON",
		"EDITOR_VISUALIZATION",
		"GEOM_TYPE_LEAF",
		"GEOM_TYPE_FROND",
		"GEOM_TYPE_BRANCH_DETAIL",
		"GEOM_TYPE_BRANCH",
		"GEOM_TYPE_MESH",
		"EFFECT_BUMP",
		"EFFECT_HUE_VARIATION",
		"EFFECT_BILLBOARD",
		"EFFECT_EXTRA_TEX",
		"EFFECT_SUBSURFACE",
		"_WINDQUALITY_NONE",
		"_WINDQUALITY_FASTEST",
		"_WINDQUALITY_FAST",
		"_WINDQUALITY_BETTER",
		"_WINDQUALITY_BEST",
		"_WINDQUALITY_PALM",
		"BILLBOARD_FACE_CAMERA_POS",
		"UNITY_DEVICE_SUPPORTS_WAVE_ANY",
		"UNITY_DEVICE_SUPPORTS_WAVE_8",
		"UNITY_DEVICE_SUPPORTS_WAVE_16",
		"UNITY_DEVICE_SUPPORTS_WAVE_32",
		"UNITY_DEVICE_SUPPORTS_WAVE_64",
		"UNITY_DEVICE_SUPPORTS_WAVE_128",
		"UNITY_DEVICE_SUPPORTS_NATIVE_16BIT",
	};

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
