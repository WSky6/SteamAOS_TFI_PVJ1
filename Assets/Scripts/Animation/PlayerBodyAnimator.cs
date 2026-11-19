using UnityEngine;
using UnityEngine.Rendering;

public class PlayerBodyAnimator : MonoBehaviour
{
    [SerializeField] private bool visibleToCamera = false;
    [SerializeField] private Color bodyColor = new Color(0.16f, 0.30f, 0.65f);
    [SerializeField] private Color accentColor = new Color(0.2f, 0.9f, 1f);
    [SerializeField] private float yaw = 0f;

    [Header("Movimiento")]
    [SerializeField] private float idleThreshold = 0.25f;
    [SerializeField] private float runThreshold = 5f;
    [SerializeField] private float walkRefSpeed = 2.2f;
    [SerializeField] private float runRefSpeed = 5f;
    [SerializeField] private float crouchScaleY = 0.65f;

    private CharacterController cc;
    private PlayerWeapon weapon;
    private PlayerHealth health;
    private CharacterAnimDriver driver;
    private GameObject model;
    private float aimUntil;
    private int lastAmmo = -1;
    private float scaleY = 1f;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Start()
    {
        GameObject prefab = Resources.Load<GameObject>("Models/Player_Body");
        if (prefab == null)
        {
            Debug.LogWarning("PlayerBodyAnimator: no encuentro Resources/Models/Player_Body. Queda la cápsula.");
            Destroy(this);
            return;
        }

        float footY = cc != null ? cc.center.y - cc.height * 0.5f : -1f;

        model = Instantiate(prefab, transform);
        model.name = "CuerpoPlayer";
        model.transform.localPosition = new Vector3(0f, footY, 0f);
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        model.transform.localScale = Vector3.one * Mathf.Clamp(cc != null ? cc.height / 2f : 1f, 0.5f, 2f);

        driver = model.AddComponent<CharacterAnimDriver>();
        bool ok = driver.Setup("Models/Player_Body",
            new[] { "Death", "Hit", "Attack" },
            new[] { "Attack" },
            new[] { "Death" });
        if (!ok) { Destroy(model); Destroy(this); return; }

        driver.ApplyMaterials(bodyColor, accentColor);

        foreach (Collider c in model.GetComponentsInChildren<Collider>()) Destroy(c);

        ApplyVisibility();
        HideCapsule();
    }

    void OnDestroy()
    {
        if (weapon != null) weapon.OnAmmoChanged -= OnAmmoChanged;
    }

    void ApplyVisibility()
    {
        ShadowCastingMode mode = visibleToCamera ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
        foreach (Renderer r in model.GetComponentsInChildren<Renderer>(true))
            r.shadowCastingMode = mode;
    }

    void HideCapsule()
    {
        foreach (MeshRenderer r in GetComponentsInChildren<MeshRenderer>(true))
        {
            if (model != null && r.transform.IsChildOf(model.transform)) continue;
            MeshFilter mf = r.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && mf.sharedMesh.name == "Capsule")
                r.enabled = false;
        }
    }

    void Update()
    {
        if (driver == null) return;

        if (weapon == null)
        {
            weapon = GetComponent<PlayerWeapon>();
            if (weapon != null)
            {
                weapon.OnAmmoChanged += OnAmmoChanged;
                lastAmmo = weapon.Ammo;
            }
        }
        if (health == null) health = GetComponent<PlayerHealth>();

        if (health != null && health.IsDead)
        {
            driver.Play("Death", 1f);
            driver.SetMove(0f);
            enabled = false;
            return;
        }

        float speed = 0f;
        if (cc != null)
        {
            Vector3 v = cc.velocity;
            v.y = 0f;
            speed = v.magnitude;
        }

        float move = 0f, playback = 1f;
        if (speed > idleThreshold)
        {
            bool running = speed >= runThreshold;
            move = running ? 2f : 1f;
            playback = Mathf.Clamp(speed / (running ? runRefSpeed : walkRefSpeed), 0.6f, 1.6f);
        }
        driver.SetMove(move, playback);
        driver.SetHeld("Attack", Time.time < aimUntil);

        float targetY = PlayerCrouch.IsCrouching ? crouchScaleY : 1f;
        scaleY = Mathf.MoveTowards(scaleY, targetY, 4f * Time.deltaTime);
        Vector3 s = model.transform.localScale;
        float baseS = s.x;
        model.transform.localScale = new Vector3(baseS, baseS * scaleY, baseS);
    }

    void OnAmmoChanged(int ammo, int magazine)
    {
        if (lastAmmo >= 0 && ammo < lastAmmo) aimUntil = Time.time + 0.5f;
        lastAmmo = ammo;
    }
}
