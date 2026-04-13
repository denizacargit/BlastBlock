using UnityEngine;
using System.Collections.Generic;

public class Obstacle : MonoBehaviour
{
    [Header("Settings")]
    public string obstacleType; // Set to "bo", "s", or "v" in Inspector
    public int health = 1;      // Set to 2 for the Vase, 1 for Box/Stone

    [Header("Visual States")]
    public Sprite crackedVaseSprite; // Assign "vase_02" here for the Vase prefab

    [Header("Shards to Spawn on Death")]
    public List<GameObject> shardPrefabs; // Drag your 3 unique particle prefabs here

    // This ensures shards spawn inside the Obstacles folder in the Hierarchy
    private Transform effectsParent;

    void Start()
    {
        // Automatically find the parent folder we created in the Hierarchy
        GameObject parentObj = GameObject.Find("Obstacles");
        if (parentObj != null)
        {
            effectsParent = parentObj.transform;
        }
    }

    public void TakeDamage()
    {
        health--;

        // If it's a vase and has 1 health left, show the cracked sprite
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
        foreach (GameObject shardPrefab in shardPrefabs)
        {
            if (shardPrefab != null)
            {
                // Spawn the shard at the obstacle's position
                GameObject piece = Instantiate(shardPrefab, transform.position, Quaternion.identity, effectsParent);
                
                Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    // Create an "explosion" force: 
                    // X goes left or right, Y always goes UP first (like a fountain)
                    Vector2 force = new Vector2(Random.Range(-3f, 3f), Random.Range(3f, 6f));
                    rb.AddForce(force, ForceMode2D.Impulse);
                }

                // The shard script handles the spinning, we just handle the cleanup
                Destroy(piece, 1.5f);
            }
        }
        
        Destroy(gameObject); // Remove the original box/stone/vase
    }
}