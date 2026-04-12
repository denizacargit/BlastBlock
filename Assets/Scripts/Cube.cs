using UnityEngine;

public class Cube : MonoBehaviour
{
    public string color; // Set this to "r", "g", "b", or "y" in the Inspector for each prefab
    public int x;
    public int y;

    private GridManager gridManager;

    void Start()
    {
        gridManager = Object.FindFirstObjectByType<GridManager>();
    }

    // This detects the click
    void OnMouseDown()
    {
        if (gridManager != null)
        {
            gridManager.OnCubeClicked(this);
        }
    }
}