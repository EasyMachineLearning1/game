using UnityEngine;
using UnityEditor;

public class FixAllPrefabs : MonoBehaviour
{
    [MenuItem("Tools/Fix All Prefabs Once")]
    public static void FixAllPrefabsOnce()
    {
        string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;
        int skippedCount = 0;
        int errorCount = 0;

        foreach (string guid in prefabGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Пропускаем Packages
            if (path.Contains("/Packages/") || path.Contains("\\Packages\\"))
            {
                skippedCount++;
                continue;
            }

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
                PrefabUtility.SaveAsPrefabAsset(prefab, path);
                fixedCount++;
                Debug.Log($"Исправлен: {prefab.name}");
            }
        }

        Debug.Log($"=== Исправлено {fixedCount} префабов ===");
        Debug.Log($"Пропущено: {skippedCount}");
        Debug.Log($"Ошибки: {errorCount}");
    }
}
