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
        // Spawn each shard from your list
        foreach (GameObject shard in shardPrefabs)
        {
            if (shard != null)
            {
                // Spawn with a tiny random offset so they don't overlap perfectly
                Vector3 offset = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
                
                // Instantiate shards as children of the Obstacles folder
                GameObject piece = Instantiate(shard, transform.position + offset, Quaternion.identity, effectsParent);
                
                // Cleanup: Destroy the shards after 1.5 seconds
                Destroy(piece, 1.5f);
            }
        }
        
        // Finally, remove the main obstacle from the grid
        Destroy(gameObject); 
    }
}