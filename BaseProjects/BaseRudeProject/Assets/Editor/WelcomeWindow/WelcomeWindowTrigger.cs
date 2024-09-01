using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class WelcomeWindowTrigger : ScriptableObject
{
    static WelcomeWindowTrigger m_Instance = null;

    static WelcomeWindowTrigger()
    {
        EditorApplication.update += OnInit;
    }

    static void OnInit()
    {
        EditorApplication.update -= OnInit;
        m_Instance = FindObjectOfType<WelcomeWindowTrigger>();
        if (m_Instance == null)
        {
            m_Instance = CreateInstance<WelcomeWindowTrigger>();

            WelcomeWindowSettings settings = WelcomeWindowSettings.Instance;
            if (settings.showOnStartup)
                WelcomeWindow.OpenWindow();
            else if (settings.firstTime)
            {
                settings.firstTime = false;
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                WelcomeWindow.OpenWindow();
            }
        }
    }
}
