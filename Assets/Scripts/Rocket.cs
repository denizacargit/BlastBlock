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

    void Start()
    {
        gridManager = Object.FindFirstObjectByType<GridManager>();
    }

    void OnMouseDown()
    {
        if (gridManager != null)
        {
            gridManager.OnRocketClicked(this);
        }
    }
}
