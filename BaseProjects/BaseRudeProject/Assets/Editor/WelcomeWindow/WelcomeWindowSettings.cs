using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WelcomeWindowSettings : ScriptableObject
{
    private static WelcomeWindowSettings _instance;

    private const string SETTINGS_PATH = "Assets/Editor/WelcomeWindow/settings.asset";
    public static WelcomeWindowSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = AssetDatabase.LoadAssetAtPath<WelcomeWindowSettings>(SETTINGS_PATH);
                if (_instance == null)
                {
                    _instance = CreateInstance<WelcomeWindowSettings>();
                    AssetDatabase.CreateAsset(_instance, SETTINGS_PATH);
                    AssetDatabase.SaveAssets();
                }
            }

            return _instance;
        }
    }

    public bool showOnStartup = false;

    public bool firstTime = true;
}
