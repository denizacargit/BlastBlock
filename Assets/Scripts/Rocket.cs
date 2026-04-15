using UnityEngine;

public enum RocketDirection
{
    Horizontal,
    Vertical
}

public class Rocket : MonoBehaviour
{
    public int x;
    public int y;
    public RocketDirection direction;

    private GridManager gridManager;

    // Caches the board controller.
    void Start()
    {
        gridManager = Object.FindFirstObjectByType<GridManager>();
    }

    // Sends rocket taps to the board.
    void OnMouseDown()
    {
        if (gridManager != null)
        {
            gridManager.OnRocketClicked(this);
        }
    }
}
