using AssetRipper.Import.Logging;
using System.Text.Json.Serialization;

namespace AssetRipper.Import.Configuration;

public sealed record class ImportSettings
{
	/// <summary>
	/// The level of scripts to export
	/// </summary>
	public ScriptContentLevel ScriptContentLevel { get; set; } = ScriptContentLevel.Level1;

	/// <summary>
	/// Including the streaming assets directory can cause some games to fail while exporting.
	/// </summary>
	[JsonIgnore]
	public bool IgnoreStreamingAssets
	{
		get => StreamingAssetsMode == StreamingAssetsMode.Ignore;
		set
		{
			StreamingAssetsMode = value ? StreamingAssetsMode.Ignore : StreamingAssetsMode.Extract;
		}
	}

	// Intermission
	[JsonIgnore] public bool Export_campaign_scenes_intermission1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_intermission2 { get; set; } = false;

	// Layer 0
	[JsonIgnore] public bool Export_campaign_scenes_level0_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level0_2 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level0_3 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level0_4 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level0_5 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level0_s { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level0_e { get; set; } = false;

	// Layer 1
	[JsonIgnore] public bool Export_campaign_scenes_level1_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level1_2 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level1_3 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level1_4 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level1_s { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level1_e { get; set; } = false;

	// Layer 2
	[JsonIgnore] public bool Export_campaign_scenes_level2_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level2_2 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level2_3 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level2_4 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level2_s { get; set; } = false;

	// Layer 3
	[JsonIgnore] public bool Export_campaign_scenes_level3_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level3_2 { get; set; } = false;

	// Layer 4
	[JsonIgnore] public bool Export_campaign_scenes_level4_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level4_2 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level4_3 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level4_4 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level4_s { get; set; } = false;

	// Layer 5
	[JsonIgnore] public bool Export_campaign_scenes_level5_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level5_2 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level5_3 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level5_4 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level5_s { get; set; } = false;

	// Layer 6
	[JsonIgnore] public bool Export_campaign_scenes_level6_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level6_2 { get; set; } = false;

	// Layer 7
	[JsonIgnore] public bool Export_campaign_scenes_level7_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level7_2 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level7_3 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level7_4 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level7_s { get; set; } = false;

	// Layer 8
	[JsonIgnore] public bool Export_campaign_scenes_level8_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level8_2 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level8_3 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_level8_4 { get; set; } = false;

	// Layer P
	[JsonIgnore] public bool Export_campaign_scenes_levelp_1 { get; set; } = false;
	[JsonIgnore] public bool Export_campaign_scenes_levelp_2 { get; set; } = false;

	// Specials
	[JsonIgnore] public bool Export_specialscenes_scenes_creditsmuseum2 { get; set; } = false;
	[JsonIgnore] public bool Export_specialscenes_scenes_earlyaccessend { get; set; } = false;
	[JsonIgnore] public bool Export_specialscenes_scenes_endless { get; set; } = false;
	[JsonIgnore] public bool Export_specialscenes_scenes_intro { get; set; } = false;
	[JsonIgnore] public bool Export_specialscenes_scenes_mainmenu { get; set; } = false;
	[JsonIgnore] public bool Export_specialscenes_scenes_tundraassets { get; set; } = false;
	[JsonIgnore] public bool Export_specialscenes_scenes_tutorial { get; set; } = false;
	[JsonIgnore] public bool Export_specialscenes_scenes_uk_construct { get; set; } = false;

	/// <summary>
	/// How the StreamingAssets folder is handled
	/// </summary>
	public StreamingAssetsMode StreamingAssetsMode { get; set; } = StreamingAssetsMode.Extract;

	/// <summary>
	/// The default version used when no version is specified, ie when the version has been stripped.
	/// </summary>
	public UnityVersion DefaultVersion { get; set; }

	/// <summary>
	/// The target version to convert all assets to. Experimental
	/// </summary>
	public UnityVersion TargetVersion { get; set; }

	public void Log()
	{
		Logger.Info(LogCategory.General, $"{nameof(ScriptContentLevel)}: {ScriptContentLevel}");
		Logger.Info(LogCategory.General, $"{nameof(StreamingAssetsMode)}: {StreamingAssetsMode}");
		Logger.Info(LogCategory.General, $"{nameof(DefaultVersion)}: {DefaultVersion}");
		Logger.Info(LogCategory.General, $"{nameof(TargetVersion)}: {TargetVersion}");
	}
}
