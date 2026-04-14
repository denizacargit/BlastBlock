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
            Instantiate(celebrationPrefab, transform.position, Quaternion.identity);
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
