using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public partial class GridManager
{
    // Finishes the current level and shows the win popup.
    void CompleteLevel()
    {
        if (levelCompleted)
        {
            return;
        }

        levelCompleted = true;
        int nextLevel = currentLevelData.level_number + 1;
        PlayerPrefs.SetInt("CurrentLevel", nextLevel);
        PlayerPrefs.Save();
        Debug.Log($"Level {currentLevelData.level_number} complete!");

        if (celebrationPrefab != null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            Transform parent = canvas != null ? canvas.transform : null;
            GameObject celebration = Instantiate(celebrationPrefab, parent);
            celebration.transform.SetAsLastSibling();

            Canvas celebrationCanvas = celebration.GetComponent<Canvas>();
            if (celebrationCanvas == null)
            {
                celebrationCanvas = celebration.AddComponent<Canvas>();
            }

            celebrationCanvas.overrideSorting = true;
            celebrationCanvas.sortingOrder = 500;

            if (celebration.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                celebration.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            RectTransform celebrationRect = celebration.GetComponent<RectTransform>();
            if (celebrationRect != null)
            {
                celebrationRect.anchorMin = new Vector2(0.5f, 0.5f);
                celebrationRect.anchorMax = new Vector2(0.5f, 0.5f);
                celebrationRect.pivot = new Vector2(0.5f, 0.5f);
                celebrationRect.anchoredPosition = Vector2.zero;
            }

            WinPopupController popupController = celebration.GetComponent<WinPopupController>();
            if (popupController != null)
            {
                popupController.Play();
                return;
            }

            Debug.LogWarning("Celebration prefab is assigned, but it has no WinPopupController. Falling back to delayed MainScene load.");
        }

        StartCoroutine(ReturnToMainSceneAfterDelay());
    }

    // Returns to the main scene after a short pause.
    IEnumerator ReturnToMainSceneAfterDelay()
    {
        yield return new WaitForSeconds(WinReturnDelay);
        SceneManager.LoadScene("MainScene");
    }

    // Spends one move and checks for failure.
    void DecrementMoves()
    {
        movesLeft--;

        UpdateMovesUI();

        if (movesLeft <= 0)
        {
            Debug.Log("Out of moves! Game over");
            ShowFailPopup();
        }
    }

    // Opens the fail popup above the board.
    void ShowFailPopup()
    {
        levelCompleted = true;

        GameObject popup = GetOrCreateFailPopup();
        if (popup == null)
        {
            return;
        }

        popup.transform.SetAsLastSibling();
        ConfigurePopupCanvas(popup, 500);
        popup.SetActive(true);

        FailPopupController popupController = popup.GetComponent<FailPopupController>();
        if (popupController != null)
        {
            popupController.Play();
        }
    }

    // Hides any active fail popup.
    void HideFailPopup()
    {
        if (failPopup != null && failPopup.scene.IsValid())
        {
            failPopup.SetActive(false);
        }
    }

    // Finds or creates the fail popup instance.
    GameObject GetOrCreateFailPopup()
    {
        if (failPopup == null)
        {
            return null;
        }

        if (failPopup.scene.IsValid())
        {
            return failPopup;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        Transform parent = canvas != null ? canvas.transform : null;
        GameObject popup = Instantiate(failPopup, parent);
        failPopup = popup;

        RectTransform popupRect = popup.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.anchoredPosition = Vector2.zero;
        }

        return popup;
    }

    // Keeps popup UI above gameplay.
    void ConfigurePopupCanvas(GameObject popup, int sortingOrder)
    {
        Canvas popupCanvas = popup.GetComponent<Canvas>();
        if (popupCanvas == null)
        {
            popupCanvas = popup.AddComponent<Canvas>();
        }

        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = sortingOrder;

        if (popup.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
        {
            popup.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
    }

    // Restarts the current level.
    public void RetryLevel()
    {
        SceneManager.LoadScene("LevelScene");
    }

    // Opens the main menu scene.
    public void ReturnToMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    // Refreshes the moves counter text.
    void UpdateMovesUI()
    {
        if (moveCounterText != null)
        {
            moveCounterText.text = movesLeft.ToString();
        }
    }

    // Clears an obstacle slot in the grid.
    public void ClearObstacleAt(int x, int y)
    {
        if (x >= 0 && x < currentLevelData.grid_width && y >= 0 && y < currentLevelData.grid_height)
        {
            allObstacles[x, y] = null;
        }
    }
}
