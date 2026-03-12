using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float damage = 10f;
    public float speed = 50f;
    public float lifetime = 5f; // destroy after seconds

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;

        // Launch bullet forward
        rb.linearVelocity = transform.forward * speed;

        // Destroy automatically after lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("[Bullet] Hit: " + collision.gameObject.name);

        // Damage Enemy AI
        EnemyHP enemyHealth = collision.gameObject.GetComponent<EnemyHP>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage((int)damage);
            Debug.Log("[Bullet] Damaged Enemy AI: " + damage);
        }

        // Destroy bullet after hitting something
        Destroy(gameObject);
    }
}
