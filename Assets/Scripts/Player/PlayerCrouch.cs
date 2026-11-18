using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

// Sigilo: mantener C para agacharse (CTRL izquierdo es el dash).
// Agachado: el jugador es más bajo, más lento y los enemigos lo ven de menos lejos.
[RequireComponent(typeof(CharacterController))]
public class PlayerCrouch : MonoBehaviour
{
    [Header("Agacharse")]
    [SerializeField] float crouchHeight = 1.0f;
    [SerializeField] float crouchSpeedMultiplier = 0.5f;
    [SerializeField] float transitionSpeed = 10f;

    [Header("Sigilo")]
    // Agachado, la visión de los enemigos se multiplica por este valor (0.5 = ve la mitad)
    [SerializeField, Range(0.1f, 1f)] float crouchVisionMultiplier = 0.5f;

    public static bool IsCrouching { get; private set; }
    public static float VisionMultiplier { get; private set; } = 1f;

    CharacterController cc;
    FirstPersonController fpc;
    Transform camTarget;

    float standHeight;
    Vector3 standCenter;
    float standCamY;
    float baseMove, baseSprint;
    bool applied;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        fpc = GetComponent<FirstPersonController>();
    }

    void Start()
    {
        standHeight = cc.height;
        standCenter = cc.center;

        if (fpc != null && fpc.CinemachineCameraTarget != null)
        {
            camTarget = fpc.CinemachineCameraTarget.transform;
            standCamY = camTarget.localPosition.y;
        }
    }

    void Update()
    {
        // Se agacha con C (Ctrl izquierdo quedó libre para el dash)
        bool wantCrouch = Keyboard.current != null && Keyboard.current.cKey.isPressed;

        // Si quiere pararse pero hay techo encima, sigue agachado
        if (!wantCrouch && IsCrouching && !CanStandUp()) wantCrouch = true;

        if (wantCrouch != IsCrouching) SetCrouch(wantCrouch);

        // Transición suave de altura y cámara
        float targetH = IsCrouching ? crouchHeight : standHeight;
        float newH = Mathf.Lerp(cc.height, targetH, Time.deltaTime * transitionSpeed);
        cc.height = newH;
        cc.center = new Vector3(standCenter.x, standCenter.y - (standHeight - newH) * 0.5f, standCenter.z);

        if (camTarget != null)
        {
            float targetY = standCamY - (IsCrouching ? (standHeight - crouchHeight) : 0f);
            Vector3 p = camTarget.localPosition;
            p.y = Mathf.Lerp(p.y, targetY, Time.deltaTime * transitionSpeed);
            camTarget.localPosition = p;
        }
    }

    void SetCrouch(bool crouch)
    {
        IsCrouching = crouch;
        VisionMultiplier = crouch ? crouchVisionMultiplier : 1f;

        if (fpc == null) return;

        if (crouch && !applied)
        {
            baseMove = fpc.MoveSpeed;
            baseSprint = fpc.SprintSpeed;
            fpc.MoveSpeed = baseMove * crouchSpeedMultiplier;
            fpc.SprintSpeed = baseSprint * crouchSpeedMultiplier;
            applied = true;
        }
        else if (!crouch && applied)
        {
            fpc.MoveSpeed = baseMove;
            fpc.SprintSpeed = baseSprint;
            applied = false;
        }
    }

    bool CanStandUp()
    {
        float extra = standHeight - cc.height;
        if (extra < 0.05f) return true;
        Vector3 origin = transform.position + Vector3.up * (cc.height - cc.radius);
        return !Physics.SphereCast(origin, cc.radius * 0.9f, Vector3.up, out _, extra + 0.05f,
                                   ~0, QueryTriggerInteraction.Ignore);
    }

    void OnDisable()
    {
        if (applied && fpc != null)
        {
            fpc.MoveSpeed = baseMove;
            fpc.SprintSpeed = baseSprint;
            applied = false;
        }
        IsCrouching = false;
        VisionMultiplier = 1f;
    }
}
