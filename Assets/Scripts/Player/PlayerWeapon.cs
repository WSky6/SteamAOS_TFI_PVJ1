using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Datos del arma (ScriptableObject)")]
    [SerializeField] private WeaponData data;

    [Header("Disparo")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float bulletSpeed = 40f;
    [SerializeField] private float fireCooldown = 0.35f;
    [SerializeField] private Color bulletColor = new Color(0.2f, 0.9f, 1f);

    [Header("Munición")]
    [SerializeField] private int magazineSize = 5;
    [SerializeField] private float reloadTime = 1.2f;

    [Header("Animación de recarga")]
    [SerializeField] private float reloadDip = 0.3f;
    [SerializeField] private float reloadTilt = 55f;

    [Header("Arma (palito)")]
    [SerializeField] private Vector3 stickPosition = new Vector3(0.3f, -0.28f, 0.55f);
    [SerializeField] private float stickLength = 0.7f;
    [SerializeField] private float stickThickness = 0.08f;
    [SerializeField] private Color stickColor = new Color(0.12f, 0.12f, 0.12f);
    [SerializeField] private Color tipColor = new Color(0.2f, 0.9f, 1f);

    [Header("Puntero")]
    [SerializeField] private bool showCrosshair = true;
    [SerializeField] private float crosshairSize = 7f;
    [SerializeField] private float crosshairGap = 4f;
    [SerializeField] private float crosshairThickness = 2f;

    static readonly RaycastHit[] hits = new RaycastHit[16];

    private Camera cam;
    private Transform pivot;
    private Transform muzzle;
    private float nextFire;
    private float recoilUntil;
    private bool warnedNoCamera;

    private int ammo;
    private bool reloading;
    private float reloadStart;

    public int Ammo => ammo;
    public int MagazineSize => magazineSize;
    public bool IsReloading => reloading;
    public float ReloadProgress => reloading
        ? Mathf.Clamp01((Time.time - reloadStart) / Mathf.Max(0.1f, reloadTime))
        : 1f;

    public event System.Action<int, int> OnAmmoChanged;
    public event System.Action OnReloadStarted;
    public event System.Action OnReloadFinished;

    void ApplyData()
    {
        if (data == null) data = Resources.Load<WeaponData>("Weapons/PlayerPistol");
        if (data == null) return;

        damage = data.damage;
        bulletSpeed = data.bulletSpeed;
        fireCooldown = data.fireCooldown;
        magazineSize = data.magazineSize;
        reloadTime = data.reloadTime;
        bulletColor = data.bulletColor;
        stickColor = data.stickColor;
        tipColor = data.tipColor;
    }

    void Awake()
    {
        ApplyData();
        ammo = magazineSize;
    }

    void Start()
    {
        OnAmmoChanged?.Invoke(ammo, magazineSize);
    }

    void SetAmmo(int value)
    {
        ammo = Mathf.Clamp(value, 0, magazineSize);
        OnAmmoChanged?.Invoke(ammo, magazineSize);
    }

    void StartReload()
    {
        if (reloading) return;

        reloading = true;
        reloadStart = Time.time;
        OnReloadStarted?.Invoke();
        GameAudio.Play2D("reload_start", 0.8f);
    }

    void FinishReload()
    {
        reloading = false;
        SetAmmo(magazineSize);
        OnReloadFinished?.Invoke();
        GameAudio.Play2D("reload_end", 0.8f);
    }

    void Update()
    {
        if (cam == null || pivot == null)
        {
            cam = FindCamera();

            if (cam == null)
            {
                if (!warnedNoCamera)
                {
                    Debug.LogWarning("PlayerWeapon: no encuentro ninguna cámara activa.");
                    warnedNoCamera = true;
                }
                return;
            }

            BuildStick();
        }

        if (Time.timeScale == 0f) return;

        float dip = 0f;

        if (reloading)
        {
            float p = ReloadProgress;

            if (p >= 1f) FinishReload();
            else dip = Mathf.Sin(p * Mathf.PI);
        }
        else
        {
            bool reloadKey = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            if (reloadKey && ammo < magazineSize) StartReload();

            if (ammo <= 0 && Time.time >= nextFire) StartReload();
        }

        float recoil = Time.time < recoilUntil ? -0.06f : 0f;
        Vector3 targetPos = stickPosition + new Vector3(0f, -reloadDip * dip, recoil);
        Quaternion targetRot = Quaternion.Euler(reloadTilt * dip, 0f, 0f);

        pivot.localPosition = Vector3.Lerp(pivot.localPosition, targetPos, 25f * Time.deltaTime);
        pivot.localRotation = Quaternion.Slerp(pivot.localRotation, targetRot, 25f * Time.deltaTime);

        bool click = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (click && !reloading && ammo > 0 && Time.time >= nextFire) Fire();
        else if (click && (reloading || ammo <= 0)) GameAudio.Play2D("player_empty", 0.7f);
    }

    Camera FindCamera()
    {
        if (Camera.main != null) return Camera.main;

        foreach (Camera c in Camera.allCameras)
        {
            if (c.enabled && c.gameObject.activeInHierarchy) return c;
        }
        return null;
    }

    void BuildStick()
    {
        pivot = new GameObject("ArmaPlayer").transform;
        pivot.SetParent(cam.transform, false);
        pivot.localPosition = stickPosition;

        GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stick.name = "Palito";
        Destroy(stick.GetComponent<Collider>());
        stick.transform.SetParent(pivot, false);
        stick.transform.localPosition = new Vector3(0f, 0f, stickLength * 0.5f);
        stick.transform.localScale = new Vector3(stickThickness, stickThickness, stickLength);
        stick.GetComponent<Renderer>().material.color = stickColor;

        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.name = "Punta";
        Destroy(tip.GetComponent<Collider>());
        tip.transform.SetParent(pivot, false);
        tip.transform.localPosition = new Vector3(0f, 0f, stickLength);
        tip.transform.localScale = Vector3.one * (stickThickness * 1.2f);
        tip.GetComponent<Renderer>().material.color = tipColor;

        muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(pivot, false);
        muzzle.localPosition = new Vector3(0f, 0f, stickLength + stickThickness * 0.6f);
    }

    void Fire()
    {
        nextFire = Time.time + fireCooldown;
        recoilUntil = Time.time + 0.08f;
        SetAmmo(ammo - 1);
        GameAudio.Play2D(data != null ? data.shotSound : "player_shot", 0.9f);

        Vector3 origin = cam.transform.position;
        Vector3 camDir = cam.transform.forward;

        Vector3 target = origin + camDir * 100f;
        int n = Physics.RaycastNonAlloc(origin, camDir, hits, 200f, ~0, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue;

        for (int i = 0; i < n; i++)
        {
            Transform t = hits[i].transform;
            if (t.IsChildOf(transform) || t.IsChildOf(cam.transform)) continue;
            if (hits[i].distance < best) best = hits[i].distance;
        }
        if (best < float.MaxValue) target = origin + camDir * best;

        Vector3 muzzlePos = muzzle.position;
        Vector3 dir = (target - muzzlePos).normalized;
        if (Vector3.Dot(dir, camDir) < 0.5f) dir = camDir;

        PlayerBullet.Spawn(muzzlePos, dir, bulletSpeed, damage, bulletColor, transform);
    }

    void OnGUI()
    {
        if (!showCrosshair || Time.timeScale == 0f) return;

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        Texture2D white = Texture2D.whiteTexture;

        GUI.color = new Color(0f, 0f, 0f, 0.8f);
        DrawCross(cx, cy, white, 1.5f);

        GUI.color = Color.white;
        DrawCross(cx, cy, white, 0f);

        GUI.color = Color.white;
    }

    void DrawCross(float cx, float cy, Texture2D tex, float extra)
    {
        float t = crosshairThickness + extra * 2f;
        float s = crosshairSize + extra;
        float g = crosshairGap - extra;

        GUI.DrawTexture(new Rect(cx - t * 0.5f, cy - g - s, t, s), tex);
        GUI.DrawTexture(new Rect(cx - t * 0.5f, cy + g, t, s), tex);
        GUI.DrawTexture(new Rect(cx - g - s, cy - t * 0.5f, s, t), tex);
        GUI.DrawTexture(new Rect(cx + g, cy - t * 0.5f, s, t), tex);
    }
}