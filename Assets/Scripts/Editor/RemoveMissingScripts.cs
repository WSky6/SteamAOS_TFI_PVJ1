using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Herramienta de editor: borra los componentes "Missing (Mono Script)" de los objetos seleccionados
// y de todos sus hijos. Uso: seleccionar el objeto (ej. Play) -> menú Tools > Quitar scripts rotos.
public static class RemoveMissingScripts
{
    [MenuItem("Tools/Quitar scripts rotos (seleccion y hijos)")]
    static void Run()
    {
        int total = 0;

        foreach (GameObject root in Selection.gameObjects)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                int n = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                if (n == 0) continue;

                Undo.RegisterCompleteObjectUndo(t.gameObject, "Quitar scripts rotos");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                EditorSceneManager.MarkSceneDirty(t.gameObject.scene);
                total += n;
            }
        }

        Debug.Log("Scripts rotos eliminados: " + total + ". Guardá la escena con Ctrl+S.");
    }
}
