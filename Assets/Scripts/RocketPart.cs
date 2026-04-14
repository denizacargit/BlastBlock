using UnityEngine;

public class RocketPart : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float lifetime;

    public void Initialize(Vector3 moveDirection, float moveSpeed, float duration)
    {
        direction = moveDirection.normalized;
        speed = moveSpeed;
        lifetime = duration;
    }

    void Update()
    {
        transform.localPosition += direction * speed * Time.deltaTime;
        lifetime -= Time.deltaTime;

        if (lifetime <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
