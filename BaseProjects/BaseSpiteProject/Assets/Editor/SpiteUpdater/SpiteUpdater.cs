using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class SpiteUpdater : Editor
{
    public const string SPITE_VERSION_PATH = "SpiteVersion.json";

    private static string ByteArrayToString(byte[] ba)
    {
        return BitConverter.ToString(ba).Replace("-", "").ToLower();
    }

    [MenuItem("SPITE/Check for updates", priority = 10001)]
    private static void CheckForUpdates()
    {
        CheckForUpdates(false);
    }

    public static void CheckForUpdates(bool silent)
    {
        // Load online catalog
        UnityWebRequest catalogReq = new UnityWebRequest("https://raw.githubusercontent.com/eternalUnion/SpiteUpdates/main/UpdateCatalog.json");
        catalogReq.downloadHandler = new DownloadHandlerBuffer();
        var catalogHandler = catalogReq.SendWebRequest();

        {
            bool canceled = false;
            do
            {
                canceled = EditorUtility.DisplayCancelableProgressBar("Spite Updater", "Downloading catalog...", catalogHandler.progress);
                Task.Delay(100).Wait();
            }
            while (!catalogHandler.isDone || canceled);

            EditorUtility.ClearProgressBar();
            if (canceled)
            {
                if (!catalogReq.isDone)
                    catalogReq.Abort();
                catalogReq.Dispose();
                return;
            }
        }

        if (catalogReq.isNetworkError || catalogReq.isHttpError)
        {
            catalogReq.Dispose();
            if (!silent)
                EditorUtility.DisplayDialog("Spite Updater", "Failed to download update catalog", "Close");
            return;
        }

        SpiteUpdateCatalog catalog;
        try
        {
            catalog = JsonConvert.DeserializeObject<SpiteUpdateCatalog>(catalogReq.downloadHandler.text);
            catalogReq.Dispose();
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            if (!silent)
                EditorUtility.DisplayDialog("Spite Updater", "Failed to load update catalog", "Close");
            catalogReq.Dispose();
            return;
        }

        // Load local file
        SpiteVersionInfo versionInfo;
        if (File.Exists(SPITE_VERSION_PATH))
        {
            try
            {
                versionInfo = JsonConvert.DeserializeObject<SpiteVersionInfo>(File.ReadAllText(SPITE_VERSION_PATH));
            }
            catch (Exception e)
            {
                Debug.LogException(e);

                if (silent)
                    return;

                if (!EditorUtility.DisplayDialog("Spite Updater", $"Failed to load version info file at {SPITE_VERSION_PATH}. Would you like to reapply all patches?", "Yes", "Cancel"))
                    return;

                versionInfo = new SpiteVersionInfo();
                versionInfo.CurrentBranch = catalog.CurrentBranch;
                versionInfo.CurrentVersion = "1.0.0";
                versionInfo.AppliedPatches = new List<string>();
                File.WriteAllText(SPITE_VERSION_PATH, JsonConvert.SerializeObject(versionInfo, Formatting.Indented));
            }
        }
        else
        {
            Debug.Log($"Could not locate version file at {SPITE_VERSION_PATH}");

            if (silent)
                return;

            if (!EditorUtility.DisplayDialog("Spite Updater", $"Failed to load version info file at {SPITE_VERSION_PATH}. Would you like to reapply all patches?", "Yes", "Cancel"))
                return;

            versionInfo = new SpiteVersionInfo();
            versionInfo.CurrentBranch = catalog.CurrentBranch;
            versionInfo.CurrentVersion = "1.0.0";
            versionInfo.AppliedPatches = new List<string>();
            File.WriteAllText(SPITE_VERSION_PATH, JsonConvert.SerializeObject(versionInfo, Formatting.Indented));
        }

        // Check branch
        if (versionInfo.CurrentBranch != catalog.CurrentBranch)
        {
            if (!silent)
                EditorUtility.DisplayDialog("Spite Updater", $"Warning: Current spite project is built using an older version of the game/vanity. Current branch is '{versionInfo.CurrentBranch}'. Latest branch is '{catalog.CurrentBranch}'.", "Ok");
        }

        // Check for files
        StringBuilder log = new StringBuilder();
        int fileUpdateCount = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (SpiteFileInfo fileInfo in catalog.Files)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(fileInfo.Guid);
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = fileInfo.Path;
                    if (!File.Exists(filePath))
                    {
                        Debug.LogWarning($"Failed to locate file {fileInfo.Path} ({fileInfo.Guid})");
                        continue;
                    }
                }

                string hash;
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    SHA256 hashObj = SHA256.Create();
                    hashObj.Initialize();
                    hash = ByteArrayToString(hashObj.ComputeHash(fs));
                }

                if (hash == fileInfo.SHA256)
                    continue;

                UnityWebRequest fileReq = new UnityWebRequest(fileInfo.Url);
                fileReq.downloadHandler = new DownloadHandlerBuffer();
                var fileHandle = fileReq.SendWebRequest();

                {
                    bool canceled = false;
                    do
                    {
                        canceled = EditorUtility.DisplayCancelableProgressBar("Downloading file", fileInfo.Path, fileHandle.progress);
                        Task.Delay(100).Wait();
                    }
                    while (!fileReq.isDone || canceled);

                    EditorUtility.ClearProgressBar();
                    if (canceled)
                    {
                        if (!fileReq.isDone)
                            fileReq.Abort();
                        fileReq.Dispose();
                        return;
                    }
                }

                if (fileReq.isHttpError || fileReq.isNetworkError)
                {
                    if (!silent)
                        EditorUtility.DisplayDialog("Spite Updater", "Failed to download file", "Close");
                    fileReq.Dispose();
                    continue;
                }

                File.WriteAllBytes(filePath, fileReq.downloadHandler.data);
                fileReq.Dispose();

                Debug.Log($"Updated file {filePath}");
                log.Append($"Updated file {filePath}\n");
                fileUpdateCount += 1;
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        if (log.Length != 0)
            log.Append("\n\n\n");

        // Check for patches
        int patchCount = 0;
        try
        {
            if (catalog.Updates.TryGetValue(versionInfo.CurrentBranch, out List<SpiteUpdateInfo> patches))
            {
                foreach (SpiteUpdateInfo patch in patches)
                {
                    if (versionInfo.AppliedPatches.Contains(patch.Version))
                        continue;

                    if (!Directory.Exists("tmp"))
                        Directory.CreateDirectory("tmp");

                    string patchFilePath = $"tmp/patch-${versionInfo.CurrentBranch}-{patch.Version}.unitypackage";
                    if (File.Exists(patchFilePath))
                        File.Delete(patchFilePath);

                    UnityWebRequest patchRequest = new UnityWebRequest(patch.Url);
                    patchRequest.downloadHandler = new DownloadHandlerFile(patchFilePath);
                    var patchHandler = patchRequest.SendWebRequest();

                    {
                        bool canceled = false;
                        do
                        {
                            canceled = EditorUtility.DisplayCancelableProgressBar("Spite Updater", "Downloading patch...", patchHandler.progress);
                            Task.Delay(100).Wait();
                        }
                        while (!patchHandler.isDone || canceled);

                        EditorUtility.ClearProgressBar();
                        if (canceled)
                        {
                            if (!patchHandler.isDone)
                                patchRequest.Abort();
                            patchRequest.Dispose();
                            if (File.Exists(patchFilePath))
                                File.Delete(patchFilePath);
                            return;
                        }
                    }

                    if (patchRequest.isHttpError || patchRequest.isNetworkError)
                    {
                        if (!silent)
                            EditorUtility.DisplayDialog("Spite Updater", $"Failed to download patch {patch.Version}", "Close");
                        patchRequest.Dispose();
                        if (File.Exists(patchFilePath))
                            File.Delete(patchFilePath);
                        return;
                    }

                    AssetDatabase.ImportPackage(patchFilePath, false);
                    if (File.Exists(patchFilePath))
                        File.Delete(patchFilePath);
                    patchRequest.Dispose();

                    if (patch.FilesToDelete != null)
                    {
                        foreach (string filePath in patch.FilesToDelete)
                        {
                            try
                            {
                                if (File.Exists(filePath))
                                    File.Delete(filePath);
                            }
                            catch (Exception ex) { Debug.LogException(ex); }
                        }
                    }

                    versionInfo.CurrentVersion = patch.Version;
                    versionInfo.AppliedPatches.Add(patch.Version);
                    File.WriteAllText(SPITE_VERSION_PATH, JsonConvert.SerializeObject(versionInfo, Formatting.Indented));

                    Debug.Log($"Installed patch {versionInfo.CurrentBranch}-{patch.Version}\n\n{patch.Changelog}");
                    log.Append($"Patch Version {patch.Version}\n===================\n{patch.Changelog}\n\n");
                    patchCount += 1;
                }
            }
        }
        finally
        {
            if (fileUpdateCount == 0 && patchCount == 0)
            {
                if (!silent)
                    EditorUtility.DisplayDialog("Spite Updater", $"Spite is up to date", "Close");
            }
            else
            {
                EditorUtility.DisplayDialog("Spite Updater", $"{fileUpdateCount} {(fileUpdateCount == 1 ? "file" : "files")} updated. {patchCount} {(patchCount == 1 ? "patch" : "patches")} applied.", "Close");
            }

            if (log.Length != 0)
            {
                File.WriteAllText("tmp/changelog.txt", log.ToString());
                Application.OpenURL($"file://{Path.GetFullPath("tmp/changelog.txt")}");
            }
        }
    }
}
