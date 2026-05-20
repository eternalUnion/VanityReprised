using AssetRipper.GUI.Web.Paths;
using AssetRipper.Import.Configuration;
using AssetRipper.IO.Files.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.GUI.Web.Pages
{
	internal class AccurateShaderInfoPage : DefaultPage
	{
		public static AccurateShaderInfoPage Instance { get; } = new();

		public override string? GetTitle() => GameFileLoader.Premium ? Localization.AssetRipperPremium : Localization.AssetRipperFree;

		public override void WriteInnerContent(TextWriter writer)
		{
			new H1(writer).Close("Accurate Shaders");

			new P(writer).Close("""
				Normally, graphics of Rude on the Unity Editor does not match to Ultrakill.
				This is caused by the shader GPU code being extremely difficult to decompile.
				"Accurate shaders" is a modification to the Unity Editor code that injects
				Ultrakill's binary shader code directly into the editor, bypassing shader compilation
				directly. Since the original shader code is used, graphics of Rude is accurate.
				""");

			using (new Table(writer).End())
			{
				using (new Tr(writer).End())
				{
					using (new Td(writer).WithCustomAttribute("align", "center").End())
						new B(writer).Close("Before");

					using (new Td(writer).WithCustomAttribute("align", "center").End())
						new B(writer).Close("After");
				}

				using (new Tr(writer).End())
				{
					using (new Td(writer).End())
						new Img(writer).WithStyle("width: 100%").WithCustomAttribute("src", "before1.png").Close();

					using (new Td(writer).End())
						new Img(writer).WithStyle("width: 100%;").WithCustomAttribute("src", "after1.png").Close();
				}

				using (new Tr(writer).End())
				{
					using (new Td(writer).End())
						new Img(writer).WithStyle("width: 100%;").WithCustomAttribute("src", "before2.png").Close();

					using (new Td(writer).End())
						new Img(writer).WithStyle("width: 100%;").WithCustomAttribute("src", "after2.png").Close();
				}
			}

			////////////////////////////////////////////////////
			
			new H1(writer).Close("Pre-Requriements");

			using (new Ul(writer).End())
			{
				new Li(writer).Close("You must have the Unity Editor already installed. As of now, Rude uses\nUnity Editor 2022.3.28f1 (2022.3.29f1 can be also used).");
				new Li(writer).Close("All Unity Editor applications must be closed during the process (it is fine for Unity Hub to be running)");
				new Li(writer).Close("Vanity Reprised must be run with elevated privileges to modify Unity Editor files");
			}

			/////////////////////////////////////////////////////
			
			new H1(writer).Close("What is modified?");

#if OS_LINUX
			new P(writer).Close("""
				The only modification to the Unity Editor is renaming "<UnityEditorPath>/Editor/Data/Tools/UnityShaderCompiler"
				to "_UnityShaderCompiler", and replacing it with
				a custom compiler instead. If the editor is not located at this path, you can overwrite the path
				by modifying the text file located in the same folder as the executable.
				""");
#else
			new P(writer).Close("""
				The only modification to the Unity Editor is renaming "C:\Program Files\Unity\Hub\Editor\<editor version>\Editor\Data\Tools\UnityShaderCompiler.exe"
				to "_UnityShaderCompiler.exe", and replacing it with a custom compiler instead. If the editor is not located at this path, you can overwrite the path
				by modifying the text file located in the same folder as the executable.
				""");

			new P(writer).Close("""
				Alongside the modification to the engine, shader code is also processed and stored inside the generated project's root directory.
				For this reason, an old project that was generated without accurate shaders cannot view them.
				""");
#endif
			/////////////////////////////////////////////////////

			new H1(writer).Close("How do I undo the modifications?");

			new P(writer).Close("You can use Vanity Reprise to undo the modifications done on the home page. It is also possible to simply rename the original file to its original name manually.");
		}
	}
}
