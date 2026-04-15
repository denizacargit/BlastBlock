using UnityEngine;

public class Shard : MonoBehaviour
{
    private float rotationSpeed;

    // Adds small visual variation.
    void Start()
    {
        rotationSpeed = Random.Range(-500f, 500f);
        float randomScale = Random.Range(0.8f, 1.2f);
        transform.localScale *= randomScale;
    }

    // Spins the shard while it is alive.
    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
