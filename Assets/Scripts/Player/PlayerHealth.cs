using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private int shotsToDie = 3;
    [SerializeField] private string firstLevelScene = "Nivel1";

    [Header("Game over (lo maneja otro script)")]
    [SerializeField] private UnityEvent onDiedUnityEvent;

    private float health;
    private bool dead;

    public float Current => playerStats.currentHealth;
    public float Max => playerStats.maxHealth;
    public float Percent =>
    playerStats.maxHealth > 0f
    ? playerStats.currentHealth / playerStats.maxHealth
    : 0f;
    public bool IsDead => dead;

    public event System.Action<float, float> OnHealthChanged;
    public event System.Action OnDied;

    void Awake()
    {
        bool isFirstLevel = SceneManager.GetActiveScene().name == firstLevelScene;
        bool wasDead = playerStats.currentHealth <= 0f;
        if (isFirstLevel || wasDead)
            playerStats.currentHealth = playerStats.maxHealth;
    }

    void Start()
    {
        Debug.Log($"Vida inicial: {playerStats.currentHealth}/{playerStats.maxHealth}");
        OnHealthChanged?.Invoke(playerStats.currentHealth, playerStats.maxHealth);
    }

    public void TakeShot(float damage = -1f)
    {
        if (damage >= 0f)
        {
            TakeDamage(damage);
        }
        else
        {
            TakeDamage(playerStats.maxHealth / Mathf.Max(1, shotsToDie) + 0.01f);
        }
    }
    public void TakeDamage(float amount)
    {
        if (dead) return;

        playerStats.currentHealth =
            Mathf.Max(0f, playerStats.currentHealth - amount);

        Debug.Log(
            $"Player recibió {amount} de daño. Vida: " +
            $"{playerStats.currentHealth}/{playerStats.maxHealth}"
        );

        OnHealthChanged?.Invoke(
            playerStats.currentHealth,
            playerStats.maxHealth
        );

        if (playerStats.currentHealth <= 0f)
        {
            GameAudio.Play2D("player_die", 1f);
            Die();
        }
        else
        {
            GameAudio.Play2D("player_hurt", 0.9f);
        }
    }

    public void Heal(float amount)
    {
        if (dead) return;

        playerStats.currentHealth =
            Mathf.Min(
                playerStats.maxHealth,
                playerStats.currentHealth + amount
            );

        OnHealthChanged?.Invoke(
            playerStats.currentHealth,
            playerStats.maxHealth
        );
    }

    void Die()
    {
        dead = true;
        Debug.Log("El player murió.");

        PlayerWeapon weapon = GetComponent<PlayerWeapon>();
        if (weapon != null) weapon.enabled = false;

        OnDied?.Invoke();
        onDiedUnityEvent?.Invoke();
    }
}