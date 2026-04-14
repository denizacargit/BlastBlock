using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEditorTools : EditorWindow
{
    private const int MinLevel = 1;
    private const int MaxLevel = 10;
    private const string CurrentLevelKey = "CurrentLevel";
    private const string LevelScenePath = "Assets/Scenes/LevelScene.unity";
    private const string LevelSceneName = "LevelScene";

    private int selectedLevel = MinLevel;

    [MenuItem("Menu/Level Selector")]
    public static void ShowWindow()
    {
        LevelEditorTools window = GetWindow<LevelEditorTools>("Level Selector");
        window.selectedLevel = Mathf.Clamp(PlayerPrefs.GetInt(CurrentLevelKey, MinLevel), MinLevel, MaxLevel);
        window.minSize = new Vector2(260f, 120f);
        window.Show();
    }

    [MenuItem("Menu/Reset To Level 1")]
    public static void ResetToLevelOne()
    {
        SetCurrentLevel(MinLevel);
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Select Level", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Enter a level number from 1 to 10. The selected level is saved and will be used when LevelScene starts.", MessageType.Info);

        selectedLevel = EditorGUILayout.IntField("Level Number", selectedLevel);
        selectedLevel = Mathf.Clamp(selectedLevel, MinLevel, MaxLevel);

        EditorGUILayout.Space();

        if (GUILayout.Button("Save Selected Level"))
        {
            SetCurrentLevel(selectedLevel);
        }

        if (GUILayout.Button("Save And Open LevelScene"))
        {
            SetCurrentLevel(selectedLevel);
            OpenOrReloadLevelScene(selectedLevel);
        }
    }

    private static void SetCurrentLevel(int levelNumber)
    {
        int clampedLevel = Mathf.Clamp(levelNumber, MinLevel, MaxLevel);
        PlayerPrefs.SetInt(CurrentLevelKey, clampedLevel);
        PlayerPrefs.Save();

        if (Application.isPlaying)
        {
            GridManager gridManager = Object.FindFirstObjectByType<GridManager>();
            if (gridManager != null)
            {
                gridManager.LoadLevel(clampedLevel);
            }
        }

        Debug.Log($"Selected level set to {clampedLevel}.");
    }

    private static void OpenOrReloadLevelScene(int levelNumber)
    {
        if (Application.isPlaying)
        {
            SceneManager.LoadScene(LevelSceneName);
            return;
        }

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(LevelScenePath);
        }
    }
}
