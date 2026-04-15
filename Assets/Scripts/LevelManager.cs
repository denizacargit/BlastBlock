using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private const int MinLevel = 1;
    private const int MaxLevel = 10;

    public TextMeshProUGUI levelButtonText; 
    public TextMeshProUGUI levelButtonSmallerText;
    public GameObject levelButtonDefaultVisual;
    public GameObject levelButtonSmallerVisual;
    
    private int currentLevel;
    private string currentButtonLabel = "";

    // Loads the saved level and wires the menu button.
    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", MinLevel);
        ResolveLevelButtonText();
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

        PrepareHoverVisuals(levelButtonObject.transform);
        BindHoverEvents(levelButtonObject);
        UpdateUI();

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
        ResolveLevelButtonText();

        if (currentLevel > MaxLevel)
        {
            currentButtonLabel = "Finished";
        }
        else
        {
            currentButtonLabel = "Level " + Mathf.Clamp(currentLevel, MinLevel, MaxLevel);
        }

        if (levelButtonText != null)
        {
            levelButtonText.text = currentButtonLabel;
        }

        ResolveLevelButtonSmallerText();
        if (levelButtonSmallerText != null)
        {
            levelButtonSmallerText.text = currentButtonLabel;
        }

        TextMeshProUGUI[] texts = Object.FindObjectsOfType<TextMeshProUGUI>(false);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name == "levelButtonText" || text.name == "levelButtonSmallerText")
            {
                text.text = currentButtonLabel;
            }
        }
    }

    // Prepares the normal and hover visuals.
    void PrepareHoverVisuals(Transform levelButtonRoot)
    {
        if (levelButtonDefaultVisual == levelButtonRoot.gameObject)
        {
            levelButtonDefaultVisual = null;
        }

        if (levelButtonSmallerVisual == levelButtonRoot.gameObject)
        {
            levelButtonSmallerVisual = null;
        }

        if (levelButtonDefaultVisual != null && !levelButtonDefaultVisual.transform.IsChildOf(levelButtonRoot))
        {
            levelButtonDefaultVisual = Instantiate(levelButtonDefaultVisual, levelButtonRoot);
            levelButtonDefaultVisual.name = "LevelButtonDefault";
            MatchLevelButtonRect(levelButtonDefaultVisual.GetComponent<RectTransform>());
            ResolveLevelButtonTextFrom(levelButtonDefaultVisual);
        }

        if (levelButtonSmallerVisual == null)
        {
            Transform existingSmaller = levelButtonRoot.Find("LevelButtonSmaller");
            if (existingSmaller != null)
            {
                levelButtonSmallerVisual = existingSmaller.gameObject;
            }
        }
        else if (!levelButtonSmallerVisual.transform.IsChildOf(levelButtonRoot))
        {
            levelButtonSmallerVisual = Instantiate(levelButtonSmallerVisual, levelButtonRoot);
            levelButtonSmallerVisual.name = "LevelButtonSmaller";
            MatchLevelButtonRect(levelButtonSmallerVisual.GetComponent<RectTransform>());
            ResolveLevelButtonSmallerText();
        }

        SetVisualRaycasts(levelButtonDefaultVisual, false);
        SetVisualRaycasts(levelButtonSmallerVisual, false);
        ResolveLevelButtonSmallerText();
        ShowDefaultLevelButtonVisual();
    }

    // Matches the hover visual to the button bounds.
    void MatchLevelButtonRect(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    // Prevents hover visuals from blocking the root button.
    void SetVisualRaycasts(GameObject visual, bool enabled)
    {
        if (visual == null)
        {
            return;
        }

        Graphic[] graphics = visual.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = enabled;
        }
    }

    // Finds the hover button text if the scene reference is missing.
    void ResolveLevelButtonSmallerText()
    {
        if (levelButtonSmallerText != null || levelButtonSmallerVisual == null)
        {
            return;
        }

        foreach (TextMeshProUGUI text in levelButtonSmallerVisual.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == "levelButtonSmallerText" || text.name == "levelButtonText")
            {
                levelButtonSmallerText = text;
                text.text = currentButtonLabel;
                return;
            }
        }
    }

    // Finds the main text inside a visual object.
    void ResolveLevelButtonTextFrom(GameObject visual)
    {
        if (levelButtonText != null || visual == null)
        {
            return;
        }

        foreach (TextMeshProUGUI text in visual.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == "levelButtonText")
            {
                levelButtonText = text;
                text.text = currentButtonLabel;
                return;
            }
        }
    }

    // Adds pointer enter and exit listeners to the root button.
    void BindHoverEvents(GameObject levelButtonObject)
    {
        EventTrigger trigger = levelButtonObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = levelButtonObject.AddComponent<EventTrigger>();
        }

        trigger.triggers.Clear();
        AddHoverEvent(trigger, EventTriggerType.PointerEnter, ShowSmallerLevelButtonVisual);
        AddHoverEvent(trigger, EventTriggerType.PointerExit, ShowDefaultLevelButtonVisual);
    }

    // Adds one EventTrigger callback.
    void AddHoverEvent(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = eventType
        };

        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    // Shows the hover version of the button.
    public void ShowSmallerLevelButtonVisual(BaseEventData eventData)
    {
        if (levelButtonSmallerVisual == null)
        {
            return;
        }

        if (levelButtonDefaultVisual != null && levelButtonDefaultVisual != levelButtonSmallerVisual)
        {
            levelButtonDefaultVisual.SetActive(false);
        }

        levelButtonSmallerVisual.SetActive(true);
    }

    // Shows the normal version of the button.
    public void ShowDefaultLevelButtonVisual(BaseEventData eventData = null)
    {
        if (levelButtonDefaultVisual != null)
        {
            levelButtonDefaultVisual.SetActive(true);
        }

        if (levelButtonSmallerVisual != null && levelButtonSmallerVisual != levelButtonDefaultVisual)
        {
            levelButtonSmallerVisual.SetActive(false);
        }
    }

    // Finds the level button text if the scene reference is missing.
    void ResolveLevelButtonText()
    {
        if (levelButtonText != null)
        {
            return;
        }

        GameObject levelButtonObject = GameObject.Find("LevelButton");
        if (levelButtonObject == null)
        {
            return;
        }

        foreach (TextMeshProUGUI text in levelButtonObject.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (text.name == "levelButtonText")
            {
                levelButtonText = text;
                return;
            }
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

    // Keeps old button references working.
    public void PlayLevelButtonAnimation()
    {
        LoadCurrentLevel();
    }
}
