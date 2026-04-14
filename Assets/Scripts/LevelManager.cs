using UnityEngine;
using TMPro; // Required for the TextMeshPro variable
using UnityEngine.SceneManagement; // Required to switch scenes

public class LevelManager : MonoBehaviour
{
    // This is the variable we will drag our text object into
    public TextMeshProUGUI levelButtonText; 
    
    private int currentLevel;

    void Start()
    {
        // 1. Load the saved level from memory (Default to 1 if first time playing)
        currentLevel = Mathf.Clamp(PlayerPrefs.GetInt("CurrentLevel", 1), 1, 10);
        
        // 2. Update the button text based on the level
        UpdateUI();
    }

    void UpdateUI()
    {
        if (levelButtonText == null) return;

        levelButtonText.text = "Level " + currentLevel;
    }

    // This is the function the button will trigger
    public void LoadCurrentLevel()
    {
        currentLevel = Mathf.Clamp(PlayerPrefs.GetInt("CurrentLevel", currentLevel), 1, 10);
        SceneManager.LoadScene("LevelScene");
    }
}
