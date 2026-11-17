using UnityEngine;

// Una dificultad = un nombre + un multiplicador de stats
[System.Serializable]
public struct DifficultyLevel
{
    public string dificultad;
    public float multiplicador;

    public DifficultyLevel(string dificultad, float multiplicador)
    {
        this.dificultad = dificultad;
        this.multiplicador = multiplicador;
    }
}

// Clase base: todo enemigo hereda de esta y obtiene el array [dificultad, multiplicador].
// Se puede editar el array desde el Inspector de cada enemigo.
public abstract class DifficultyScaled : MonoBehaviour
{
    const string PrefKey = "Dificultad";

    // Array heredado por todos los enemigos: [dificultad, multiplicador]
    [Header("Dificultad")]
    [SerializeField]
    protected DifficultyLevel[] niveles =
    {
        new DifficultyLevel("Facil",   0.75f),
        new DifficultyLevel("Normal",  1.00f),
        new DifficultyLevel("Dificil", 1.50f),
    };

    static int current = -1;

    // Índice de la dificultad elegida (se guarda entre partidas)
    public static int CurrentIndex
    {
        get
        {
            if (current < 0) current = PlayerPrefs.GetInt(PrefKey, 1);
            return current;
        }
    }

    // Llamar desde el menú: DifficultyScaled.SetDifficulty(0) facil, 1 normal, 2 dificil
    public static void SetDifficulty(int index)
    {
        current = Mathf.Max(0, index);
        PlayerPrefs.SetInt(PrefKey, current);
    }

    // Multiplicador que se aplica a vida, velocidad, daño y visión
    protected float Multiplier
    {
        get
        {
            if (niveles == null || niveles.Length == 0) return 1f;
            int i = Mathf.Clamp(CurrentIndex, 0, niveles.Length - 1);
            return niveles[i].multiplicador;
        }
    }
}
