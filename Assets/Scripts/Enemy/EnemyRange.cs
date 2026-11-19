using UnityEngine;
using UnityEngine.AI;

public class EnemyRanged : DifficultyScaled, IDamageable
{
    private enum State { Idle, Patrol, Alert, Dead }

    float VisionRange { get { return data.visionRange * EnemyUtils.StealthMultiplier; } }

    [SerializeField] private EnemyData data;

    [Header("Paredes / Visión")]
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private float eyeOffset = 0.5f;

    [Header("Visual")]
    [SerializeField] private Color bodyColor = new Color(0.6f, 0f, 0.9f);
    [SerializeField] private Color bulletColor = new Color(1f, 0.9f, 0f);
    [SerializeField] private Color coneColor = new Color(0.7f, 0f, 1f, 0.22f);
    [SerializeField] private Color alertConeColor = new Color(1f, 0.55f, 0f, 0.30f);
    [SerializeField] private float coneYOffset = -0.95f;
    [SerializeField] private int coneSegments = 40;

    [Header("Patrulla")]
    [Tooltip("Puntos de patrulla en orden. Deben estar sobre el NavMesh y NO ser hijos del enemigo. Vacío = se queda quieto como antes.")]
    [SerializeField] private Transform[] patrolPoints;
    [Tooltip("Desactivado: circuito en bucle (A-B-C-A). Activado: va y vuelve (A-B-C-B-A).")]
    [SerializeField] private bool pingPong = false;
    [SerializeField] private float waitAtPoint = 1.5f;
    [SerializeField, Range(0.1f, 1f)] private float patrolSpeedMultiplier = 0.5f;
    [SerializeField] private float arriveThreshold = 0.5f;

    private Transform player;
    private float health;
    private State state = State.Idle;

    private Quaternion baseRotation;
    private float scanPhase;
    private float nextShot;
    private float alertUntil;
    private Vector3 lastSeenPos;

    private NavMeshAgent agent;
    private int patrolIndex;
    private int patrolDir = 1;
    private bool waiting;
    private float waitStart, waitUntil;

    private bool HasPatrol => agent != null && patrolPoints != null && patrolPoints.Length > 0;

    private GameObject coneObj;
    private Mesh coneMesh;
    private Vector3[] coneVerts;

    void Awake()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            agent = gameObject.AddComponent<NavMeshAgent>();
            Debug.LogWarning($"{name}: no tenía NavMeshAgent, se agregó uno automáticamente. Conviene agregarlo al prefab.");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Start()
    {
        health = (data.maxHealth * Multiplier);
        baseRotation = transform.rotation;
        scanPhase = Random.Range(0f, 6.28f);

        PaintBody();
        EnemyUtils.CreateEye(transform);
        coneObj = EnemyUtils.CreateFan(transform, "VisionCone", coneYOffset, coneColor,
                                       coneSegments, out coneMesh, out coneVerts);

        player = EnemyUtils.Player;
        if (player == null) Debug.LogWarning("EnemyRanged: no encuentro ningún objeto con el tag Player");

        if (HasPatrol)
        {
            SetupAgent();
            StartPatrol();
        }
    }

    void Update()
    {
        if (state == State.Dead) return;

        if (player == null)
        {
            player = EnemyUtils.Player;
            if (player == null) return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle:
                ScanHead();
                if (CanSeePlayer(dist, true)) EnterAlert();
                break;

            case State.Patrol:
                if (CanSeePlayer(dist, true)) EnterAlert();
                else UpdatePatrol();
                break;

            case State.Alert:
                bool sees = CanSeePlayer(dist, false);

                if (sees)
                {
                    lastSeenPos = player.position;
                    alertUntil = Time.time + data.alertMemoryTime;
                    TurnTowards(lastSeenPos);

                    if (Time.time >= nextShot && AngleTo(lastSeenPos) < 20f)
                        Shoot();
                }
                else
                {
                    TurnTowards(lastSeenPos);

                    if (Time.time > alertUntil) LeaveAlert();
                }
                break;
        }
    }

    void LateUpdate()
    {
        if (state == State.Dead || coneObj == null) return;

        EnemyUtils.UpdateFan(transform, coneMesh, coneVerts, data.visionAngle, VisionRange,
                             coneSegments, eyeOffset, obstacleMask);
    }

    void EnterAlert()
    {
        GameAudio.Play("ranged_alert", transform.position, 0.9f);
        state = State.Alert;
        lastSeenPos = player.position;
        alertUntil = Time.time + data.alertMemoryTime;
        nextShot = Time.time + 1f;
        EnemyUtils.SetFanColor(coneMesh, alertConeColor);

        waiting = false;
        if (HasPatrol && agent.enabled)
        {
            agent.updateRotation = false;
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }
        }
    }

    void LeaveAlert()
    {
        baseRotation = transform.rotation;
        EnemyUtils.SetFanColor(coneMesh, coneColor);

        if (HasPatrol) StartPatrol();
        else state = State.Idle;
    }

    void SetupAgent()
    {
        agent.speed = PatrolSpeed();
        if (data.acceleration > 0f) agent.acceleration = data.acceleration;
        if (data.angularSpeed > 0f) agent.angularSpeed = data.angularSpeed;
        agent.stoppingDistance = 0.1f;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
                agent.Warp(navHit.position);
            else
                Debug.LogWarning($"{name}: NO está sobre el NavMesh. Movelo sobre la zona celeste.");
        }
    }

    float PatrolSpeed()
    {
        float baseSpeed = (data.moveSpeed * Multiplier) > 0f ? (data.moveSpeed * Multiplier) : 3.5f;
        return baseSpeed * patrolSpeedMultiplier;
    }

    void StartPatrol()
    {
        state = State.Patrol;
        waiting = false;

        agent.updateRotation = true;
        agent.speed = PatrolSpeed();
        if (agent.isOnNavMesh) agent.isStopped = false;

        GoToPatrolPoint();
    }

    void UpdatePatrol()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        if (waiting)
        {
            ScanHeadAt((Time.time - waitStart) * data.headScanSpeed);

            if (Time.time >= waitUntil)
            {
                waiting = false;
                agent.isStopped = false;
                AdvancePatrolIndex();
                GoToPatrolPoint();
            }
            return;
        }

        if (agent.pathPending) return;

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            AdvancePatrolIndex();
            GoToPatrolPoint();
            return;
        }

        if (agent.remainingDistance <= arriveThreshold)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            baseRotation = transform.rotation;
            waiting = true;
            waitStart = Time.time;
            waitUntil = Time.time + waitAtPoint;
        }
    }

    void GoToPatrolPoint()
    {
        if (!HasPatrol || !agent.enabled || !agent.isOnNavMesh) return;

        Vector3 dest = patrolPoints[patrolIndex].position;
        if (NavMesh.SamplePosition(dest, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            dest = hit.position;

        agent.SetDestination(dest);
    }

    void AdvancePatrolIndex()
    {
        if (patrolPoints.Length <= 1) return;

        if (pingPong)
        {
            int next = patrolIndex + patrolDir;
            if (next < 0 || next >= patrolPoints.Length)
            {
                patrolDir = -patrolDir;
                next = patrolIndex + patrolDir;
            }
            patrolIndex = next;
        }
        else
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }
    }

    bool CanSeePlayer(float dist, bool useAngle)
    {
        if (dist > VisionRange) return false;

        if (useAngle)
        {
            Vector3 flat = player.position - transform.position;
            flat.y = 0f;
            if (Vector3.Angle(transform.forward, flat) > data.visionAngle * 0.5f)
                return false;
        }

        Vector3 origin = transform.position + Vector3.up * eyeOffset;
        return !EnemyUtils.SegmentBlocked(origin, EnemyUtils.PlayerCenter(), transform, obstacleMask);
    }

    void ScanHead()
    {
        ScanHeadAt(Time.time * data.headScanSpeed + scanPhase);
    }

    void ScanHeadAt(float phase)
    {
        if (data.headScanAngle <= 0f) return;

        float angle = Mathf.Sin(phase) * data.headScanAngle;
        transform.rotation = baseRotation * Quaternion.Euler(0f, angle, 0f);
    }

    void TurnTowards(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion look = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, look, data.turnSpeed * Time.deltaTime);
    }

    float AngleTo(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return 0f;
        return Vector3.Angle(transform.forward, dir);
    }

    void Shoot()
    {
        nextShot = Time.time + data.shootInterval;
        GameAudio.Play("ranged_shot", transform.position, 1f);

        EnemyWeapon weapon = GetComponent<EnemyWeapon>();
        Vector3 muzzle = (weapon != null && weapon.Muzzle != null)
            ? weapon.Muzzle.position
            : transform.position + Vector3.up * eyeOffset + transform.forward * 0.8f;

        Vector3 dir = (EnemyUtils.PlayerCenter() - muzzle).normalized;

        EnemyBullet.Spawn(data.bulletPrefab, muzzle, dir, data.bulletSpeed, (data.bulletDamage * Multiplier),
                          bulletColor, transform);
    }

    void PaintBody()
    {
        Renderer r = GetComponent<Renderer>();
        if (r == null) r = GetComponentInChildren<Renderer>();
        if (r != null) r.material.color = bodyColor;
    }

    public void TakeDamage(float amount)
    {
        if (state == State.Dead) return;

        health -= amount;
        if (health <= 0f) Die();
        else GameAudio.Play("ranged_hurt", transform.position, 0.9f);
    }

    void Die()
    {
        state = State.Dead;
        GameAudio.Play("ranged_die", transform.position, 1f);
        if (coneObj != null) coneObj.SetActive(false);
        if (agent != null && agent.enabled) agent.enabled = false;

        EnemyEvents.RaiseDied(data.points, transform.position);
        Debug.Log($"{name} murió (+{data.points} puntos)");
        Destroy(gameObject, 0.5f);
    }

    [ContextMenu("Test: recibir golpe")]
    void TestHit() { TakeDamage((data.maxHealth * Multiplier)); }

    void DrawPatrolGizmos()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.green;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            Gizmos.DrawWireSphere(patrolPoints[i].position, 0.35f);

            int next = i + 1;
            if (next >= patrolPoints.Length)
            {
                if (pingPong) break;
                next = 0;
            }
            if (patrolPoints[next] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
        }
    }

    void OnDrawGizmosSelected()
    {
        DrawPatrolGizmos();

        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, VisionRange);

        Gizmos.color = Color.cyan;
        Vector3 l = Quaternion.Euler(0, -data.visionAngle * 0.5f, 0) * transform.forward;
        Vector3 r = Quaternion.Euler(0, data.visionAngle * 0.5f, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, l * VisionRange);
        Gizmos.DrawRay(transform.position, r * VisionRange);
    }
}