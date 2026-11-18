using UnityEngine;

// ScriptableObject con los datos de un arma. La lógica (PlayerWeapon) queda limpia
// y todos los números, colores y sonidos se editan desde el asset, sin tocar código.
// Crear uno nuevo: clic derecho en Project > Create > Game > Weapon Data
[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identificación")]
    public string displayName = "Pistola";

    [Header("Disparo")]
    public float damage = 25f;
    public float bulletSpeed = 40f;
    public float fireCooldown = 0.35f;

    [Header("Munición")]
    public int magazineSize = 5;
    public float reloadTime = 1.2f;

    [Header("Recursos visuales")]
    public Color bulletColor = new Color(0.2f, 0.9f, 1f);
    public Color stickColor = new Color(0.12f, 0.12f, 0.12f);
    public Color tipColor = new Color(0.2f, 0.9f, 1f);

    [Header("Sonido (nombre en Resources/Audio/SFX)")]
    public string shotSound = "player_shot";
}
