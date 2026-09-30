using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Автоматическая конвертация всех материалов в URP Lit.
/// Запускается из меню Tools → Convert Materials to URP
/// </summary>
public class MaterialConverter : MonoBehaviour
{
    [MenuItem("Tools/Convert Materials to URP Lit")]
    public static void ConvertAllMaterials()
    {
        string[] materialGUIDs = AssetDatabase.FindAssets("t:Material");
        int convertedCount = 0;
        int skippedCount = 0;
        int errorCount = 0;

        foreach (string guid in materialGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null) continue;

            // Пропускаем не-материалы (шрифты, файлы и т.д.)
            if (!path.EndsWith(".mat", System.StringComparison.OrdinalIgnoreCase))
            {
                skippedCount++;
                continue;
            }

            // Пропускаем файлы в папке Packages (защита)
            if (path.Contains("/Packages/") || path.Contains("\\Packages\\"))
            {
                skippedCount++;
                continue;
            }

            // Дополнительно проверяем через AssetDatabase.IsValidFolder
            if (AssetDatabase.IsValidFolder("Packages"))
            {
                string packagePath = "Packages";
                if (path.StartsWith(packagePath + "/", System.StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(packagePath + "\\", System.StringComparison.OrdinalIgnoreCase))
                {
                    skippedCount++;
                    continue;
                }
            }

            string shaderName = material.shader.name;
            
            // Пропускаем уже URP материалы
            if (shaderName.Contains("Universal Render Pipeline/Lit") ||
                shaderName.Contains("URP/Lit"))
            {
                skippedCount++;
                continue;
            }

            // Пропускаем Special и Decal шейдеры
            if (shaderName.Contains("Shader Graph") ||
                shaderName.Contains("Decal") ||
                shaderName.Contains("Hidden") ||
                shaderName.Contains("Error") ||
                shaderName.Contains("GUI") ||
                shaderName.Contains("Unlit"))
            {
                skippedCount++;
                continue;
            }

            try
            {
                // Создаём новый материал с URP Lit шейдером
                Material newMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                
                // Копируем текстуры (с проверками)
                if (material.HasProperty("_MainTex") && material.GetTexture("_MainTex") != null)
                    newMaterial.SetTexture("_BaseMap", material.GetTexture("_MainTex"));
                else if (material.HasProperty("_BaseMap") && material.GetTexture("_BaseMap") != null)
                    newMaterial.SetTexture("_BaseMap", material.GetTexture("_BaseMap"));
                
                // Копируем цвет (с проверками)
                if (material.HasProperty("_Color"))
                    newMaterial.SetColor("_BaseColor", material.GetColor("_Color"));
                else if (material.HasProperty("_BaseColor"))
                    newMaterial.SetColor("_BaseColor", material.GetColor("_BaseColor"));
                else
                    newMaterial.SetColor("_BaseColor", Color.white);
                
                // Копируем нормалки (с проверками)
                if (material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap") != null)
                    newMaterial.SetTexture("_NormalMap", material.GetTexture("_BumpMap"));
                else if (material.HasProperty("_NormalMap") && material.GetTexture("_NormalMap") != null)
                    newMaterial.SetTexture("_NormalMap", material.GetTexture("_NormalMap"));
                
                // Копируем metallic и smoothness (с проверками)
                if (material.HasProperty("_Metallic"))
                    newMaterial.SetFloat("_Metallic", material.GetFloat("_Metallic"));
                else
                    newMaterial.SetFloat("_Metallic", 0f);
                
                if (material.HasProperty("_Glossiness"))
                    newMaterial.SetFloat("_Smoothness", material.GetFloat("_Glossiness"));
                else if (material.HasProperty("_Smoothness"))
                    newMaterial.SetFloat("_Smoothness", material.GetFloat("_Smoothness"));
                else
                    newMaterial.SetFloat("_Smoothness", 0.5f);
                
                // Копируем альфа-порог
                if (material.HasProperty("_Cutoff"))
                    newMaterial.SetFloat("_Cutoff", material.GetFloat("_Cutoff"));
                
                // Сохраняем как новый файл
                string newPath = Path.Combine(Path.GetDirectoryName(path), 
                    Path.GetFileNameWithoutExtension(path) + "_URP" + Path.GetExtension(path));
                
                // Удаляем старый файл если есть (но не в Packages)
                if (File.Exists(newPath) && !newPath.Contains("/Packages/") && !newPath.Contains("\\Packages\\"))
                {
                    AssetDatabase.DeleteAsset(newPath);
                }
                
                // Создаём новый файл только если путь не в Packages
                bool isPackagePath = newPath.Contains("/Packages/") || newPath.Contains("\\Packages\\");
                if (!isPackagePath)
                {
                    AssetDatabase.CreateAsset(newMaterial, newPath);
                    convertedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
            catch (System.Exception e)
            {
                errorCount++;
                Debug.LogError($"Ошибка при конвертации {material.name}: {e.Message}");
            }
        }

        // Не вызываем Refresh — Unity сам обновится
        Debug.Log($"=== Конвертация завершена ===");
        Debug.Log($"Конвертировано: {convertedCount}");
        Debug.Log($"Пропущено: {skippedCount}");
        Debug.Log($"Ошибки: {errorCount}");
    }

    [MenuItem("Tools/Fix Selected Prefabs")]
    public static void FixSelectedPrefabs()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int fixedCount = 0;

        foreach (GameObject obj in selectedObjects)
        {
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(true);
            
            foreach (MeshRenderer renderer in renderers)
            {
                if (renderer.sharedMaterials == null || renderer.sharedMaterials.Length == 0)
                    continue;

                Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                bool changed = false;

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

                    // Пропускаем Special шейдеры
                    if (shaderName.Contains("Shader Graph") ||
                        shaderName.Contains("Decal") ||
                        shaderName.Contains("Hidden") ||
                        shaderName.Contains("GUI") ||
                        shaderName.Contains("Unlit"))
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
                    fixedCount++;
                }
            }
        }

        Debug.Log($"=== Исправлено {fixedCount} объектов ===");
    }
}
