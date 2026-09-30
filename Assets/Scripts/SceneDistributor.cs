using UnityEngine;

/// <summary>
/// Автоматическая расстановка объектов на сцене.
/// Перетащи в сцену и настрой префабы в Inspector.
/// </summary>
public class SceneDistributor : MonoBehaviour
{
    [Header("Настройки генерации")]
    [SerializeField] private Vector3 areaSize = new Vector3(50, 1, 50); // Размер зоны (X, Y, Z)


    [Header("Здания")]
    [SerializeField] private GameObject[] buildings; // Панельные дома
    [SerializeField] private GameObject[] garages; // Гаражи
    [SerializeField] private GameObject[] fences; // Заборы

    [Header("Растения")]
    [SerializeField] private GameObject[] trees; // Деревья
    [SerializeField] private GameObject[] bushes; // Кусты
    [SerializeField] private GameObject[] grass; // Трава/мелкие растения

    [Header("Мелочи")]
    [SerializeField] private GameObject[] props; // Урны, фонари, автоматы и т.д.

    [Header("Дороги")]
    [SerializeField] private GameObject[] roads; // Дорожные покрытия
    [SerializeField] private GameObject[] sidewalks; // Тротуары

    [Header("Дополнительно")]
    [SerializeField] private bool debugMode = true; // Показывать зону в редакторе

    private void Start()
    {
        DistributeObjects();
    }

    public void DistributeObjects()
    {
        Debug.Log("=== Расстановка объектов на сцене ===");

        // Центр зоны
        Vector3 center = new Vector3(
            transform.position.x,
            0,
            transform.position.z
        );

        // 1. Здания (по краям зоны)
        PlaceBuildings(center);

        // 2. Гаражи (вдоль одной стороны)
        PlaceGarages(center);

        // 3. Деревья (вдоль "улицы")
        PlaceTrees(center);

        // 4. Кусты (беспорядочно)
        PlaceBushes(center);

        // 5. Мелочи (случайно)
        PlaceProps(center);

        // 6. Дороги
        PlaceRoads(center);

        Debug.Log("=== Расстановка завершена ===");
    }

    /// <summary>
    /// Размещает здания по периметру зоны
    /// </summary>
    private void PlaceBuildings(Vector3 center)
    {
        if (buildings == null || buildings.Length == 0) return;

        int count = Random.Range(2, 5);
        for (int i = 0; i < count; i++)
        {
            // Выбираем случайную сторону
            int side = Random.Range(0, 4);
            float x, z;

            switch (side)
            {
                case 0: // Север
                    x = center.x + Random.Range(-areaSize.x / 2, areaSize.x / 2);
                    z = center.z + areaSize.z / 2 + 5f;
                    break;
                case 1: // Юг
                    x = center.x + Random.Range(-areaSize.x / 2, areaSize.x / 2);
                    z = center.z - areaSize.z / 2 - 5f;
                    break;
                case 2: // Восток
                    x = center.x + areaSize.x / 2 + 5f;
                    z = center.z + Random.Range(-areaSize.z / 2, areaSize.z / 2);
                    break;
                default: // Запад
                    x = center.x - areaSize.x / 2 - 5f;
                    z = center.z + Random.Range(-areaSize.z / 2, areaSize.z / 2);
                    break;
            }

            GameObject prefab = buildings[Random.Range(0, buildings.Length)];
            GameObject building = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
            building.name = $"Building_{i}";

            // Случайный поворот (0, 90, 180, 270)
            float rotY = Random.Range(0, 4) * 90f;
            building.transform.rotation = Quaternion.Euler(0, rotY, 0);

            if (debugMode)
                Debug.Log($"Поставлен дом: {prefab.name} в ({x:F1}, {z:F1})");
        }
    }

    /// <summary>
    /// Размещает гаражи линией
    /// </summary>
    private void PlaceGarages(Vector3 center)
    {
        if (garages == null || garages.Length == 0) return;

        int garageCount = Random.Range(3, 8);
        float startX = center.x - (garageCount * 4f) / 2f;
        float z = center.z + areaSize.z / 2 - 8f;

        for (int i = 0; i < garageCount; i++)
        {
            GameObject prefab = garages[Random.Range(0, garages.Length)];
            GameObject garage = Instantiate(prefab, new Vector3(startX + i * 4f, 0, z), Quaternion.identity);
            garage.name = $"Garage_{i}";

            if (debugMode)
                Debug.Log($"Поставлен гараж: {prefab.name} в ({startX + i * 4f:F1}, {z:F1})");
        }
    }

    /// <summary>
    /// Размещает деревья вдоль "улицы"
    /// </summary>
    private void PlaceTrees(Vector3 center)
    {
        if (trees == null || trees.Length == 0) return;

        int treeCount = Random.Range(4, 10);
        float streetZ = center.z;

        for (int i = 0; i < treeCount; i++)
        {
            // Деревья по обе стороны улицы
            float side = (i % 2 == 0) ? 1 : -1;
            float x = center.x + Random.Range(-areaSize.x / 2, areaSize.x / 2);
            float z = streetZ + side * Random.Range(2f, 5f);

            GameObject prefab = trees[Random.Range(0, trees.Length)];
            GameObject tree = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
            tree.name = $"Tree_{i}";

            // Случайный масштаб для разнообразия
            float scale = Random.Range(0.8f, 1.2f);
            tree.transform.localScale = new Vector3(scale, scale, scale);

            // Случайный поворот
            tree.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            if (debugMode)
                Debug.Log($"Поставлено дерево: {prefab.name} в ({x:F1}, {z:F1})");
        }
    }

    /// <summary>
    /// Размещает кусты беспорядочно
    /// </summary>
    private void PlaceBushes(Vector3 center)
    {
        if (bushes == null || bushes.Length == 0) return;

        int bushCount = Random.Range(10, 25);

        for (int i = 0; i < bushCount; i++)
        {
            float x = center.x + Random.Range(-areaSize.x / 2, areaSize.x / 2);
            float z = center.z + Random.Range(-areaSize.z / 2, areaSize.z / 2);

            GameObject prefab = bushes[Random.Range(0, bushes.Length)];
            GameObject bush = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
            bush.name = $"Bush_{i}";

            // Случайный масштаб
            float scale = Random.Range(0.5f, 1.5f);
            bush.transform.localScale = new Vector3(scale, scale, scale);

            if (debugMode)
                Debug.Log($"Поставлен куст: {prefab.name} в ({x:F1}, {z:F1})");
        }
    }

    /// <summary>
    /// Размещает мелочи (фонари, урны, автоматы и т.д.)
    /// </summary>
    private void PlaceProps(Vector3 center)
    {
        if (props == null || props.Length == 0) return;

        int propCount = Random.Range(5, 15);

        for (int i = 0; i < propCount; i++)
        {
            float x = center.x + Random.Range(-areaSize.x / 2, areaSize.x / 2);
            float z = center.z + Random.Range(-areaSize.z / 2, areaSize.z / 2);

            GameObject prefab = props[Random.Range(0, props.Length)];
            GameObject prop = Instantiate(prefab, new Vector3(x, 0, z), Quaternion.identity);
            prop.name = $"Prop_{i}";

            // Случайный поворот
            prop.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

            if (debugMode)
                Debug.Log($"Поставлен объект: {prefab.name} в ({x:F1}, {z:F1})");
        }
    }

    /// <summary>
    /// Размещает дороги
    /// </summary>
    private void PlaceRoads(Vector3 center)
    {
        if (roads == null || roads.Length == 0) return;

        // Главная дорога по центру (с севера на юг)
        float roadX = center.x;
        float roadZ = center.z;

        GameObject road = Instantiate(roads[0], new Vector3(roadX, 0.01f, roadZ), Quaternion.identity);
        road.name = "MainRoad";
        road.transform.localScale = new Vector3(6f, 1f, areaSize.z);

        // Перпендикулярная дорога (с запада на восток)
        GameObject road2 = Instantiate(roads[0], new Vector3(roadX, 0.01f, roadZ), Quaternion.Euler(0, 90, 0));
        road2.name = "CrossRoad";
        road2.transform.localScale = new Vector3(6f, 1f, areaSize.x);

        // Тротуары вдоль дороги
        if (sidewalks != null && sidewalks.Length > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                float offsetX = (i % 2 == 0) ? -3.5f : 3.5f;
                GameObject sidewalk = Instantiate(sidewalks[0],
                    new Vector3(roadX + offsetX, 0.02f, roadZ),
                    Quaternion.identity);
                sidewalk.name = $"Sidewalk_{i}";
                sidewalk.transform.localScale = new Vector3(2f, 1f, areaSize.z);
            }
        }

        if (debugMode)
            Debug.Log($"Поставлена дорога в ({roadX:F1}, {roadZ:F1})");
    }

    // Отрисовка зоны в редакторе
    private void OnDrawGizmosSelected()
    {
        if (!debugMode) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(areaSize.x, 0, areaSize.z)
        );
    }
}
