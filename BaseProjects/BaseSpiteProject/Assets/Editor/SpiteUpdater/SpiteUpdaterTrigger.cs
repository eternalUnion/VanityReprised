using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SpiteUpdaterTrigger : ScriptableObject
{
    static SpiteUpdaterTrigger m_Instance = null;

    static SpiteUpdaterTrigger()
    {
        EditorApplication.update += OnInit;
    }

    static void OnInit()
    {
        EditorApplication.update -= OnInit;
        m_Instance = FindObjectOfType<SpiteUpdaterTrigger>();
        if (m_Instance == null)
        {
            m_Instance = CreateInstance<SpiteUpdaterTrigger>();
            Debug.Log("Checking for updates...");
            SpiteUpdater.CheckForUpdates(true);
        }
    }
}
