using System;
using System.Collections.Generic;

namespace AssetRipper.PackageGuid
{
	public class FileData
	{
		public string FileName { get; set; }
		public string Guid { get; set; }
	}

	public class ShaderData
	{
		public string ShaderName { get; set; }
		public string Guid { get; set; }
	}

	public class ScriptData
	{
		public string Namespace { get; set; }
		public string ClassName { get; set; }
		public string Guid { get; set; }
	}

	public class PackageGuids
	{
		public List<FileData> Files;
		public List<ShaderData> Shaders;
		public List<ScriptData> Scripts;
	}
}
