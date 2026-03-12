using UnityEngine;
using UnityEngine.AI;
using GamePlay;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour, IHear
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public PlayerHP playerHealth;

    [Header("Patrol Settings")]
    public float patrolRadius = 20f;
    public float patrolIdleTime = 3f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public float attackDuration = 1f;
    public float rotationSpeed = 7f;
    public int attackDamage = 10;

    [Header("Vision Settings")]
    public Transform eyePoint;
    public float viewDistance = 20f;
    public float viewAngle = 90f;

    [Header("Hearing Settings")]
    public float maxHearingRange = 40f;

    [Header("Investigate Settings")]
    public float investigateDuration = 6f;
    public int lookAroundCount = 3;

    // -- Private fields -------------------------------------------------------
    private NavMeshAgent _agent;
    private float _cooldownTimer;
    private float _idleTimer;
    private float _attackTimer;
    private bool _damageAppliedThisAttack;

    private Vector3 _patrolPoint;
    private bool _isPatrolling;
    private bool _isIdle;
    private bool _isAttacking;

    private Vector3 _investigatePos;
    private bool _hasInvestigateTarget;
    private float _investigateTimer;
    private int _looksDone;
    private float _lookTimer;
    private Quaternion _lookTarget;
    private bool _isLooking;

    private Vector3 _lastKnownPlayerPos;
    private bool _hasLastKnownPos;
    private float _chaseMemoryTime = 5f;
    private float _chaseMemoryTimer;
    private bool _playerVisible;

    private enum State { Patrol, Chase, Attack, Investigate, ChaseLastKnown }
    private State _state;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponent<Animator>();
        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<PlayerHP>();
    }

    void OnEnable() => Sounds.Register(this);
    void OnDisable() => Sounds.Unregister(this);

    void Start()
    {
        _cooldownTimer = 0f;
        SetNewPatrolPoint();
        _state = State.Patrol;
    }

    void Update()
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        if (player == null) return;

        _playerVisible = CanSeePlayer();
        float dist = Vector3.Distance(transform.position, player.position);

        // -- Attack handling -------------------------------------------------------
        if (_isAttacking)
        {
            _attackTimer -= Time.deltaTime;
            FaceTarget(player.position);
            UpdateAnimation();

            if (_attackTimer <= 0f && !_damageAppliedThisAttack)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackRange && playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                    Debug.Log($"[EnemyAI] Attack finished! Dealt {attackDamage} damage. Player HP: {playerHealth.CurrentHP()}");
                }
                else
                {
                    Debug.Log("[EnemyAI] Attack finished but player out of range.");
                }

                _damageAppliedThisAttack = true;
                EndAttack();
            }

            return;
        }

        // -- Handle other states only if not attacking ---------------------------
        if (_playerVisible)
        {
            _lastKnownPlayerPos = player.position;
            _hasLastKnownPos = true;
            _chaseMemoryTimer = _chaseMemoryTime;

            if (dist <= attackRange && _cooldownTimer <= 0f)
                _state = State.Attack;
            else
                _state = State.Chase;

            _hasInvestigateTarget = false;
        }
        else if (_hasLastKnownPos && _chaseMemoryTimer > 0f)
        {
            _chaseMemoryTimer -= Time.deltaTime;
            _state = State.ChaseLastKnown;
        }
        else if (_hasInvestigateTarget)
            _state = State.Investigate;
        else
            _state = State.Patrol;

        switch (_state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.Chase: DoChase(); break;
            case State.ChaseLastKnown: DoChaseLastKnown(); break;
            case State.Attack: DoAttack(); break;
            case State.Investigate: DoInvestigate(); break;
        }

        UpdateAnimation();
        if (!_isAttacking && !_isLooking)
            RotateTowardsVelocity();
    }

    // -- IHear -------------------------------------------------------
    public void RespondToSound(Sound sound)
    {
        float dist = Vector3.Distance(transform.position, sound.pos);
        if (dist > sound.range || dist > maxHearingRange) return;

        // Both Interesting and Dangerous sounds trigger investigation
        if (sound.soundType == Sound.SoundType.Interesting ||
            sound.soundType == Sound.SoundType.Dangerous)
        {
            StartInvestigation(sound.pos);
        }
    }

    // -- Patrol -------------------------------------------------------
    void DoPatrol()
    {
        if (_isIdle)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer >= patrolIdleTime)
            {
                SetNewPatrolPoint();
                _idleTimer = 0f;
            }
            return;
        }

        if (!_isPatrolling || Vector3.Distance(transform.position, _patrolPoint) < 1.5f)
        {
            _isIdle = true;
            _isPatrolling = false;
            _agent.ResetPath();
        }
    }

    void SetNewPatrolPoint()
    {
        Vector3 dir = Random.insideUnitSphere * patrolRadius + transform.position;
        if (NavMesh.SamplePosition(dir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            _patrolPoint = hit.position;
            _agent.SetDestination(_patrolPoint);
            _isPatrolling = true;
            _isIdle = false;
        }
    }

    // -- Chase -------------------------------------------------------
    void DoChase()
    {
        _isIdle = false;
        _isPatrolling = false;
        if (_agent.isOnNavMesh && player != null)
            _agent.SetDestination(player.position);
    }

    void DoChaseLastKnown()
    {
        _isIdle = false;
        _isPatrolling = false;

        if (_agent.isOnNavMesh)
            _agent.SetDestination(_lastKnownPlayerPos);

        float distToLastKnown = Vector3.Distance(transform.position, _lastKnownPlayerPos);
        if (distToLastKnown < 1.5f && !_playerVisible)
        {
            _hasLastKnownPos = false;
            _chaseMemoryTimer = 0f;
            _state = State.Patrol;
            SetNewPatrolPoint();
        }
    }

    // -- Attack -------------------------------------------------------
    void DoAttack()
    {
        if (_isAttacking) return;

        _isAttacking = true;
        _cooldownTimer = attackCooldown;
        _attackTimer = attackDuration;
        _damageAppliedThisAttack = false;

        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");
    }

    public void EndAttack()
    {
        _isAttacking = false;
        _attackTimer = 0f;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        _playerVisible = CanSeePlayer();

        if (_playerVisible)
        {
            if (distanceToPlayer <= attackRange && _cooldownTimer <= 0f)
                _state = State.Attack;
            else
                _state = State.Chase;
        }
        else if (_hasInvestigateTarget)
            _state = State.Investigate;
        else
        {
            _state = State.Patrol;
            SetNewPatrolPoint();
        }
    }

    public void CancelAttack()
    {
        if (!_isAttacking) return;
        _isAttacking = false;
        _attackTimer = 0f;
        animator.ResetTrigger("Attack");
        if (_agent.isOnNavMesh && player != null)
            _agent.SetDestination(player.position);
        _state = State.Chase;
    }

    // -- Investigate -------------------------------------------------------
    void StartInvestigation(Vector3 pos)
    {
        if (_isAttacking || _playerVisible) return;

        _investigatePos = pos;
        _hasInvestigateTarget = true;
        _investigateTimer = investigateDuration;
        _looksDone = 0;
        _isLooking = false;
        _isIdle = false;
        _isPatrolling = false;

        _state = State.Investigate;

        if (_agent.isOnNavMesh)
            _agent.SetDestination(_investigatePos);
    }

    void DoInvestigate()
    {
        if (!_hasInvestigateTarget) { _state = State.Patrol; return; }

        _investigateTimer -= Time.deltaTime;
        float distToTarget = Vector3.Distance(transform.position, _investigatePos);

        if (distToTarget > 1.5f)
        {
            _isLooking = false;
            if (_agent.isOnNavMesh)
                _agent.SetDestination(_investigatePos);
        }
        else
        {
            _agent.ResetPath();
            DoLookAround();
        }

        if (_investigateTimer <= 0f)
            FinishInvestigation();
    }

    void DoLookAround()
    {
        if (_looksDone >= lookAroundCount) { FinishInvestigation(); return; }

        if (!_isLooking)
        {
            float angle = Random.Range(-160f, 160f);
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;
            _lookTarget = Quaternion.LookRotation(dir);
            _lookTimer = 1.3f;
            _isLooking = true;
        }

        _lookTimer -= Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, _lookTarget,
            Time.deltaTime * rotationSpeed * 0.6f);

        if (_lookTimer <= 0f)
        {
            _looksDone++;
            _isLooking = false;
        }
    }

    void FinishInvestigation()
    {
        _hasInvestigateTarget = false;
        _isLooking = false;
        _looksDone = 0;
        _state = State.Patrol;
        SetNewPatrolPoint();
    }

    // -- Helpers -------------------------------------------------------
    void FaceTarget(Vector3 target)
    {
        Vector3 flat = new Vector3(target.x, transform.position.y, target.z);
        transform.rotation = Quaternion.Slerp(transform.rotation,
            Quaternion.LookRotation(flat - transform.position),
            Time.deltaTime * rotationSpeed);
    }

    void RotateTowardsVelocity()
    {
        if (_agent.velocity.sqrMagnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(_agent.velocity.normalized),
                Time.deltaTime * rotationSpeed);
    }

    void UpdateAnimation()
    {
        bool moving = _agent.hasPath && _agent.remainingDistance > _agent.stoppingDistance;
        animator.SetBool("isWalking", moving && !_isAttacking);
    }

    bool CanSeePlayer()
    {
        if (player == null || eyePoint == null) return false;

        Vector3 dir = (player.position - eyePoint.position).normalized;
        float dist = Vector3.Distance(eyePoint.position, player.position);

        if (dist > viewDistance) return false;
        if (Vector3.Angle(eyePoint.forward, dir) > viewAngle / 2f) return false;

        if (Physics.Raycast(eyePoint.position, dir, out RaycastHit hit, viewDistance))
            if (hit.transform == player) return true;

        return false;
    }
    // --------------------- For Debugging and Visulization ----------------------
    // --- Gizmos --------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        if (eyePoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(eyePoint.position, viewDistance);
            Vector3 left = Quaternion.Euler(0, -viewAngle / 2, 0) * eyePoint.forward;
            Vector3 right = Quaternion.Euler(0, viewAngle / 2, 0) * eyePoint.forward;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(eyePoint.position, left * viewDistance);
            Gizmos.DrawRay(eyePoint.position, right * viewDistance);
        }

        Gizmos.color = new Color(0f, 1f, 0.4f, 0.1f);
        Gizmos.DrawWireSphere(transform.position, maxHearingRange);

        if (_hasInvestigateTarget)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_investigatePos, 0.4f);
            Gizmos.DrawLine(transform.position, _investigatePos);
        }

        if (_hasLastKnownPos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_lastKnownPlayerPos, 0.4f);
            Gizmos.DrawLine(transform.position, _lastKnownPlayerPos);
        }
    }
}