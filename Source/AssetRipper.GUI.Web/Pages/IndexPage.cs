using AssetRipper.AccurateShaders;
using AssetRipper.GUI.Web.Paths;
using AssetRipper.Import.Configuration;
using AssetRipper.IO.Files.Utils;
using AssetRipper.Processing.Configuration;
using AssetRipper.Text.Html;
using System.Security.AccessControl;
using System.Security.Principal;

namespace AssetRipper.GUI.Web.Pages;

public sealed class IndexPage : DefaultPage
{
	public static IndexPage Instance { get; } = new();

	public override string? GetTitle() => GameFileLoader.Premium ? Localization.AssetRipperPremium : Localization.AssetRipperFree;

	private int id_counter = 0;
	private record FormInfo(string id, string url, string method);
	private List<FormInfo> forms = new();

	private void WriteGetLink(TextWriter writer, string url, string name, string? @class = null)
	{
		new Input(writer).WithForm($"form_{id_counter}").WithType("submit").WithClass(@class).WithValue(name.ToHtml()).Close();
		forms.Add(new FormInfo($"form_{id_counter++}", url, "get"));
	}

	private void WritePostLink(TextWriter writer, string url, string name, string? @class = null, bool enabled = true)
	{
		new Input(writer).WithCustomAttribute(enabled ? "" : "disabled").WithForm($"form_{id_counter}").WithType("submit").WithClass(@class).WithValue(name.ToHtml()).Close();
		forms.Add(new FormInfo($"form_{id_counter++}", url, "post"));
	}

	private static void WriteCheckBox(TextWriter writer, string label, bool @checked, string id, bool enabled = true, string? extraClass = null)
	{
		using (new Div(writer).End())
		{
			string classText = (string.IsNullOrEmpty(extraClass)) ? "m-1" : "m-1 " + extraClass;

			if (enabled)
				new Input(writer).WithClass(classText).WithType("checkbox").WithValue().WithId(id).WithName(id).MaybeWithChecked(@checked).Close();
			else
				new Input(writer).WithClass(classText).WithType("checkbox").WithValue().WithId(id).WithName(id).MaybeWithChecked(@checked).WithCustomAttribute("disabled").Close();

			using (new Label(writer).WithClass("form-check-label").WithFor(id).End())
				writer.Write(label);
		}
	}

	public override void WriteInnerContent(TextWriter writer)
	{
		id_counter = 0;
		forms.Clear();

		writer.Write("""
				<script>
				function select_all(layer) {
					document.querySelectorAll("." + layer).forEach(e => e.checked = true)
				}

				function deselect_all(layer) {
					document.querySelectorAll("." + layer).forEach(e => e.checked = false)
				}
				</script>
				""");

		using (new Div(writer).WithClass("text-center container mt-5").End())
		{
			new H1(writer).WithClass("display-4 mb-4").Close("Vanity Reprised");

			using (new Div(writer).WithClass("d-flex justify-content-center mt-3").End())
			{
				WriteGetLink(writer, "/AccurateShaderInfo", "What is \"accurate shaders\"?", "btn btn-primary m-1");
				WriteUninstallAccurateShaderButton(writer);
			}

			using (new Form(writer).WithAction(GameFileLoader.IsLoaded ? "/ExportRude" : "/LoadFolder").WithMethod("post").End())
			{
				WriteAccurateShader(writer, AccurateShaderDefinition.AccurateShaders_2022_3_28f1, nameof(AccurateShaderDefinition.AccurateShaders_2022_3_28f1));
				WriteAccurateShader(writer, AccurateShaderDefinition.AccurateShaders_2022_3_29f1, nameof(AccurateShaderDefinition.AccurateShaders_2022_3_29f1));
				new Div(writer).WithClass("mt-4").Close();

				if (GameFileLoader.IsLoaded)
				{
					using (new Div(writer).WithClass("d-flex justify-content-center").End())
					{
						new Button(writer).WithClass("btn btn-success m-1").WithType("submit").Close("Generate RUDE project");
						WritePostLink(writer, "/Reset", Localization.MenuFileReset, "btn btn-danger m-1");
					}
				}
				else
				{
					using (new Div(writer).WithClass("text-left mt-2").End())
					{
						new Button(writer).WithClass("btn btn-primary m-1").WithType("submit").Close("Open ULTRAKILL folder");
					}
				}

				WriteSceneExport(writer, !GameFileLoader.IsLoaded);
			}
			
			new P(writer).WithClass("mt-4").Close("Donate for Asset Ripper, the original project:");
			using (new Div(writer).WithClass("d-flex justify-content-center mt-3").End())
			{
				new A(writer).WithClass("btn btn-danger m-1").WithNewTabAttributes().WithHref("https://patreon.com/ds5678").Close("Patreon");
				new A(writer).WithClass("btn btn-danger m-1").WithNewTabAttributes().WithHref("https://paypal.me/ds5678").Close("Paypal");
				new A(writer).WithClass("btn btn-danger m-1").WithNewTabAttributes().WithHref("https://github.com/sponsors/ds5678").Close("GitHub Sponsors");
			}
		}

		foreach (FormInfo formInfo in forms)
		{
			new Form(writer).WithId(formInfo.id).WithAction(formInfo.url).WithMethod(formInfo.method).Close();
		}
	}

	private void WriteUninstallAccurateShaderButton(TextWriter writer)
	{
		bool enabled = false;

		AccurateShaderDefinition def;
		
		def = AccurateShaderDefinition.AccurateShaders_2022_3_28f1;
		if (def.EditorInstalled && def.CanModify && def.AlreadyInstalled)
			enabled = true;

		def = AccurateShaderDefinition.AccurateShaders_2022_3_29f1;
		if (def.EditorInstalled && def.CanModify && def.AlreadyInstalled)
			enabled = true;

		WritePostLink(writer, "/UninstallAccurateShader", "Uninstall accurate shaders", "btn btn-danger m-1", enabled: enabled);
	}

	private void WriteAccurateShader(TextWriter writer, AccurateShaderDefinition definition, string id)
	{
		using (new Div(writer).WithClass("d-flex justify-content-center").End())
		{
			bool enabled = true;
			string text = $"Install accurate shaders for {definition.Editor}";

			if (!definition.EditorInstalled)
			{
				enabled = false;
				text = $"<s>{text}</s> (UNITY EDITOR NOT INSTALLED)";
			}
			else if (!definition.CanModify)
			{
				enabled = false;

#if OS_LINUX
				text = $"<s>{text}</s> (MUST RUN WITH SUDO)";
#else
				text = $"<s>{text}</s> (MUST RUN AS ADMINISTRATOR)";
#endif
			}

			WriteCheckBox(writer, text, definition.Export, id, enabled);
		}
	}

	private void WriteSceneExport(TextWriter writer, bool enabled)
	{
		new H1(writer).WithCustomAttribute("align", "left").Close(enabled ? "Scenes To Export" : "Scenes To Export (Cannot edit after import)");

		using (new U(writer).End())
			new H2(writer).WithCustomAttribute("align", "left").Close("Main Levels");

		using (new Div(writer).WithStyle("display: grid; grid-gap: 10px; justify-items: start; align-items: center; grid-template-columns: max-content max-content max-content max-content max-content max-content max-content max-content max-content max-content;").End())
		{
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer0')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer0')").Close("Deselect All");
			new B(writer).Close("Prelude");
			WriteCheckBox(writer, "0-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_1, nameof(ImportSettings.Export_campaign_scenes_level0_1), enabled, "layer0");
			WriteCheckBox(writer, "0-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_2, nameof(ImportSettings.Export_campaign_scenes_level0_2), enabled, "layer0");
			WriteCheckBox(writer, "0-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_3, nameof(ImportSettings.Export_campaign_scenes_level0_3), enabled, "layer0");
			WriteCheckBox(writer, "0-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_4, nameof(ImportSettings.Export_campaign_scenes_level0_4), enabled, "layer0");
			WriteCheckBox(writer, "0-5", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_5, nameof(ImportSettings.Export_campaign_scenes_level0_5), enabled, "layer0");
			WriteCheckBox(writer, "0-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_s, nameof(ImportSettings.Export_campaign_scenes_level0_s), enabled, "layer0");
			WriteCheckBox(writer, "0-E", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_e, nameof(ImportSettings.Export_campaign_scenes_level0_e), enabled, "layer0");

			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer1')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer1')").Close("Deselect All");
			new B(writer).Close("Layer 1 - Limbo");
			WriteCheckBox(writer, "1-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_1, nameof(ImportSettings.Export_campaign_scenes_level1_1), enabled, "layer1");
			WriteCheckBox(writer, "1-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_2, nameof(ImportSettings.Export_campaign_scenes_level1_2), enabled, "layer1");
			WriteCheckBox(writer, "1-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_3, nameof(ImportSettings.Export_campaign_scenes_level1_3), enabled, "layer1");
			WriteCheckBox(writer, "1-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_4, nameof(ImportSettings.Export_campaign_scenes_level1_4), enabled, "layer1");
			writer.Write("<p></p>");
			WriteCheckBox(writer, "1-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_s, nameof(ImportSettings.Export_campaign_scenes_level1_s), enabled, "layer1");
			WriteCheckBox(writer, "1-E", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_e, nameof(ImportSettings.Export_campaign_scenes_level1_e), enabled, "layer1");

			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer2')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer2')").Close("Deselect All");
			new B(writer).Close("Layer 2 - Lust");
			WriteCheckBox(writer, "2-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_1, nameof(ImportSettings.Export_campaign_scenes_level2_1), enabled, "layer2");
			WriteCheckBox(writer, "2-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_2, nameof(ImportSettings.Export_campaign_scenes_level2_2), enabled, "layer2");
			WriteCheckBox(writer, "2-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_3, nameof(ImportSettings.Export_campaign_scenes_level2_3), enabled, "layer2");
			WriteCheckBox(writer, "2-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_4, nameof(ImportSettings.Export_campaign_scenes_level2_4), enabled, "layer2");
			writer.Write("<p></p>");
			WriteCheckBox(writer, "2-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_s, nameof(ImportSettings.Export_campaign_scenes_level2_s), enabled, "layer2");
			writer.Write("<p></p>");

			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer3')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer3')").Close("Deselect All");
			new B(writer).Close("Layer 3 - Gluttony");
			WriteCheckBox(writer, "3-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level3_1, nameof(ImportSettings.Export_campaign_scenes_level3_1), enabled, "layer3");
			WriteCheckBox(writer, "3-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level3_2, nameof(ImportSettings.Export_campaign_scenes_level3_2), enabled, "layer3");
			writer.Write("<p></p>");
			writer.Write("<p></p>");
			writer.Write("<p></p>");
			WriteCheckBox(writer, "P-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_levelp_1, nameof(ImportSettings.Export_campaign_scenes_levelp_1), enabled, "layerp");
			writer.Write("<p></p>");

			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer4')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer4')").Close("Deselect All");
			new B(writer).Close("Layer 4 - Greed");
			WriteCheckBox(writer, "4-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_1, nameof(ImportSettings.Export_campaign_scenes_level4_1), enabled, "layer4");
			WriteCheckBox(writer, "4-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_2, nameof(ImportSettings.Export_campaign_scenes_level4_2), enabled, "layer4");
			WriteCheckBox(writer, "4-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_3, nameof(ImportSettings.Export_campaign_scenes_level4_3), enabled, "layer4");
			WriteCheckBox(writer, "4-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_4, nameof(ImportSettings.Export_campaign_scenes_level4_4), enabled, "layer4");
			writer.Write("<p></p>");
			WriteCheckBox(writer, "4-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_s, nameof(ImportSettings.Export_campaign_scenes_level4_s), enabled, "layer4");
			writer.Write("<p></p>");

			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer5')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer5')").Close("Deselect All");
			new B(writer).Close("Layer 5 - Wrath");
			WriteCheckBox(writer, "5-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_1, nameof(ImportSettings.Export_campaign_scenes_level5_1), enabled, "layer5");
			WriteCheckBox(writer, "5-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_2, nameof(ImportSettings.Export_campaign_scenes_level5_2), enabled, "layer5");
			WriteCheckBox(writer, "5-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_3, nameof(ImportSettings.Export_campaign_scenes_level5_3), enabled, "layer5");
			WriteCheckBox(writer, "5-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_4, nameof(ImportSettings.Export_campaign_scenes_level5_4), enabled, "layer5");
			writer.Write("<p></p>");
			WriteCheckBox(writer, "5-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_s, nameof(ImportSettings.Export_campaign_scenes_level5_s), enabled, "layer5");
			writer.Write("<p></p>");

			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer6')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer6')").Close("Deselect All");
			new B(writer).Close("Layer 6 - Heresy");
			WriteCheckBox(writer, "6-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level6_1, nameof(ImportSettings.Export_campaign_scenes_level6_1), enabled, "layer6");
			WriteCheckBox(writer, "6-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level6_2, nameof(ImportSettings.Export_campaign_scenes_level6_2), enabled, "layer6");
			writer.Write("<p></p>");
			writer.Write("<p></p>");
			writer.Write("<p></p>");
			WriteCheckBox(writer, "P-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_levelp_2, nameof(ImportSettings.Export_campaign_scenes_levelp_2), enabled, "layerp");
			writer.Write("<p></p>");

			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer7')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer7')").Close("Deselect All");
			new B(writer).Close("Layer 7 - Violence");
			WriteCheckBox(writer, "7-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_1, nameof(ImportSettings.Export_campaign_scenes_level7_1), enabled, "layer7");
			WriteCheckBox(writer, "7-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_2, nameof(ImportSettings.Export_campaign_scenes_level7_2), enabled, "layer7");
			WriteCheckBox(writer, "7-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_3, nameof(ImportSettings.Export_campaign_scenes_level7_3), enabled, "layer7");
			WriteCheckBox(writer, "7-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_4, nameof(ImportSettings.Export_campaign_scenes_level7_4), enabled, "layer7");
			writer.Write("<p></p>");
			WriteCheckBox(writer, "7-S", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_s, nameof(ImportSettings.Export_campaign_scenes_level7_s), enabled, "layer7");
			writer.Write("<p></p>");

			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-success m-1").WithCustomAttribute("onclick", "select_all('layer8')").Close("Select All");
			new Button(writer).WithType("button").WithCustomAttribute(enabled ? "" : "disabled").WithClass("btn btn-danger m-1").WithCustomAttribute("onclick", "deselect_all('layer8')").Close("Deselect All");
			new B(writer).Close("Layer 8 - Fraud");
			WriteCheckBox(writer, "8-1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level8_1, nameof(ImportSettings.Export_campaign_scenes_level8_1), enabled, "layer8");
			WriteCheckBox(writer, "8-2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level8_2, nameof(ImportSettings.Export_campaign_scenes_level8_2), enabled, "layer8");
			WriteCheckBox(writer, "8-3", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level8_3, nameof(ImportSettings.Export_campaign_scenes_level8_3), enabled, "layer8");
			WriteCheckBox(writer, "8-4", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level8_4, nameof(ImportSettings.Export_campaign_scenes_level8_4), enabled, "layer8");
			writer.Write("<p></p>");
			writer.Write("<p></p>");
			writer.Write("<p></p>");
		}

		using (new U(writer).End())
			new H2(writer).WithCustomAttribute("align", "left").Close("Special Scenes");

		using (new Div(writer).WithStyle("display: grid; grid-gap: 10px; grid-template-columns: max-content; justify-items: start;").End())
		{
			WriteCheckBox(writer, "Intro", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_intro, nameof(ImportSettings.Export_specialscenes_scenes_intro), enabled);
			WriteCheckBox(writer, "Tutorial", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_tutorial, nameof(ImportSettings.Export_specialscenes_scenes_tutorial), enabled);
			WriteCheckBox(writer, "Intermission 1", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_intermission1, nameof(ImportSettings.Export_campaign_scenes_intermission1), enabled);
			WriteCheckBox(writer, "Intermission 2", GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_intermission2, nameof(ImportSettings.Export_campaign_scenes_intermission2), enabled);
			WriteCheckBox(writer, "Main menu", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_mainmenu, nameof(ImportSettings.Export_specialscenes_scenes_mainmenu), enabled);
			WriteCheckBox(writer, "Credits museum", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_creditsmuseum2, nameof(ImportSettings.Export_specialscenes_scenes_creditsmuseum2), enabled);
			WriteCheckBox(writer, "Cybergrind", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_endless, nameof(ImportSettings.Export_specialscenes_scenes_endless), enabled);
			WriteCheckBox(writer, "Sandbox", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_uk_construct, nameof(ImportSettings.Export_specialscenes_scenes_uk_construct), enabled);
			WriteCheckBox(writer, "Early access end", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_earlyaccessend, nameof(ImportSettings.Export_specialscenes_scenes_earlyaccessend), enabled);
			WriteCheckBox(writer, "Tundra assets", GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_tundraassets, nameof(ImportSettings.Export_specialscenes_scenes_tundraassets), enabled);
		}
	}
}
