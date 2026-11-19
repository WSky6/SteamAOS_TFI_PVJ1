using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public enum EnemyType { Melee, Distancia, Boss }

    public EnemyType type = EnemyType.Melee;

    public float maxHealth = 50f;
    public int hitsToKill = 5;

    public float moveSpeed = 5f;
    public float acceleration = 30f;
    public float angularSpeed = 540f;
    public float turnSpeed = 180f;

    public float maxJumpHeight = 1.8f;
    public float jumpDistance = 8f;
    public float jumpTriggerDistance = 1.5f;
    public float jumpSpeed = 9f;
    public float jumpArcHeight = 3.5f;
    public float jumpCooldown = 1.5f;

    public float visionRange = 12f;
    public float visionAngle = 120f;

    public float headScanAngle = 60f;
    public float headScanSpeed = 1f;

    public float allyAlertRadius = 10f;
    public float alertMemoryTime = 10f;

    public float attackRange = 1.8f;
    [Range(10f, 360f)] public float attackAngle = 90f;
    public bool instantKill = true;
    public float attackDamage = 10f;
    public float attackWindup = 0.5f;
    public float attackCooldown = 1.5f;

    public GameObject bulletPrefab;
    public float bulletSpeed = 14f;
    public float bulletDamage = 25f;
    public float shootInterval = 3f;

    public int burstShots = 3;
    public float burstShotDelay = 0.25f;
    public float burstInterval = 4f;
    public float shootMinDistance = 8f;

    public int points = 10;
}