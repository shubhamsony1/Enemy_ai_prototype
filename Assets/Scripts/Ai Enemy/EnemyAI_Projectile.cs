using UnityEngine;

public class EnemyAI_Projectile : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Attack Settings")]
    public float minAttackRange = 10f;
    public float maxAttackRange = 35f;
    public float attackCooldown = 3f;
    public int projectileDamage = 20;

    private float _cooldownTimer = 0f;

    void Update()
    {
        if (player == null) return;

        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        float distance = Vector3.Distance(transform.position, player.position);

        // Only fire if player is within min and max range
        if (distance >= minAttackRange && distance <= maxAttackRange && _cooldownTimer <= 0f)
        {
            FireProjectile();
            _cooldownTimer = attackCooldown;
        }

        // Rotate towards player smoothly
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }

    void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null || player == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        EnemyProjectile projectile = proj.GetComponent<EnemyProjectile>();
        if (projectile != null)
            projectile.Launch(player, projectileDamage, 15f, 5f); // speed & arcHeight

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        Debug.Log($"[EnemyAI_Projectile] Fired projectile at player!");
    }

}
