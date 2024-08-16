using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Processing
{
	public sealed class RemoveAssetBundleNames : IAssetProcessor
	{
		public void Process(GameData gameData)
		{
			foreach (var asset in gameData.GameBundle.FetchAssets())
			{
				asset.AssetBundleName = "";
			}
		}
	}
}
