using AssetRipper.GUI.Web.Paths;
using AssetRipper.Processing.Configuration;

namespace AssetRipper.GUI.Web.Pages;

public sealed class CommandsPage : VuePage
{
	public static CommandsPage Instance { get; } = new();

	public override string? GetTitle() => Localization.Commands;

	public override void WriteInnerContent(TextWriter writer)
	{
		if (!GameFileLoader.IsLoaded)
		{
			using (new P(writer).End())
			{
				using (new Form(writer).WithAction("/LoadFolder").WithMethod("post").End())
				{
					new Input(writer).WithClass("form-control").WithType("text").WithName("Path")
						.WithCustomAttribute("v-model", "load_path")
						.WithCustomAttribute("@input", "handleLoadPathChange").Close();

					using (new Div(writer).WithClass("d-flex mt-3 justify-content-left").End())
					{
						if (Dialogs.Supported)
						{
							new Button(writer).WithCustomAttribute("@click", "handleSelectLoadFolder").WithClass("btn btn-success m-1").Close(Localization.SelectFolder);
						}

						new Input(writer).WithCustomAttribute("v-if", "load_path_exists").WithType("submit").WithClass("btn btn-primary m-1").WithValue(Localization.MenuLoad).Close();
						new Button(writer).WithCustomAttribute("v-else").WithClass("btn btn-primary m-1").WithCustomAttribute("disabled").Close(Localization.MenuLoad);
					}
				}
			}
		}
		else
		{
			using (new P(writer).End())
			{
				WriteLink(writer, "/Reset", Localization.MenuFileReset, "btn btn-danger");
			}
			using (new P(writer).WithClass("mt-1 d-flex justify-content-left").End())
			{
				using (new Form(writer).End())
				{
					new Input(writer).WithClass("form-control m-1").WithType("text").WithName("Path")
						.WithCustomAttribute("v-model", "export_path")
						.WithCustomAttribute("@input", "handleExportPathChange").Close();
				}
				
				if (Dialogs.Supported)
				{
					new Button(writer).WithCustomAttribute("@click", "handleSelectExportFolder").WithClass("btn btn-success m-1").Close(Localization.SelectFolder);
				}
			}
			using (new P(writer).End())
			{
				using (new Form(writer).WithAction("/Export/UnityProject").WithMethod("post").End())
				{
					new Input(writer).WithType("hidden").WithName("Path").WithCustomAttribute("v-model", "export_path").Close();
					new Button(writer).WithCustomAttribute("v-if", "export_path === ''").WithClass("btn btn-primary").WithCustomAttribute("disabled").Close("Select folder first");
					new Input(writer).WithCustomAttribute("v-else").WithType("submit").WithClass("btn btn-primary").WithValue("Generate project").Close();
				}
			}
		}
	}

	protected override void WriteScriptReferences(TextWriter writer)
	{
		base.WriteScriptReferences(writer);
		new Script(writer).WithSrc("/js/commands_page.js").Close();
	}

	private static void WriteLink(TextWriter writer, string url, string name, string? @class = null)
	{
		using (new Form(writer).WithAction(url).WithMethod("post").End())
		{
			new Input(writer).WithType("submit").WithClass(@class).WithValue(name.ToHtml()).Close();
		}
	}
}
