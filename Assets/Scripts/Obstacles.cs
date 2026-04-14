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

    private Transform effectsParent;

    void Start()
    {
        GameObject parentObj = GameObject.Find("Obstacles");
        if (parentObj != null)
        {
            effectsParent = parentObj.transform;
        }
    }

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
        // ÖNEMLİ: GridManager'daki referansı temizlemek için olay (event) fırlatabilir 
        // veya GridManager'a doğrudan erişebiliriz. 
        // En basit yol:
        FindObjectOfType<GridManager>().ClearObstacleAt(x, y);

        foreach (GameObject shardPrefab in shardPrefabs)
        {
            if (shardPrefab != null)
            {
                GameObject piece = Instantiate(shardPrefab, transform.position, Quaternion.identity, effectsParent);
                
                Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 force = new Vector2(Random.Range(-3f, 3f), Random.Range(3f, 6f));
                    rb.AddForce(force, ForceMode2D.Impulse);
                }

                Destroy(piece, 1.5f);
            }
        }
        
        Destroy(gameObject);
    }
}