using UnityEngine;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour
{
    [Header("Grid Info")]
    public int x;
    public int y;

    [Header("Settings")]
    public string obstacleType;
    public int health = 1;

    [Header("Visual States")]
    public Sprite crackedVaseSprite;

    [Header("Shards to Spawn on Death")]
    public List<GameObject> shardPrefabs;

    // Applies one hit to the obstacle.
    public void TakeDamage()
    {
        health--;

        if (health == 1 && obstacleType == "v" && crackedVaseSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = crackedVaseSprite;
        }
        else if (health <= 0)
        {
            Explode();
        }
    }

    // Removes the obstacle from the board.
    void Explode()
    {
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            gridManager.ClearObstacleAt(x, y);
        }

        Destroy(gameObject);
    }
}
