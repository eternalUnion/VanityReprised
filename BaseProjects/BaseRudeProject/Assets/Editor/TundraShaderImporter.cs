using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TundraShaderImporter : MonoBehaviour
{
	public static IEnumerable<string> GetFilesRecursive(string path)
	{
		foreach (string file in Directory.GetFiles(path))
			yield return file;
		foreach (string folder in Directory.GetDirectories(path))
			foreach (string file in GetFilesRecursive(folder))
				yield return file;
	}

	public static string GUIDfromMetaFile(string filePath)
	{
		string metaFile = File.ReadAllText(filePath);
		int guidIndex = metaFile.IndexOf("guid: ") + "guid: ".Length;
		return guidIndex == -1 ? "" : metaFile.Substring(guidIndex, 32);
	}

	[MenuItem("Tools/Tundra Shader Importer")]
    static void Run()
    {
        if (!EditorUtility.DisplayDialog("Tundra Shader Importer", "Open your tundra assets folder", "Ok", "Cancel"))
            return;

        string tundraAssets = EditorUtility.OpenFolderPanel("Tundra assets folder", Application.dataPath, "Shaders");
        if (!Directory.Exists(tundraAssets))
        {
            EditorUtility.DisplayDialog("Error", "Folder not found!", "Ok");
            return;
        }

		AssetDatabase.StartAssetEditing();
		try
        {
            string libDir = GetFilesRecursive(tundraAssets).Where(path => path.EndsWith("PSX_Core.cginc")).FirstOrDefault();
            if (!string.IsNullOrEmpty(libDir))
            {
                File.Copy(libDir, Path.Combine(Application.dataPath, "ULTRAKILL Addressables", "Shaders", "PSX_Core.cginc"), true);
                if (File.Exists(libDir + ".meta"))
					File.Copy(libDir + ".meta", Path.Combine(Application.dataPath, "ULTRAKILL Addressables", "Shaders", "PSX_Core.cginc.meta"), true);
			}

			foreach (string shaderMeta in GetFilesRecursive(tundraAssets).Where(path => path.EndsWith(".shader.meta")))
			{
                EditorUtility.DisplayProgressBar("Importing", $"Processing {shaderMeta}", 0);

				string guid = GUIDfromMetaFile(shaderMeta);
				if (string.IsNullOrEmpty(guid))
					continue;
				string realFilePath = shaderMeta.Substring(0, shaderMeta.Length - ".meta".Length);
				if (!File.Exists(realFilePath))
					continue;

				string localAssetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (string.IsNullOrEmpty(localAssetPath))
					continue;

				Debug.Log($"Found corresponding shader {Path.GetFileNameWithoutExtension(realFilePath)}");

                string fullLocalAssetsPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), localAssetPath);
                File.Copy(realFilePath, fullLocalAssetsPath, true);
			}
		}
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
