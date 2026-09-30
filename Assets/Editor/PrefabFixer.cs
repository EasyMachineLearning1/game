using UnityEngine;
using UnityEditor;

/// <summary>
/// Исправляет материалы в конкретных префабах.
/// Запусти из меню Tools → Fix Specific Prefabs
/// </summary>
public class PrefabFixer : MonoBehaviour
{
    [MenuItem("Tools/Fix Specific Prefabs")]
    public static void FixSpecificPrefabs()
    {
        // Ищем все префабы в папке Assets (не в Packages)
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;
        int errorCount = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Пропускаем файлы в Packages
            if (path.Contains("/Packages/") || path.Contains("\\Packages\\"))
                continue;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            bool changed = false;
            MeshRenderer[] renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Material[] newMaterials = new Material[renderer.sharedMaterials.Length];

                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    Material oldMat = renderer.sharedMaterials[i];
                    if (oldMat == null)
                    {
                        newMaterials[i] = null;
                        continue;
                    }

                    string shaderName = oldMat.shader.name;
                    
                    // Если уже URP Lit — пропускаем
                    if (shaderName.Contains("Universal Render Pipeline/Lit") ||
                        shaderName.Contains("URP/Lit"))
                    {
                        newMaterials[i] = oldMat;
                        continue;
                    }

                    // Создаём новый URP материал
                    Material newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    newMat.CopyPropertiesFromMaterial(oldMat);
                    
                    newMaterials[i] = newMat;
                    changed = true;
                }

                if (changed)
                {
                    renderer.sharedMaterials = newMaterials;
                }
            }

            if (changed)
            {
                // Сохраняем префаб
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                fixedCount++;
                Debug.Log($"Исправлен: {prefab.name}");
            }
        }

        Debug.Log($"=== Исправлено {fixedCount} префабов ===");
        Debug.Log($"Ошибки: {errorCount}");
    }

    [MenuItem("Tools/Fix Single Prefab")]
    public static void FixSinglePrefab()
    {
        GameObject[] selected = Selection.gameObjects;
        if (selected.Length == 0)
        {
            Debug.LogWarning("Выдели префаб в Hierarchy или Project View");
            return;
        }

        foreach (GameObject obj in selected)
        {
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);
            bool changed = false;

            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Material[] newMaterials = new Material[renderer.sharedMaterials.Length];

                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    Material oldMat = renderer.sharedMaterials[i];
                    if (oldMat == null)
                    {
                        newMaterials[i] = null;
                        continue;
                    }

                    string shaderName = oldMat.shader.name;
                    
                    if (shaderName.Contains("Universal Render Pipeline/Lit") ||
                        shaderName.Contains("URP/Lit"))
                    {
                        newMaterials[i] = oldMat;
                        continue;
                    }

                    Material newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    newMat.CopyPropertiesFromMaterial(oldMat);
                    
                    newMaterials[i] = newMat;
                    changed = true;
                }

                if (changed)
                {
                    renderer.sharedMaterials = newMaterials;
                }
            }

            if (changed)
            {
                Debug.Log($"Исправлен: {obj.name}");
            }
        }

        Debug.Log("=== Готово ===");
    }
}
