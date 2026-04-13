// To deal with the shards that are created after the obstacle is exploded

using UnityEngine;

public class Shard : MonoBehaviour
{
    private float rotationSpeed;

    void Start()
    {
        // Pick a random rotation speed between -500 and 500 degrees per second
        rotationSpeed = Random.Range(-500f, 500f);

        // Optional: Give it a slight random scale so the pieces look varied
        float randomScale = Random.Range(0.8f, 1.2f);
        transform.localScale *= randomScale;
    }

    void Update()
    {
        // Rotate the shard every frame based on the speed chosen at the start
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}