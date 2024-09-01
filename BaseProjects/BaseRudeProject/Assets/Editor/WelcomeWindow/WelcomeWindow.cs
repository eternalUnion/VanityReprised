using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class WelcomeWindow : EditorWindow
{
    [MenuItem("RUDE/Open welcome window", priority = 1000)]
    public static void OpenWindow()
    {
        WelcomeWindow window = GetWindow<WelcomeWindow>();
        window.titleContent = new GUIContent("Welcome");
    }

    private void CreateGUI()
    {
        try
        {
            VisualTreeAsset window = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/WelcomeWindow/window.uxml");
            window.CloneTree(rootVisualElement);

            Button rudeWikiButton = rootVisualElement.Q<Button>("rude-wiki");
            if (rudeWikiButton != null)
                rudeWikiButton.clicked += () => Application.OpenURL("https://coolboi21.github.io/Rude-Docs/#/Home");

            Button esWikiButton = rootVisualElement.Q<Button>("es-wiki");
            if (esWikiButton != null)
                esWikiButton.clicked += () => Application.OpenURL("https://layzyidiot.github.io/e-sw/#/");

            Button tundraWikiButton = rootVisualElement.Q<Button>("tundra-wiki");
            if (tundraWikiButton != null)
                tundraWikiButton.clicked += () => Application.OpenURL("https://docs.tundra.pitr.dev/category/important");

            Button openExporterButton = rootVisualElement.Q<Button>("open-exporter");
            if (openExporterButton != null)
                openExporterButton.clicked += () =>
                {
                    RudeExporter wnd = GetWindow<RudeExporter>();
                    wnd.titleContent = new GUIContent("Rude Exporter");
                };

            Toggle showOnStartupToggle = rootVisualElement.Q<Toggle>("show-on-startup");
            if (showOnStartupToggle != null)
            {
                WelcomeWindowSettings settings = WelcomeWindowSettings.Instance;
                showOnStartupToggle.SetValueWithoutNotify(settings.showOnStartup);

                showOnStartupToggle.RegisterValueChangedCallback(e =>
                {
                    if (settings == null)
                        settings = WelcomeWindowSettings.Instance;

                    Undo.RecordObject(settings, "Changed welcome window setting");
                    settings.showOnStartup = e.newValue;
                    EditorUtility.SetDirty(settings);
                });

            }

            void RefreshUI()
            {
                if (rootVisualElement == null)
                {
                    Undo.undoRedoPerformed -= RefreshUI;
                    return;
                }

                WelcomeWindowSettings settings = WelcomeWindowSettings.Instance;

                if (showOnStartupToggle != null)
                    showOnStartupToggle.SetValueWithoutNotify(settings.showOnStartup);
            }

            Undo.undoRedoPerformed += RefreshUI;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
