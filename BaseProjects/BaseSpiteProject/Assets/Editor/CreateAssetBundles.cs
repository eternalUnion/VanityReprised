using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.IO.Compression;
using System.Linq;

public class CreateAssetBundles : EditorWindow
{
    public static Texture2D levelImage;
    private static string author;
    private static string levelName;
    private static string doomahName;

    const string sourceScenePath = "Assets/SampleScene.unity";

    static string currentSceneName;
    static string currentDoomahName;
    static bool autoLevelFolder;
    static string selectedScene;

    public static int selectedTab = 0; // Static field to store the selected tab index

    [MenuItem("SPITE/New Level")]
    static void NewLevel()
    {
        CreateAssetBundles window = GetWindow<CreateAssetBundles>("Create Level");
        selectedTab = 0; // Set the tab index for "Create Level"
        window.Show();
    }

    [MenuItem("SPITE/Compile Level")]
    static void Init()
    {
        CreateAssetBundles window = GetWindow<CreateAssetBundles>("Create Asset Bundles");
        selectedTab = 1; // Set the tab index for "Build Level"
        window.Show();
    }

    void OnGUI()
    {
        // Tabs
        string[] tabNames = { "Create Level", "Build Level" };
        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        switch (selectedTab)
        {
            case 0:
                DrawCreateLevelGUI();
                break;
            case 1:
                DrawBuildLevelGUI();
                break;
        }
    }

    void DrawCreateLevelGUI()
    {
        GUILayout.Label("Create Level", EditorStyles.boldLabel);

        GUIStyle style = new GUIStyle(EditorStyles.miniBoldLabel);
        style.wordWrap = true;
        EditorGUILayout.LabelField("Please be aware if you have modified SampleScene this may not work!", style);

        GUILayout.Space(10);

        currentSceneName = EditorGUILayout.TextField("Scene Name", currentSceneName);
        currentDoomahName = EditorGUILayout.TextField("Doomah Name", currentDoomahName);

        autoLevelFolder = GUILayout.Toggle(autoLevelFolder, "Auto create level folder in assets.");

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create Level"))
        {
            if (string.IsNullOrEmpty(currentSceneName) || string.IsNullOrEmpty(currentDoomahName))
            {
                EditorGUILayout.HelpBox("Please fill out the names before you create your level!", MessageType.Warning);
                return;
            }

            string scenePath = "Assets/";
            if (autoLevelFolder)
            {
                scenePath += currentSceneName;
                Directory.CreateDirectory(Path.Combine(Application.dataPath, currentSceneName));
            }
            scenePath += "/" + currentSceneName + ".unity";

            FileUtil.CopyFileOrDirectory(sourceScenePath, scenePath);
            AssetDatabase.Refresh();

            Object scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            AssetImporter assetImporter = AssetImporter.GetAtPath(scenePath);

            assetImporter.assetBundleName = currentDoomahName;
            assetImporter.SaveAndReimport();

            EditorSceneManager.OpenScene(scenePath);
        }
    }

    void DrawBuildLevelGUI()
    {
        GUILayout.Label("Create Asset Bundles", EditorStyles.boldLabel);

        levelImage = (Texture2D)EditorGUILayout.ObjectField("Level Image", levelImage, typeof(Texture2D), false);

        EditorGUILayout.HelpBox("Please ensure the image is in 4:3 aspect ratio.", MessageType.Info);

        author = EditorGUILayout.TextField("Author", author);
        levelName = EditorGUILayout.TextField("Level Name", levelName);
        doomahName = EditorGUILayout.TextField(".doomah Name", doomahName);

        if (string.IsNullOrEmpty(doomahName))
        {
            EditorGUILayout.HelpBox(".doomah Name cannot be empty!", MessageType.Error);
        }

        GUILayout.Space(20); // Add space between fields and the dropdown

        // Scene Dropdown Logic
        Rect dropdownRect = EditorGUILayout.GetControlRect(); // Get the position of the dropdown button
        if (EditorGUI.DropdownButton(dropdownRect, new GUIContent(string.IsNullOrEmpty(selectedScene) ? "Select scene to build" : selectedScene), FocusType.Passive))
        {
            GenericMenu menu = new GenericMenu();

            // Filter excluded scenes
            string[] excludedScenes = { "Bootstrap", "CreditsMuseum2", "EarlyAccessEnd", "Endless", "Intermission1", "Intermission2", "Intro", "Level 0-1", "Level 0-2", "Level 0-3", "Level 0-4", "Level 0-5", "Level 0-S", "Level 1-1", "Level 1-2", "Level 1-3", "Level 1-4", "Level 1-S", "Level 2-1", "Level 2-2", "Level 2-3", "Level 2-4", "Level 2-S", "Level 3-1", "Level 3-2", "Level 4-1", "Level 4-2", "Level 4-3", "Level 4-4", "Level 4-S", "Level 5-1", "Level 5-2", "Level 5-3", "Level 5-4", "Level 5-S", "Level 6-1", "Level 6-2", "Level 7-1", "Level 7-2", "Level 7-3", "Level 7-4", "Level 7-S", "Level P-1", "Level P-2", "Main Menu", "TundraAssets", "Tutorial", "uk_construct" }; // Customize the scenes to exclude here
            foreach (var scenePath in Directory.GetFiles("Assets", "*.unity", SearchOption.AllDirectories))
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                if (System.Array.IndexOf(excludedScenes, sceneName) == -1)
                {
                    menu.AddItem(new GUIContent(sceneName), false, (object scene) => selectedScene = (string)scene, sceneName);
                }
            }

            // Drop the menu at the dropdownRect position
            menu.DropDown(dropdownRect);
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        bool allFieldsFilled = !string.IsNullOrEmpty(selectedScene) &&
                                !string.IsNullOrEmpty(doomahName) &&
                                levelImage != null;

        GUI.enabled = allFieldsFilled;

        if (GUILayout.Button("Build level"))
        {
            if (!allFieldsFilled)
            {
                Debug.LogError("Please ensure all fields are filled.");
                return;
            }

            // Set the selected scene's asset label to the .doomah name
            ClearAllAssetLabels();
            SetSceneAssetLabel(selectedScene, doomahName);
            CreateBundles();
        }

        GUI.enabled = true;
    }

    // Clear all asset labels
    static void ClearAllAssetLabels()
    {
        foreach (var assetPath in AssetDatabase.GetAllAssetPaths())
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                importer.assetBundleName = null;
                importer.SaveAndReimport();
            }
        }
    }

    // Set the selected scene's asset label to the .doomah name
    static void SetSceneAssetLabel(string sceneName, string label)
    {
        string scenePath = AssetDatabase.FindAssets(sceneName)
                                        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                                        .FirstOrDefault(path => path.EndsWith(".unity"));
        if (!string.IsNullOrEmpty(scenePath))
        {
            AssetImporter importer = AssetImporter.GetAtPath(scenePath);
            importer.assetBundleName = label;
            importer.SaveAndReimport();
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

        if (!levelImage) levelImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ULTRAKILL Assets/Textures/UI/questionMark.png");
        if (string.IsNullOrEmpty(author)) author = "UNKNOWN AUTHOR";
        if (string.IsNullOrEmpty(levelName)) levelName = "UNNAMED";

        try
        {
            AssetBundleBuild[] build = new AssetBundleBuild[1];
            build[0].assetBundleName = doomahName;
            string[] assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(doomahName);
            build[0].assetNames = assetNames;
            BuildPipeline.BuildAssetBundles(outputPath, build, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
        catch
        {
            Debug.LogError("Failed to build level!");
            return;
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

        EditorUtility.RevealInFinder(Path.Combine(outputPath, doomahName + ".doomah"));
        doomahName = "";
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
