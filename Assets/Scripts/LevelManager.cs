using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private const int MinLevel = 1;
    private const int MaxLevel = 10;

    public TextMeshProUGUI levelButtonText; 
    
    private int currentLevel;

    // Loads the saved level and wires the menu button.
    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", MinLevel);
        UpdateUI();
        BindLevelButtons();
    }

    // Connects every level button visual to the same action.
    void BindLevelButtons()
    {
        GameObject levelButtonObject = GameObject.Find("LevelButton");
        if (levelButtonObject == null)
        {
            return;
        }

        Image rootImage = levelButtonObject.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.raycastTarget = true;
        }

        Button rootButton = levelButtonObject.GetComponent<Button>();
        if (rootButton == null)
        {
            rootButton = levelButtonObject.AddComponent<Button>();
        }

        rootButton.onClick.RemoveListener(LoadCurrentLevel);
        rootButton.onClick.AddListener(LoadCurrentLevel);

        Button[] levelButtons = levelButtonObject.GetComponentsInChildren<Button>(true);
        foreach (Button button in levelButtons)
        {
            if (IsResetButton(button.gameObject.name))
            {
                continue;
            }

            button.onClick.RemoveListener(LoadCurrentLevel);
            button.onClick.AddListener(LoadCurrentLevel);
        }
    }

    // Keeps utility buttons out of the level launch flow.
    bool IsResetButton(string buttonName)
    {
        return buttonName.ToLowerInvariant().Contains("reset");
    }

    // Displays the current progress on the main button.
    void UpdateUI()
    {
        if (levelButtonText == null) return;

        if (currentLevel > MaxLevel)
        {
            levelButtonText.text = "Finished";
        }
        else
        {
            levelButtonText.text = "Level " + Mathf.Clamp(currentLevel, MinLevel, MaxLevel);
        }
    }

    // Resets local progress for testing.
    public void ResetToLevelOne()
    {
        currentLevel = MinLevel;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
        UpdateUI();
    }

    // Reloads the saved level number into the UI.
    public void RefreshFromSavedLevel()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", MinLevel);
        UpdateUI();
    }

    // Opens the saved level.
    public void LoadCurrentLevel()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", currentLevel);

        if (currentLevel <= MaxLevel)
        {
            PlayerPrefs.SetInt("CurrentLevel", Mathf.Clamp(currentLevel, MinLevel, MaxLevel));
            PlayerPrefs.Save();
            SceneManager.LoadScene("LevelScene");
        }
    }
}
