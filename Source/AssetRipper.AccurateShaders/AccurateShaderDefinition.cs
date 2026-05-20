using System;

namespace AssetRipper.AccurateShaders
{
	public class AccurateShaderDefinition
	{
		public static AccurateShaderDefinition AccurateShaders_2022_3_28f1 = new("2022.3.28f1");
		public static AccurateShaderDefinition AccurateShaders_2022_3_29f1 = new("2022.3.29f1");

		public bool Export = false;

		/// <summary>
		/// Unity Editor version
		/// </summary>
		public string Editor { get; private set; }

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

		private AccurateShaderDefinition(string editor)
		{
			Editor = editor;
		}
	}
}
