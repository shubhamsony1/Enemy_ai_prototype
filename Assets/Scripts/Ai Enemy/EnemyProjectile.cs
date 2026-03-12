using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Vector3 _startPos;
    private Vector3 _targetPos;
    private float _speed;
    private float _arcHeight;
    private int _damage;

    private float _travelTime;
    private float _elapsedTime;
    private bool _initialized = false;
    private Transform _player;

    public void Launch(Transform player, int damage, float speed = 15f, float arcHeight = 5f)
    {
        _player = player;
        _damage = damage;
        _startPos = transform.position;
        _targetPos = player.position; // initial target
        _speed = speed;
        _arcHeight = arcHeight;

        float dist = Vector3.Distance(_startPos, _targetPos);
        _travelTime = dist / _speed;
        _elapsedTime = 0f;
        _initialized = true;
    }

    void Update()
    {
        if (!_initialized || _player == null) return;

        _elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsedTime / _travelTime);

        // Parabolic arc
        Vector3 nextPos = Vector3.Lerp(_startPos, _targetPos, t);
        nextPos.y += Mathf.Sin(t * Mathf.PI) * _arcHeight;

        // Move projectile
        transform.position = nextPos;

        // Face movement direction
        Vector3 moveDir = (nextPos - _startPos).normalized;
        if (moveDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(moveDir);

        // Check collision with player (simple distance check)
        if (Vector3.Distance(transform.position, _player.position) < 1f)
        {
            _player.GetComponent<PlayerHP>()?.TakeDamage(_damage);
            Debug.Log($"[EnemyProjectile] Hit player for {_damage} damage!");
            Destroy(gameObject);
        }

        // Destroy projectile after it reaches target
        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
