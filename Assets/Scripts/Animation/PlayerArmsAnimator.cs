using UnityEngine;

public class PlayerArmsAnimator : MonoBehaviour
{
    [Header("Aspecto")]
    [SerializeField] private Color sleeveColor = new Color(0.16f, 0.22f, 0.32f);
    [SerializeField] private Color accentColor = new Color(0.2f, 0.9f, 1f);
    [Tooltip("Mové los brazos respecto de la cámara (x derecha, y arriba, z adelante).")]
    [SerializeField] private Vector3 offset = Vector3.zero;

    [Header("Movimiento")]
    [SerializeField] private float idleThreshold = 0.25f;
    [Tooltip("Por encima de esta velocidad usa la animación de correr.")]
    [SerializeField] private float runThreshold = 5f;
    [SerializeField] private float walkRefSpeed = 4f;
    [SerializeField] private float runRefSpeed = 6f;

    private CharacterController cc;
    private PlayerWeapon weapon;
    private PlayerHealth health;
    private CharacterAnimDriver driver;
    private GameObject model;
    private Camera cam;
    private Transform weaponMuzzle, modelMuzzle;
    private int lastAmmo = -1;
    private float nextTry;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void OnDestroy()
    {
        if (weapon != null) weapon.OnAmmoChanged -= OnAmmoChanged;
        if (model != null) Destroy(model);
    }

    static Camera FindCamera()
    {
        if (Camera.main != null) return Camera.main;
        foreach (Camera c in Camera.allCameras)
            if (c.enabled && c.gameObject.activeInHierarchy) return c;
        return null;
    }

    void Update()
    {
        if (model == null)
        {
            if (Time.unscaledTime < nextTry) return;
            nextTry = Time.unscaledTime + 1f;

            cam = FindCamera();
            if (cam == null)
            {
                Debug.LogWarning("[Brazos] No encuentro ninguna cámara activa.");
                return;
            }
            if (!Build()) { enabled = false; return; }
        }

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

        bool dead = health != null && health.IsDead;
        if (model.activeSelf == dead) model.SetActive(!dead);
        if (dead) return;

        if (cam == null || !cam.gameObject.activeInHierarchy) cam = FindCamera();
        if (cam != null && model.transform.parent != cam.transform)
        {
            model.transform.SetParent(cam.transform, false);
            model.transform.localPosition = offset;
        }

        HideStick();

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
            playback = Mathf.Clamp(speed / (running ? runRefSpeed : walkRefSpeed), 0.6f, 1.5f);
        }
        driver.SetMove(move, playback);

        if (weapon != null && weapon.IsReloading)
            driver.Scrub("Reload", weapon.ReloadProgress);
    }

    void LateUpdate()
    {
        if (model == null || weaponMuzzle == null || modelMuzzle == null) return;
        weaponMuzzle.position = modelMuzzle.position;
    }

    bool Build()
    {
        GameObject prefab = Resources.Load<GameObject>("Models/Player_Arms");
        if (prefab == null)
        {
            Debug.LogWarning("[Brazos] No encuentro Assets/Scripts/Resources/Models/Player_Arms.fbx. Queda el palito.");
            return false;
        }

        model = Instantiate(prefab, cam.transform);
        model.name = "BrazosPlayer";
        model.transform.localPosition = offset;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        driver = model.AddComponent<CharacterAnimDriver>();
        bool ok = driver.Setup("Models/Player_Arms",
            new[] { "Reload", "Shoot" },
            new string[0],
            new string[0]);

        if (!ok)
        {
            Debug.LogWarning("[Brazos] El FBX no trae clips de animación. Revisá que se importó bien (Inspector > Animation).");
            Destroy(model);
            return false;
        }

        driver.FixImportScale(0.85f);
        driver.ApplyMaterials(sleeveColor, accentColor, false);

        Bounds b = default;
        bool has = false;
        foreach (Renderer r in model.GetComponentsInChildren<Renderer>())
        {
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        if (has)
        {
            float z = cam.transform.InverseTransformPoint(b.center).z;
            if (z < 0f)
            {
                model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                Debug.LogWarning("[Brazos] El modelo estaba al revés, lo giré 180°.");
            }
            Debug.Log($"[Brazos] Creados. Tamaño {b.size.ToString("F2")}, centro (cámara) {cam.transform.InverseTransformPoint(b.center).ToString("F2")}");
        }

        modelMuzzle = null;
        foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
            if (t.name == "Muzzle") { modelMuzzle = t; break; }

        return true;
    }

    void HideStick()
    {
        if (cam == null) return;
        Transform pivot = cam.transform.Find("ArmaPlayer");
        if (pivot == null) return;

        foreach (Renderer r in pivot.GetComponentsInChildren<Renderer>())
            if (r.enabled) r.enabled = false;

        if (weaponMuzzle == null) weaponMuzzle = pivot.Find("Muzzle");
    }

    void OnAmmoChanged(int ammo, int magazine)
    {
        if (driver != null && lastAmmo >= 0 && ammo < lastAmmo) driver.Play("Shoot", 1f);
        lastAmmo = ammo;
    }
}
