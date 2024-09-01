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
				WriteCheckBox(writer, "Export campaign scenes", GameFileLoader.Settings.ImportSettings.EnableCampaignSceneExport, nameof(ImportSettings.EnableCampaignSceneExport), false);
				WriteCheckBox(writer, "Export special scenes", GameFileLoader.Settings.ImportSettings.EnableSpecialSceneExport, nameof(ImportSettings.EnableSpecialSceneExport), false);

				using (new Div(writer).WithClass("d-flex justify-content-center").End())
				{
					WriteGetLink(writer, "/ExportRude", "Generate RUDE project", "btn btn-success m-1");
					WriteGetLink(writer, "/ExportSpite", "Generate SPITE project", "btn btn-success m-1");
					WritePostLink(writer, "/Reset", Localization.MenuFileReset, "btn btn-danger m-1");
				}
			}
			else
			{
				using (new Form(writer).WithClass("text-left mt-2").WithAction("/LoadFolder").WithMethod("post").End())
				{
					WriteCheckBox(writer, "Export campaign scenes", GameFileLoader.Settings.ImportSettings.EnableCampaignSceneExport, nameof(ImportSettings.EnableCampaignSceneExport));
					WriteCheckBox(writer, "Export special scenes", GameFileLoader.Settings.ImportSettings.EnableSpecialSceneExport, nameof(ImportSettings.EnableSpecialSceneExport));

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
}
