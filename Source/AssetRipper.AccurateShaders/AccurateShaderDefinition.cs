using System;
using System.Security.Cryptography;

namespace AssetRipper.AccurateShaders
{
	public class AccurateShaderDefinition
	{
#if OS_LINUX
		public static AccurateShaderDefinition AccurateShaders_2022_3_28f1 = new("2022.3.28f1", TODO);
		public static AccurateShaderDefinition AccurateShaders_2022_3_29f1 = new("2022.3.29f1", TODO);
#else
		public static AccurateShaderDefinition AccurateShaders_2022_3_28f1 = new("2022.3.28f1", "98de53ed3af19bfd719a667bef648c44");
		public static AccurateShaderDefinition AccurateShaders_2022_3_29f1 = new("2022.3.29f1", "6e5b57f3a8a025dd0b4055c022f748f8");
#endif

		public static string GetMD5(string filePath)
		{
			MD5 md5 = MD5.Create();
			using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return BitConverter.ToString(md5.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
			}
		}

		public static bool AtLeastOneAlreadyInstalled
		{
			get => AccurateShaders_2022_3_28f1.AlreadyInstalled || AccurateShaders_2022_3_29f1.AlreadyInstalled;
		}

		public static bool AtLeastOneExporting
		{
			get => AccurateShaders_2022_3_28f1.Export || AccurateShaders_2022_3_29f1.Export;
		}

		public bool Export = false;

		/// <summary>
		/// Unity Editor version
		/// </summary>
		public string Editor { get; private set; }

		/// <summary>
		/// MD5 hash of the original shader compiler executable
		/// </summary>
		public string ShaderCompilerMD5 { get; private set; }

		private string? _editorPath = null;
		/// <summary>
		/// Directory of the Unity Editor
		/// </summary>
		public string EditorPath
		{
			get
			{
				if (_editorPath != null)
					return _editorPath;

				string configPath = $"{Editor.Replace('.', '_')}_path.txt";
				if (File.Exists(configPath))
				{
					_editorPath = File.ReadAllText(configPath).Trim();
				}
				else
				{
#if OS_LINUX
					throw new Exception("NOT IMPLEMENTED");
#else
					_editorPath = @$"C:\Program Files\Unity\Hub\Editor\{Editor}";
#endif
				}
				
				return _editorPath;
			}
		}

		/// <summary>
		/// Path to the tools folder of the Editor
		/// </summary>
#if OS_LINUX
		public string ToolsPath => Path.Combine(EditorPath, "Editor", "Data", "Tools");
#else
		public string ToolsPath => Path.Combine(EditorPath, "Editor", "Data", "Tools");
#endif

		/// <summary>
		/// Path to the UnityShaderCompiler executable
		/// </summary>
#if OS_LINUX
		public string ShaderCompilerPath => Path.Combine(EditorPath, "Editor", "Data", "Tools", "UnityShaderCompiler");
#else
		public string ShaderCompilerPath => Path.Combine(EditorPath, "Editor", "Data", "Tools", "UnityShaderCompiler.exe");
#endif

		/// <summary>
		/// Returns true if the Unity Editor is locally installed
		/// </summary>
		public bool EditorInstalled => Directory.Exists(EditorPath);

		private bool _triedToModify;
		private bool _canModify;
		/// <summary>
		/// Returns true if the user can modify the Unity Editor installation
		/// </summary>
		public bool CanModify
		{
			get
			{
				if (_triedToModify)
					return _canModify;

				if (!Directory.Exists(ToolsPath))
				{
					_triedToModify = true;
					_canModify = false;
					return _canModify;
				}

				_triedToModify = true;
				_canModify = true;

				string tempFileName = "temp.txt";
				int tries = 0;
				while (File.Exists(Path.Combine(ToolsPath, tempFileName)) && tries < 1000)
				{
					tempFileName = $"temp_{tries++}.txt";
				}

				if (tries >= 1000)
				{
					_canModify = false;
					return _canModify;
				}

				try
				{
					_canModify = true;

					using (FileStream fs = File.Create(Path.Combine(ToolsPath, tempFileName)))
						fs.Close();

					File.Delete(Path.Combine(ToolsPath, tempFileName));
				}
				catch (Exception)
				{
					_canModify = false;
				}

				return _canModify;
			}
		}

		/// <summary>
		/// Path to the UnityShaderCompiler executable
		/// </summary>
#if OS_LINUX
		public bool AlreadyInstalled => File.Exists(Path.Combine(EditorPath, "Editor", "Data", "Tools", "_UnityShaderCompiler"));
#else
		public bool AlreadyInstalled => File.Exists(Path.Combine(EditorPath, "Editor", "Data", "Tools", "_UnityShaderCompiler.exe"));
#endif

		private AccurateShaderDefinition(string editor, string shaderCompilerMD5)
		{
			Editor = editor;
			ShaderCompilerMD5 = shaderCompilerMD5;
		}
	}
}
