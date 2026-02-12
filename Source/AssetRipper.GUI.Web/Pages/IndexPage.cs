using AssetRipper.GUI.Web.Paths;
using AssetRipper.Import.Configuration;
using AssetRipper.Processing.Configuration;

namespace AssetRipper.GUI.Web.Pages;

public sealed class IndexPage : DefaultPage
{
	public static IndexPage Instance { get; } = new();

	public override string? GetTitle() => GameFileLoader.Premium ? Localization.AssetRipperPremium : Localization.AssetRipperFree;

	private static void WriteGetLink(TextWriter writer, string url, string name, string? @class = null)
	{
		using (new Form(writer).WithAction(url).WithMethod("get").End())
		{
			new Input(writer).WithType("submit").WithClass(@class).WithValue(name.ToHtml()).Close();
		}
	}

	private static void WritePostLink(TextWriter writer, string url, string name, string? @class = null)
	{
		using (new Form(writer).WithAction(url).WithMethod("post").End())
		{
			new Input(writer).WithType("submit").WithClass(@class).WithValue(name.ToHtml()).Close();
		}
	}

	private static void WriteCheckBox(TextWriter writer, string label, bool @checked, string id, bool enabled = true)
	{
		using (new Div(writer).End())
		{
			if (enabled)
				new Input(writer).WithClass("m-1").WithType("checkbox").WithValue().WithId(id).WithName(id).MaybeWithChecked(@checked).Close();
			else
				new Input(writer).WithClass("m-1").WithType("checkbox").WithValue().WithId(id).WithName(id).MaybeWithChecked(@checked).WithCustomAttribute("disabled").Close();

			new Label(writer).WithClass("form-check-label").WithFor(id).Close(label);
		}
	}

	public override void WriteInnerContent(TextWriter writer)
	{
		using (new Div(writer).WithClass("text-center container mt-5").End())
		{
			new H1(writer).WithClass("display-4 mb-4").Close("Vanity Reprised");

			if (GameFileLoader.IsLoaded)
			{
				WriteSceneExport(writer, false);

				using (new Div(writer).WithClass("d-flex justify-content-center").End())
				{
					WriteGetLink(writer, "/ExportRude", "Generate RUDE project", "btn btn-success m-1");
					WritePostLink(writer, "/Reset", Localization.MenuFileReset, "btn btn-danger m-1");
				}
			}
			else
			{
				using (new Form(writer).WithClass("text-left mt-2").WithAction("/LoadFolder").WithMethod("post").End())
				{
					WriteSceneExport(writer, true);

					new Button(writer).WithClass("btn btn-primary m-1").WithType("submit").Close("Open ULTRAKILL folder");
				}
			}

			new P(writer).WithClass("mt-4").Close("Donate for Asset Ripper, the original project:");
			using (new Div(writer).WithClass("d-flex justify-content-center mt-3").End())
			{
				new A(writer).WithClass("btn btn-danger m-1").WithNewTabAttributes().WithHref("https://patreon.com/ds5678").Close("Patreon");
				new A(writer).WithClass("btn btn-danger m-1").WithNewTabAttributes().WithHref("https://paypal.me/ds5678").Close("Paypal");
				new A(writer).WithClass("btn btn-danger m-1").WithNewTabAttributes().WithHref("https://github.com/sponsors/ds5678").Close("GitHub Sponsors");
			}
		}
	}

	private void WriteSceneExport(TextWriter writer, bool enabled)
	{
		// Campaign scenes
		using (new Details(writer).End())
		{
			new Summary(writer).Close("Campaign scenes");

			// Intermission
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Intermissions");
				WriteCheckBox(writer, "Intermission 1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_intermission1, nameof(ImportSettings.Export_campaign_scenes_intermission1), enabled);
				WriteCheckBox(writer, "Intermission 2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_intermission2, nameof(ImportSettings.Export_campaign_scenes_intermission2), enabled);
			}

			// Layer 0
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Prelude");
				WriteCheckBox(writer, "0-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_1, nameof(ImportSettings.Export_campaign_scenes_level0_1), enabled);
				WriteCheckBox(writer, "0-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_2, nameof(ImportSettings.Export_campaign_scenes_level0_2), enabled);
				WriteCheckBox(writer, "0-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_3, nameof(ImportSettings.Export_campaign_scenes_level0_3), enabled);
				WriteCheckBox(writer, "0-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_4, nameof(ImportSettings.Export_campaign_scenes_level0_4), enabled);
				WriteCheckBox(writer, "0-5", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_5, nameof(ImportSettings.Export_campaign_scenes_level0_5), enabled);
				WriteCheckBox(writer, "0-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_s, nameof(ImportSettings.Export_campaign_scenes_level0_s), enabled);
				WriteCheckBox(writer, "0-E", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_e, nameof(ImportSettings.Export_campaign_scenes_level0_e), enabled);
			}

			// Layer 1
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Layer 1");
				WriteCheckBox(writer, "1-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_1, nameof(ImportSettings.Export_campaign_scenes_level1_1), enabled);
				WriteCheckBox(writer, "1-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_2, nameof(ImportSettings.Export_campaign_scenes_level1_2), enabled);
				WriteCheckBox(writer, "1-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_3, nameof(ImportSettings.Export_campaign_scenes_level1_3), enabled);
				WriteCheckBox(writer, "1-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_4, nameof(ImportSettings.Export_campaign_scenes_level1_4), enabled);
				WriteCheckBox(writer, "1-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_s, nameof(ImportSettings.Export_campaign_scenes_level1_s), enabled);
				WriteCheckBox(writer, "1-E", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_e, nameof(ImportSettings.Export_campaign_scenes_level1_e), enabled);
			}

			// Layer 2
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Layer 2");
				WriteCheckBox(writer, "2-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_1, nameof(ImportSettings.Export_campaign_scenes_level2_1), enabled);
				WriteCheckBox(writer, "2-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_2, nameof(ImportSettings.Export_campaign_scenes_level2_2), enabled);
				WriteCheckBox(writer, "2-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_3, nameof(ImportSettings.Export_campaign_scenes_level2_3), enabled);
				WriteCheckBox(writer, "2-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_4, nameof(ImportSettings.Export_campaign_scenes_level2_4), enabled);
				WriteCheckBox(writer, "2-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_s, nameof(ImportSettings.Export_campaign_scenes_level2_s), enabled);
			}

			// Layer 3
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Layer 3");
				WriteCheckBox(writer, "3-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level3_1, nameof(ImportSettings.Export_campaign_scenes_level3_1), enabled);
				WriteCheckBox(writer, "3-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level3_2, nameof(ImportSettings.Export_campaign_scenes_level3_2), enabled);
			}

			// Layer 4
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Layer 4");
				WriteCheckBox(writer, "4-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_1, nameof(ImportSettings.Export_campaign_scenes_level4_1), enabled);
				WriteCheckBox(writer, "4-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_2, nameof(ImportSettings.Export_campaign_scenes_level4_2), enabled);
				WriteCheckBox(writer, "4-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_3, nameof(ImportSettings.Export_campaign_scenes_level4_3), enabled);
				WriteCheckBox(writer, "4-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_4, nameof(ImportSettings.Export_campaign_scenes_level4_4), enabled);
				WriteCheckBox(writer, "4-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_s, nameof(ImportSettings.Export_campaign_scenes_level4_s), enabled);
			}

			// Layer 5
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Layer 5");
				WriteCheckBox(writer, "5-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_1, nameof(ImportSettings.Export_campaign_scenes_level5_1), enabled);
				WriteCheckBox(writer, "5-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_2, nameof(ImportSettings.Export_campaign_scenes_level5_2), enabled);
				WriteCheckBox(writer, "5-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_3, nameof(ImportSettings.Export_campaign_scenes_level5_3), enabled);
				WriteCheckBox(writer, "5-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_4, nameof(ImportSettings.Export_campaign_scenes_level5_4), enabled);
				WriteCheckBox(writer, "5-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_s, nameof(ImportSettings.Export_campaign_scenes_level5_s), enabled);
			}

			// Layer 6
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Layer 6");
				WriteCheckBox(writer, "6-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level6_1, nameof(ImportSettings.Export_campaign_scenes_level6_1), enabled);
				WriteCheckBox(writer, "6-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level6_2, nameof(ImportSettings.Export_campaign_scenes_level6_2), enabled);
			}

			// Layer 7
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Layer 7");
				WriteCheckBox(writer, "7-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_1, nameof(ImportSettings.Export_campaign_scenes_level7_1), enabled);
				WriteCheckBox(writer, "7-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_2, nameof(ImportSettings.Export_campaign_scenes_level7_2), enabled);
				WriteCheckBox(writer, "7-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_3, nameof(ImportSettings.Export_campaign_scenes_level7_3), enabled);
				WriteCheckBox(writer, "7-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_4, nameof(ImportSettings.Export_campaign_scenes_level7_4), enabled);
				WriteCheckBox(writer, "7-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_s, nameof(ImportSettings.Export_campaign_scenes_level7_s), enabled);
			}

			// Layer P
			using (new Details(writer).End())
			{
				new Summary(writer).Close("Primes");
				WriteCheckBox(writer, "P-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_levelp_1, nameof(ImportSettings.Export_campaign_scenes_levelp_1), enabled);
				WriteCheckBox(writer, "P-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_levelp_2, nameof(ImportSettings.Export_campaign_scenes_levelp_2), enabled);
			}
		}

		// Special scenes
		using (new Details(writer).End())
		{
			new Summary(writer).Close("Special scenes");

			WriteCheckBox(writer, "Intro", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_intro, nameof(ImportSettings.Export_specialscenes_scenes_intro), enabled);
			WriteCheckBox(writer, "Tutorial", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_tutorial, nameof(ImportSettings.Export_specialscenes_scenes_tutorial), enabled);
			WriteCheckBox(writer, "Main menu", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_mainmenu, nameof(ImportSettings.Export_specialscenes_scenes_mainmenu), enabled);
			WriteCheckBox(writer, "Credits museum", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_creditsmuseum2, nameof(ImportSettings.Export_specialscenes_scenes_creditsmuseum2), enabled);
			WriteCheckBox(writer, "Cybergrind", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_endless, nameof(ImportSettings.Export_specialscenes_scenes_endless), enabled);
			WriteCheckBox(writer, "Sandbox", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_uk_construct, nameof(ImportSettings.Export_specialscenes_scenes_uk_construct), enabled);
			WriteCheckBox(writer, "Early access end", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_earlyaccessend, nameof(ImportSettings.Export_specialscenes_scenes_earlyaccessend), enabled);
			WriteCheckBox(writer, "Tundra assets", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_tundraassets, nameof(ImportSettings.Export_specialscenes_scenes_tundraassets), enabled);
		}
	}
}
