using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public partial class GridManager
{
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

    IEnumerator ReturnToMainSceneAfterDelay()
    {
        yield return new WaitForSeconds(WinReturnDelay);
        SceneManager.LoadScene("MainScene");
    }

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

    void ShowFailPopup()
    {
        levelCompleted = true;

        if (failPopup != null)
        {
            failPopup.SetActive(true);
        }
    }

    void HideFailPopup()
    {
        if (failPopup != null)
        {
            failPopup.SetActive(false);
        }
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public void ReturnToMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }

    void UpdateMovesUI()
    {
        if (moveCounterText != null)
        {
            moveCounterText.text = movesLeft.ToString();
        }
    }

    public void ClearObstacleAt(int x, int y)
    {
        if (x >= 0 && x < currentLevelData.grid_width && y >= 0 && y < currentLevelData.grid_height)
        {
            allObstacles[x, y] = null;
        }
    }
}
