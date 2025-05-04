using UnityEngine;
using UnityEngine.EventSystems; // Необходимо для проверки кликов по UI

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance; // Синглтон для легкого доступа

    public GameObject selectedTowerPrefab = null; // Префаб выбранной башни для установки
    public LayerMask tileLayerMask;       // Маска слоя, чтобы Raycast попадал только на тайлы
    public Camera mainCamera;             // Ссылка на основную камеру

    void Awake()
    {
        // Настройка синглтона
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Уничтожить дубликат, если он есть
            return;
        }
    }

    void Start()
    {
        // Попытка найти камеру, если она не назначена вручную
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (mainCamera == null)
        {
            Debug.LogError("TowerPlacementManager: Main Camera не найдена!");
        }
    }

    void Update()
    {
        // 1. Проверяем, выбрана ли башня для установки
        if (selectedTowerPrefab == null)
        {
            // Если башня не выбрана, ничего не делаем в Update
            return;
        }

        // 2. Отмена выбора/установки по правой кнопке мыши
        if (Input.GetMouseButtonDown(1)) // 1 = правая кнопка мыши
        {
            DeselectTower();
            Debug.Log("Установка отменена");
            return;
        }

        // 3. Обработка клика левой кнопкой мыши для установки
        if (Input.GetMouseButtonDown(0)) // 0 = левая кнопка мыши
        {
            // 4. Игнорируем клик, если он попал на элемент UI (кнопку и т.д.)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("Клик по UI, установка игнорируется.");
                return;
            }

            // 5. Пытаемся установить башню
            PlaceSelectedTower();
        }

        // --- Необязательно: Визуализация призрака башни под курсором ---
        // Можно добавить код, который будет перемещать полупрозрачную копию
        // selectedTowerPrefab под курсором мыши над доступными тайлами.
        // Это улучшит UX, но усложнит код.
    }

    // Метод, вызываемый кнопками UI для выбора башни
    public void SelectTowerToPlace(GameObject towerPrefab)
    {
        selectedTowerPrefab = towerPrefab;
        Debug.Log($"Выбрана башня: {towerPrefab.name}");
        // Здесь можно добавить логику списания денег, если нужно
    }

    // Метод для отмены выбора башни
    public void DeselectTower()
    {
        selectedTowerPrefab = null;
        // Убрать визуализацию призрака, если она есть
    }

    // Логика установки башни
    // Логика установки башни (версия для 2D)
    private void PlaceSelectedTower()
    {
        // Получаем позицию мыши в мировых координатах для 2D
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Пускаем луч из точки мыши (или можно из камеры, но для ортографической 2D это проще)
        // Physics2D.Raycast требует начальную точку и направление. Для клика мыши направление не важно,
        // важен только коллайдер прямо под точкой.
        // Указываем дистанцию 0, чтобы проверить только точку под курсором.
        // И используем нашу маску слоя тайлов.
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f, tileLayerMask);

        // Проверяем, попали ли мы в коллайдер на нужном слое
        if (hit.collider != null)
        {
            // Получаем скрипт тайла, на который попал луч
            TileScript tile = hit.collider.GetComponent<TileScript>();

            // Проверяем, что тайл существует, на нем можно строить и он не занят
            if (tile != null && tile.isBuildable && !tile.hasTower)
            {
                // Создаем экземпляр башни из префаба
                // Позиция: центр тайла. Используем позицию коллайдера/тайла.
                Vector3 placementPosition = hit.transform.position;
                // Можно добавить небольшой оффсет по Z, чтобы башня была "выше" тайла, если нужно
                // placementPosition.z -= 0.1f;

                GameObject newTower = Instantiate(selectedTowerPrefab, placementPosition, Quaternion.identity);

                // Помечаем тайл как занятый
                tile.hasTower = true;
                // tile.towerInstance = newTower; // Если нужно хранить ссылку

                Debug.Log($"Башня {selectedTowerPrefab.name} установлена на тайл {tile.name} (Координаты: {hit.transform.position})");

                DeselectTower();

                // Здесь можно добавить логику списания ресурсов за башню
            }
            else
            {
                // Сообщаем, почему нельзя установить
                if (tile == null) Debug.LogWarning($"Клик попал на коллайдер слоя '{LayerMask.LayerToName(hit.collider.gameObject.layer)}', но без TileScript! Объект: {hit.collider.name}");
                else if (!tile.isBuildable) Debug.Log($"На этом тайле ({hit.collider.name}) строить нельзя.");
                else if (tile.hasTower) Debug.Log($"Этот тайл ({hit.collider.name}) уже занят.");
            }
        }
        else // Сюда попадаем, если Physics2D.Raycast ничего не нашел под курсором на указанном слое
        {
            Debug.Log("Клик мимо тайлов (или объектов на указанном слое с коллайдерами).");
        }
    }
}