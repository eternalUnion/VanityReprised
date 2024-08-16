using AssetRipper.Assets;
using AssetRipper.PackageGuid;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AssetRipper.Processing
{
	public sealed class MergePackageAssets : IAssetProcessor
	{
		private static PackageGuids _data;
		private static PackageGuids data
		{
			get
			{
				if (_data == null)
				{
					if (!File.Exists("Resources/packageGuids.json"))
						throw new IOException("Required file not found: Resources/packageGuids.json");

					_data = JsonConvert.DeserializeObject<PackageGuids>(File.ReadAllText("Resources/packageGuids.json"));
				}

				return _data;
			}
		}

		public static void TryMerge(IUnityObjectBase asset)
		{
			if (asset is IShader shader)
			{
				ShaderData packageShader = data.Shaders.Where(d => d.ShaderName == shader.Name).FirstOrDefault();
				if (packageShader == null)
					return;

				GameData.ObjectGuids[asset] = UnityGuid.Parse(packageShader.Guid);
				GameData.ObjectsToMerge[asset] = UnityGuid.Parse(packageShader.Guid);
			}
			else if (asset is ITexture2D texture)
			{
				FileData packageTexture = data.Files.Where(d => Path.GetFileNameWithoutExtension(d.FileName) == texture.Name).FirstOrDefault();
				if (packageTexture == null)
					return;

				GameData.ObjectGuids[asset] = UnityGuid.Parse(packageTexture.Guid);
				GameData.ObjectsToMerge[asset] = UnityGuid.Parse(packageTexture.Guid);
			}
			else if (asset is IMaterial material)
			{
				string matFileName = material.Name + ".mat";
				FileData packageMaterial = data.Files.Where(d => d.FileName == matFileName).FirstOrDefault();
				if (packageMaterial == null)
					return;

				GameData.ObjectGuids[asset] = UnityGuid.Parse(packageMaterial.Guid);
				GameData.ObjectsToMerge[asset] = UnityGuid.Parse(packageMaterial.Guid);
			}
		}

		public void Process(GameData gameData)
		{
			foreach (var collection in gameData.GameBundle.Collections)
			{
				foreach (var asset in collection.Assets.Values)
				{
					TryMerge(asset);
				}
			}
		}
	}
}
