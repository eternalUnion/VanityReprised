using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class RudeUpdaterTrigger : ScriptableObject
{
    static RudeUpdaterTrigger m_Instance = null;

    static RudeUpdaterTrigger()
    {
        EditorApplication.update += OnInit;
    }

    static void OnInit()
    {
        EditorApplication.update -= OnInit;
        m_Instance = FindObjectOfType<RudeUpdaterTrigger>();
        if (m_Instance == null)
        {
            m_Instance = CreateInstance<RudeUpdaterTrigger>();
            Debug.Log("Checking for updates...");
            RudeUpdater.CheckForUpdates(true);
        }
    }
}
