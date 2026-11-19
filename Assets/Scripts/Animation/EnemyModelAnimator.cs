using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;

public class EnemyModelAnimator : MonoBehaviour
{
    public enum Kind { Melee, Ranged, Boss }

    [SerializeField] private Kind kind = Kind.Melee;

    [Header("Modelo")]
    [SerializeField] private float modelScale = 1f;
    [Tooltip("Si el modelo mira hacia atrás, poné 180.")]
    [SerializeField] private float yaw = 0f;
    [SerializeField] private Color accentColor = new Color(0.1f, 0.85f, 0.95f);

    [Header("Animación")]
    [Tooltip("Velocidad (m/s) a la que el clip de caminar se ve natural.")]
    [SerializeField] private float walkRefSpeed = 1.6f;
    [Tooltip("Velocidad (m/s) a la que el clip de correr se ve natural.")]
    [SerializeField] private float runRefSpeed = 4.5f;
    [Tooltip("Por encima de esta velocidad usa el clip de correr.")]
    [SerializeField] private float runThreshold = 2.4f;
    [Tooltip("En qué punto del clip de ataque (0 a 1) cae el golpe. Se sincroniza con el attackWindup del enemigo.")]
    [SerializeField, Range(0.1f, 0.9f)] private float attackImpact = 0.5f;
    [SerializeField] private float corpseSeconds = 6f;

    public void Configure(Kind k) { kind = k; }

    private CharacterAnimDriver driver;
    private Renderer rootRenderer;
    private MonoBehaviour src;
    private FieldInfo fState, fHealth, fHits, fData;
    private EnemyWeapon weapon;
    private Transform modelMuzzle;

    private Vector3 lastPos;
    private float speedSmooth;
    private float lastDamageMetric = float.NaN;
    private bool dead;
    private float attackSpeed = 1f;
    private float hideTimer = 1.5f;

    static bool warned;

    void Start()
    {
        string path = "Models/Enemy_" + kind;
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            if (!warned)
            {
                Debug.LogWarning("[Animaciones] No encuentro el modelo 'Resources/" + path + "'. " +
                                 "El archivo " + "Enemy_" + kind + ".fbx tiene que estar dentro de una carpeta Resources/Models " +
                                 "(ej: Assets/Scripts/Resources/Models). Los enemigos siguen como cápsulas.");
                warned = true;
            }
            Destroy(this);
            return;
        }

        FindSource();

        rootRenderer = GetComponent<Renderer>();

        float footY = -1f;
        CapsuleCollider cc = GetComponent<CapsuleCollider>();
        if (cc != null) footY = cc.center.y - cc.height * 0.5f;

        GameObject model = Instantiate(prefab, transform);
        model.name = "Modelo3D";
        model.transform.localPosition = new Vector3(0f, footY, 0f);
        model.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        model.transform.localScale = Vector3.one * modelScale;

        driver = model.AddComponent<CharacterAnimDriver>();
        bool ok = driver.Setup("Models/Enemy_" + kind,
            new[] { "Death", "Hit", "Attack" },
            new[] { "Attack" },
            new[] { "Death" });

        if (!ok)
        {
            Debug.LogWarning("[Animaciones] " + name + ": falló la carga de clips. Sigue como cápsula.");
            Destroy(model);
            Destroy(this);
            return;
        }
        Debug.Log("[Animaciones] " + name + ": modelo 3D puesto (" + kind + ")");

        driver.FixImportScale(2.1f);

        Color body = (rootRenderer != null && rootRenderer.sharedMaterial != null) ? rootRenderer.sharedMaterial.color : Color.red;
        driver.ApplyMaterials(body, accentColor);

        HideOldVisuals();

        EnemyData data = fData != null ? fData.GetValue(src) as EnemyData : null;
        if (data != null && data.attackWindup > 0.05f && kind != Kind.Ranged)
        {
            float impactTime = driver.ClipLength("Attack") * attackImpact;
            attackSpeed = Mathf.Clamp(impactTime / data.attackWindup, 0.6f, 3f);
        }

        weapon = GetComponent<EnemyWeapon>();
        modelMuzzle = FindDeep(model.transform, "Muzzle");

        lastPos = transform.position;
    }

    void FindSource()
    {
        MonoBehaviour[] candidates = { GetComponent<EnemyMelee>(), GetComponent<EnemyRanged>(), GetComponent<EnemyBoss>() };
        foreach (MonoBehaviour mb in candidates)
        {
            if (mb == null) continue;
            FieldInfo fs = FindField(mb.GetType(), "state");
            if (fs == null || !fs.FieldType.IsEnum) continue;

            src = mb;
            fState = fs;
            fHealth = FindField(mb.GetType(), "health");
            fHits = FindField(mb.GetType(), "hits");
            fData = FindField(mb.GetType(), "data");
            return;
        }
    }

    static FieldInfo FindField(Type t, string fieldName)
    {
        const BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        while (t != null && t != typeof(MonoBehaviour))
        {
            FieldInfo fi = t.GetField(fieldName, f);
            if (fi != null) return fi;
            t = t.BaseType;
        }
        return null;
    }

    void Update()
    {
        if (driver == null || dead) return;

        if (hideTimer > 0f)
        {
            hideTimer -= Time.unscaledDeltaTime;
            HideOldVisuals();
        }

        string st = (src != null && fState != null) ? fState.GetValue(src).ToString() : "";

        if (st == "Dead")
        {
            Die();
            return;
        }

        float metric = float.NaN;
        if (fHealth != null && fHealth.FieldType == typeof(float)) metric = -(float)fHealth.GetValue(src);
        else if (fHits != null && fHits.FieldType == typeof(int)) metric = (int)fHits.GetValue(src);

        if (!float.IsNaN(metric))
        {
            if (!float.IsNaN(lastDamageMetric) && metric > lastDamageMetric + 0.001f)
            {
                driver.Play("Hit", 1f);
                driver.Flash();
            }
            lastDamageMetric = metric;
        }

        float dt = Time.deltaTime;
        if (dt > 0f)
        {
            Vector3 d = transform.position - lastPos;
            d.y = 0f;
            float inst = d.magnitude / dt;
            speedSmooth = Mathf.Lerp(speedSmooth, inst, 1f - Mathf.Exp(-12f * dt));
            lastPos = transform.position;
        }

        bool attacking = st == "Attack" || st == "Alert";

        float move = 0f;
        float playback = 1f;
        if (speedSmooth > 0.15f)
        {
            bool running = speedSmooth >= runThreshold;
            move = running ? 2f : 1f;
            float refSpeed = running ? runRefSpeed : walkRefSpeed;
            playback = Mathf.Clamp(speedSmooth / refSpeed, 0.5f, 1.7f);
        }
        if (attacking && st == "Attack") move = 0f;

        driver.SetMove(move, playback);
        driver.SetHeld("Attack", attacking, kind == Kind.Ranged ? 1f : attackSpeed);
    }

    void LateUpdate()
    {
        if (driver == null) return;

        if (!dead && rootRenderer != null && rootRenderer.sharedMaterial != null) driver.SetBodyColor(rootRenderer.sharedMaterial.color);

        if (!dead && weapon != null && weapon.Muzzle != null && modelMuzzle != null)
            weapon.Muzzle.position = modelMuzzle.position;
    }

    void Die()
    {
        dead = true;

        driver.transform.SetParent(null, true);
        driver.Play("Death", 1f);
        driver.DestroyAfter(corpseSeconds);
        driver.SetMove(0f);
        driver.SetHeld("Attack", false);
        driver = null;
        enabled = false;
    }

    void HideOldVisuals()
    {
        if (rootRenderer != null && rootRenderer.enabled) rootRenderer.enabled = false;

        Transform eye = transform.Find("Ojo");
        if (eye != null) eye.gameObject.SetActive(false);

        Transform arma = transform.Find("Arma");
        if (arma != null)
            foreach (Renderer r in arma.GetComponentsInChildren<Renderer>())
                r.enabled = false;
    }

    static Transform FindDeep(Transform root, string childName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == childName) return t;
        return null;
    }
}
