using UnityEngine;

public class PlayerDeathZones : MonoBehaviour
{
    [SerializeField] private string deathLayerName = "DeathPlain";

    [Header("Seguro contra caídas infinitas (apagado por defecto)")]
    [SerializeField] private bool useFallLimit = false;
    [SerializeField] private float fallLimitY = -100f;

    private int deathLayer = -1;
    private PlayerHealth health;
    private CharacterController cc;
    private bool warnedLayer;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        deathLayer = LayerMask.NameToLayer(deathLayerName);
    }

    void Update()
    {
        if (health == null) health = GetComponent<PlayerHealth>();
        if (health == null || health.IsDead) return;

        if (deathLayer < 0)
        {
            if (!warnedLayer)
            {
                Debug.LogWarning($"PlayerDeathZones: no existe el layer '{deathLayerName}'. Crealo en Project Settings > Tags and Layers.");
                warnedLayer = true;
            }
        }
        else
        {
            Vector3 origin = FeetPosition() + Vector3.up * 0.1f;
            if (Physics.Raycast(origin, Vector3.down, 0.6f, 1 << deathLayer, QueryTriggerInteraction.Collide))
            {
                Kill("pisó una zona de muerte");
                return;
            }
        }

        if (useFallLimit && transform.position.y < fallLimitY)
            Kill("cayó al vacío");
    }

    Vector3 FeetPosition()
    {
        if (cc != null)
        {
            Bounds b = cc.bounds;
            return new Vector3(b.center.x, b.min.y, b.center.z);
        }
        return transform.position;
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (deathLayer >= 0 && hit.gameObject.layer == deathLayer)
            Kill("tocó una zona de muerte");
    }

    void OnTriggerEnter(Collider other)
    {
        if (deathLayer >= 0 && other.gameObject.layer == deathLayer)
            Kill("cayó a una zona de muerte");
    }

    void Kill(string reason)
    {
        if (health == null || health.IsDead) return;

        Debug.Log($"Player murió: {reason}");
        health.TakeDamage(999999f);
    }
}