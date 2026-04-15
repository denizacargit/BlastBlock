using UnityEngine;

public class Cube : MonoBehaviour
{
    public string color;
    public int x;
    public int y;

    private GridManager gridManager;

    // Caches the board controller.
    void Start()
    {
        gridManager = Object.FindFirstObjectByType<GridManager>();
    }

    // Sends cube taps to the board.
    void OnMouseDown()
    {
        if (gridManager != null)
        {
            gridManager.OnCubeClicked(this);
        }
    }
}
