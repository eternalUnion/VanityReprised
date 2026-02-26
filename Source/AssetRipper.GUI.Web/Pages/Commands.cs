using AssetRipper.Import.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AssetRipper.GUI.Web.Pages;

public static class Commands
{
	private const string RootPath = "/";
	private const string CommandsPath = "/Commands";

	public readonly struct LoadFile : ICommand
	{
		static async Task<string?> ICommand.Execute(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			string[]? paths;
			if (form.TryGetValue("Path", out StringValues values))
			{
				paths = values;
			}
			else if (Dialogs.Supported)
			{
				Dialogs.OpenFiles.GetUserInput(out paths);
			}
			else
			{
				return CommandsPath;
			}

			if (paths is { Length: > 0 })
			{
				GameFileLoader.LoadAndProcess(paths);
			}
			return null;
		}
	}

	public readonly struct LoadFolder : ICommand
	{
		static async Task<string?> ICommand.Execute(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_intermission1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_intermission1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_intermission2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_intermission2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level0_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level0_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_3 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level0_3));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_4 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level0_4));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_5 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level0_5));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_s = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level0_s));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level0_e = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level0_e));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level1_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level1_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_3 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level1_3));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_4 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level1_4));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_s = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level1_s));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level1_e = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level1_e));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level2_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level2_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_3 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level2_3));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_4 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level2_4));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level2_s = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level2_s));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level3_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level3_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level3_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level3_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level4_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level4_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_3 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level4_3));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_4 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level4_4));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level4_s = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level4_s));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level5_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level5_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_3 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level5_3));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_4 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level5_4));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level5_s = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level5_s));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level6_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level6_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level6_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level6_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level7_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level7_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_3 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level7_3));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_4 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level7_4));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level7_s = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level7_s));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level8_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level8_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level8_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level8_2));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level8_3 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level8_3));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_level8_4 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_level8_4));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_levelp_1 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_levelp_1));
			GameFileLoader.Settings.ImportSettings.Export_campaign_scenes_levelp_2 = form.ContainsKey(nameof(ImportSettings.Export_campaign_scenes_levelp_2));
			GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_creditsmuseum2 = form.ContainsKey(nameof(ImportSettings.Export_specialscenes_scenes_creditsmuseum2));
			GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_earlyaccessend = form.ContainsKey(nameof(ImportSettings.Export_specialscenes_scenes_earlyaccessend));
			GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_endless = form.ContainsKey(nameof(ImportSettings.Export_specialscenes_scenes_endless));
			GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_intro = form.ContainsKey(nameof(ImportSettings.Export_specialscenes_scenes_intro));
			GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_mainmenu = form.ContainsKey(nameof(ImportSettings.Export_specialscenes_scenes_mainmenu));
			GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_tundraassets = form.ContainsKey(nameof(ImportSettings.Export_specialscenes_scenes_tundraassets));
			GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_tutorial = form.ContainsKey(nameof(ImportSettings.Export_specialscenes_scenes_tutorial));
			GameFileLoader.Settings.ImportSettings.Export_specialscenes_scenes_uk_construct = form.ContainsKey(nameof(ImportSettings.Export_specialscenes_scenes_uk_construct));

			string? path;
			if (form.TryGetValue("Path", out StringValues values))
			{
				path = values;
			}
			else if (Dialogs.Supported)
			{
				Dialogs.OpenFolder.GetUserInput(out path);
			}
			else
			{
				return CommandsPath;
			}

			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.LoadAndProcess([path]);
			}
			return null;
		}
	}

	public readonly struct ExportUnityProject : ICommand
	{
		static async Task<string?> ICommand.Execute(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			string? path;
			if (form.TryGetValue("Path", out StringValues values))
			{
				path = values;
			}
			else
			{
				return CommandsPath;
			}
			
			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.ExportUnityProject(path);
			}
			return null;
		}
	}

	public readonly struct ExportPrimaryContent : ICommand
	{
		static async Task<string?> ICommand.Execute(HttpRequest request)
		{
			IFormCollection form = await request.ReadFormAsync();

			string? path;
			if (form.TryGetValue("Path", out StringValues values))
			{
				path = values;
			}
			else
			{
				return CommandsPath;
			}

			if (!string.IsNullOrEmpty(path))
			{
				GameFileLoader.ExportPrimaryContent(path);
			}
			return null;
		}
	}

	public readonly struct Reset : ICommand
	{
		static Task<string?> ICommand.Execute(HttpRequest request)
		{
			GameFileLoader.Reset();
			return Task.FromResult<string?>(null);
		}
	}

	public static async Task HandleCommand<T>(HttpContext context) where T : ICommand
	{
		string? redirectionTarget = await T.Execute(context.Request);
		context.Response.Redirect(redirectionTarget ?? RootPath);
	}
}
