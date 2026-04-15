using UnityEngine;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour
{
    [Header("Grid Info")]
    public int x; // GridManager tarafından atanacak 
    public int y; // GridManager tarafından atanacak 

    [Header("Settings")]
    public string obstacleType; // "bo" (Box), "s" (Stone), "v" (Vase) 
    public int health = 1;      // Vazo için 2, Kutu/Taş için 1 

    [Header("Visual States")]
    public Sprite crackedVaseSprite; // Vazo (Vase) için çatlamış sprite 

    [Header("Shards to Spawn on Death")]
    public List<GameObject> shardPrefabs;

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
