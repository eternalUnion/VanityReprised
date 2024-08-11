using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.IO.Compression;

public class CreateAssetBundles : EditorWindow
{   
    public static Texture2D levelImage;
    private static string author;
    private static string levelName;

    [MenuItem("SPITE/New Level")]
    static void NewLevel()
    {
        string sourceScenePath = "Assets/SPITE/SCENES/DONOTDELETE.unity";

        string newSceneName = EditorUtility.SaveFilePanel("Name your level", "Assets/", "", "unity");

        if (string.IsNullOrEmpty(newSceneName))
            return;

        FileUtil.CopyFileOrDirectory(sourceScenePath, newSceneName);

        AssetDatabase.Refresh();

        EditorSceneManager.OpenScene(newSceneName);
    }
    
    [MenuItem("SPITE/Compile Level")]
    static void Init()
    {
        GetWindow<CreateAssetBundles>("Create Asset Bundles");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Create Asset Bundles", EditorStyles.boldLabel);

        levelImage = (Texture2D)EditorGUILayout.ObjectField("Level Image", levelImage, typeof(Texture2D), false);

        EditorGUILayout.HelpBox("Please ensure the image is in 4:3 aspect ratio.", MessageType.Info);

        author = EditorGUILayout.TextField("Author", author);
        levelName = EditorGUILayout.TextField("Level Name", levelName);

        if (GUILayout.Button("Create Bundles"))
        {
            CreateBundles();
        }
    }

    static void CreateBundles()
    {
        string bundleDirectoryName = "ExportedDoomahs";
        string outputPath = Path.Combine(Application.dataPath, "..", bundleDirectoryName);

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        DeleteExistingBundles(outputPath);

        try
        {
            BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
        finally
        {
            // No action needed in finally block
        }

        DeleteManifestFiles(outputPath);
        
        string[] bundleFiles = Directory.GetFiles(outputPath);

        foreach (string bundleFile in bundleFiles)
        {
            if (!bundleFile.EndsWith(".bundle"))
            {
                string newFileName = bundleFile + ".bundle";
                File.Move(bundleFile, newFileName);
            }
        }
        
        DeleteBundleFile(outputPath, "ExportedDoomahs.bundle");

        Debug.Log("Asset bundles created at: " + outputPath);

        if (!string.IsNullOrEmpty(author) || !string.IsNullOrEmpty(levelName))
        {
            CreateInfoFile(outputPath, author, levelName);
        }

        ZipAndRenameBundle(outputPath, levelImage);
        
        DeleteAllBundleFiles(outputPath);

        EditorUtility.RevealInFinder(outputPath);
    }

    static void DeleteManifestFiles(string directory)
    {
        string[] manifestFiles = Directory.GetFiles(directory, "*manifest*");
        foreach (string manifestFile in manifestFiles)
        {
            File.Delete(manifestFile);
        }
    }
    
    static void DeleteAllBundleFiles(string directory)
    {
        string[] manifestFiles = Directory.GetFiles(directory, "*bundle*");
        foreach (string manifestFile in manifestFiles)
        {
            File.Delete(manifestFile);
        }
    }

    
    static void DeleteExistingBundles(string directory)
    {
        string[] files = Directory.GetFiles(directory);
        foreach (string filePath in files)
        {
            File.Delete(filePath);
        }
    }

    static void DeleteBundleFile(string directory, string fileName)
    {
        string filePath = Path.Combine(directory, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    static void CreateInfoFile(string directory, string author, string levelName)
    {
        string infoFilePath = Path.Combine(directory, "info.txt");
        using (StreamWriter writer = new StreamWriter(infoFilePath))
        {
            writer.WriteLine(author);
            writer.WriteLine(levelName);
        }
    }

    static void ZipAndRenameBundle(string outputPath, Texture2D levelImage)
    {
        string[] bundleFiles = Directory.GetFiles(outputPath, "*.bundle");

        if (bundleFiles.Length == 0)
        {
            Debug.LogError("No asset bundles found in ExportedDoomahs directory.");
            return;
        }

        string firstBundleName = Path.GetFileNameWithoutExtension(bundleFiles[0]);

        string zipFileName = Path.Combine(outputPath, firstBundleName + ".zip");

        using (FileStream zipFileStream = new FileStream(zipFileName, FileMode.Create))
        {
            using (ZipArchive zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Create))
            {
                // Add bundle files
                foreach (string bundleFile in bundleFiles)
                {
                    string entryName = Path.GetFileName(bundleFile);
                    ZipArchiveEntry entry = zipArchive.CreateEntry(entryName);
                    using (FileStream fileStream = new FileStream(bundleFile, FileMode.Open))
                    {
                        using (Stream entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }

                if (levelImage != null)
                {
                    string levelImagePath = AssetDatabase.GetAssetPath(levelImage);
                    string entryName = Path.GetFileName(levelImagePath);
                    ZipArchiveEntry entry = zipArchive.CreateEntry(entryName);
                    using (FileStream fileStream = new FileStream(levelImagePath, FileMode.Open))
                    {
                        using (Stream entryStream = entry.Open())
                        {
                            fileStream.CopyTo(entryStream);
                        }
                    }
                }

                // Add info.txt file
                string infoFilePath = Path.Combine(outputPath, "info.txt");
                if (File.Exists(infoFilePath))
                {
                    ZipArchiveEntry infoEntry = zipArchive.CreateEntry("info.txt");
                    using (FileStream infoFileStream = new FileStream(infoFilePath, FileMode.Open))
                    {
                        using (Stream entryStream = infoEntry.Open())
                        {
                            infoFileStream.CopyTo(entryStream);
                        }
                    }
                }
            }
        }

        string doomahFileName = Path.Combine(outputPath, firstBundleName + ".doomah");
        File.Move(zipFileName, doomahFileName);
    }
}
