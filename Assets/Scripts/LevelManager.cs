using UnityEngine;
using TMPro; // Required for the TextMeshPro variable
using UnityEngine.SceneManagement; // Required to switch scenes
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private const int MinLevel = 1;
    private const int MaxLevel = 10;

    // This is the variable we will drag our text object into
    public TextMeshProUGUI levelButtonText; 
    
    private int currentLevel;

    void Start()
    {
        // 1. Load the saved level from memory (Default to 1 if first time playing)
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", MinLevel);
        
        // 2. Update the button text based on the level
        UpdateUI();
        BindLevelButtons();
    }

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

    bool IsResetButton(string buttonName)
    {
        return buttonName.ToLowerInvariant().Contains("reset");
    }

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

    public void ResetToLevelOne()
    {
        currentLevel = MinLevel;
        PlayerPrefs.SetInt("CurrentLevel", currentLevel);
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void RefreshFromSavedLevel()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", MinLevel);
        UpdateUI();
    }

    // This is the function the button will trigger
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
