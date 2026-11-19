using UnityEngine;

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

public abstract class DifficultyScaled : MonoBehaviour
{
    const string PrefKey = "Dificultad";

    [Header("Dificultad")]
    [SerializeField]
    protected DifficultyLevel[] niveles =
    {
        new DifficultyLevel("Facil",   0.50f),
        new DifficultyLevel("Normal",  1.00f),
        new DifficultyLevel("Dificil", 1.50f),
    };

    static int current = -1;

    public static int CurrentIndex
    {
        get
        {
            if (current < 0) current = PlayerPrefs.GetInt(PrefKey, 1);
            return current;
        }
    }

    public static void SetDifficulty(int index)
    {
        current = Mathf.Max(0, index);
        PlayerPrefs.SetInt(PrefKey, current);
    }

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
