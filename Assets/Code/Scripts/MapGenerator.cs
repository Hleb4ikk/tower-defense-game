using System;
using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.Tilemaps; // Не используется, можно убрать

// Убедитесь, что enum определен правильно
enum MapObj { Grass, Cross, Access, Path } // Значения по умолчанию: Grass=0, Cross=1, Access=2, Path=3

public class MapGenerator : MonoBehaviour
{
    [Header("Tile Sprites")]
    public Sprite pathTile;
    public Sprite grassTile;
    public Sprite crossTile; // Спрайт для креста/декорации

    [Header("Map Dimensions")]
    public int width = 20;
    public int height = 12;

    [Header("Generation Settings")]
    [Range(0, 100)] public int crossDensity = 15; // Процент заполнения крестами (примерно)

    [Header("Setup")]
    public string tileLayerName = "Tiles"; // Имя слоя для тайлов в редакторе Unity

    // --- Приватные поля ---
    private int _tileLayer;         // Кэшированный индекс слоя
    private Transform _mapContainer; // Родительский Transform для тайлов

    void Awake()
    {
        // Проверяем и кэшируем индекс слоя
        _tileLayer = LayerMask.NameToLayer(tileLayerName);
        if (_tileLayer == -1)
        {
            Debug.LogError($"Слой '{tileLayerName}' не найден! Пожалуйста, создайте его в Edit -> Project Settings -> Tags and Layers.");
            // Можно добавить return или отключение компонента, если слой критичен
        }

        // Кэшируем Transform этого объекта для родительства тайлов
        _mapContainer = this.transform;

        // Запускаем генерацию
        GenerateAndDrawMap();
    }

    void GenerateAndDrawMap()
    {
        MapObj[][] field = GenerateField();
        GeneratePath(field);
        // Генерируем объекты *после* пути, чтобы не перезаписать его
        // Передаем плотность вместо фиксированного числа
        GenerateMapObjects(field, MapObj.Cross, crossDensity);
        // Генерируем доступные тайлы *после* объектов
        GenerateAccess(field);
        DrawField(field);
    }

    MapObj[][] GenerateField()
    {
        MapObj[][] field = new MapObj[height][];
        for (int i = 0; i < height; i++)
        {
            field[i] = new MapObj[width];
            // По умолчанию все ячейки - Grass (т.к. enum Grass = 0)
            // for(int j=0; j<width; ++j) field[i][j] = MapObj.Grass; // Не обязательно
        }
        return field;
    }

    void DrawField(MapObj[][] field)
    {

        for (int x = 0; x < field.Length; x++) // field.Length это height
        {
            for (int y = 0; y < field[x].Length; y++) // field[x].Length это width
            {
                GameObject tile = null;
                bool isBuildableTile = false;
                Sprite currentSprite = grassTile; // Спрайт по умолчанию
                string tileNamePrefix = "Grass";
                GameObject decorObject = null; // Для креста или другого декора

                MapObj currentType = field[x][y];

                // --- 1. Определяем свойства тайла ---
                switch (currentType)
                {
                    case MapObj.Path:
                        tileNamePrefix = "Path";
                        currentSprite = pathTile;
                        isBuildableTile = false; // На пути строить нельзя
                        break;
                    case MapObj.Cross: // Основа - трава, сверху - крест
                        tileNamePrefix = "Grass";
                        currentSprite = grassTile;
                        isBuildableTile = false; // На декорациях строить нельзя
                        // Готовим GameObject для декора
                        decorObject = new GameObject($"Cross_Visual_{x}_{y}");
                        break;
                    case MapObj.Access: // Тайлы для строительства
                        tileNamePrefix = "Access";
                        currentSprite = grassTile; // Используем спрайт травы
                        isBuildableTile = true;    // Здесь можно строить!
                        break;
                    case MapObj.Grass: // Обычная трава
                    default:
                        tileNamePrefix = "Grass";
                        currentSprite = grassTile;
                        isBuildableTile = false; // По умолчанию на траве строить нельзя
                                                 // (Измените на true, если хотите разрешить)
                        break;
                }

                // --- 2. Создаем основной GameObject тайла ---
                tile = new GameObject($"{tileNamePrefix}_Tile_{x}_{y}");

                // --- 3. Устанавливаем родителя и позицию ---
                tile.transform.SetParent(_mapContainer);
                tile.transform.localPosition = new Vector2(y, x); // ВАЖНО: Обычно X - это ширина (y), Y - высота (x)
                                                                  // Если у вас наоборот, используйте new Vector2(x, y)

                // --- 4. Добавляем SpriteRenderer ---
                SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
                sr.sprite = currentSprite;
                // Добавляем легкое затемнение для Access тайлов
                if (currentType == MapObj.Access)
                {
                    sr.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Немного сероватый
                }

                // --- 5. Добавляем TileScript и настраиваем ---
                TileScript tileScript = tile.AddComponent<TileScript>();
                tileScript.isBuildable = isBuildableTile;
                tileScript.hasTower = false; // Изначально башни нет

                // --- 6. Добавляем Collider2D ---
                var collider = tile.AddComponent<BoxCollider2D>();
                // collider.size = new Vector2(1, 1); // Можно настроить размер, если нужно

                // --- 7. Устанавливаем слой ---
                if (_tileLayer != -1)
                {
                    tile.layer = _tileLayer;
                }

                // --- 8. Создаем и настраиваем декор (если есть) ---
                if (decorObject != null && currentType == MapObj.Cross)
                {
                    // Настраиваем SpriteRenderer для декора
                    SpriteRenderer decorSr = decorObject.AddComponent<SpriteRenderer>();
                    decorSr.sprite = crossTile;
                    decorSr.sortingOrder = 1; // Декор поверх тайла

                    // Делаем декор дочерним объектом тайла
                    decorObject.transform.SetParent(tile.transform);

                    // Устанавливаем локальную позицию декора относительно тайла
                    // (0, 0.3f) - немного выше центра тайла, подберите значение
                    decorObject.transform.localPosition = new Vector2(0, 0.3f);
                }

                // Опционально: Сортировка для изометрии/вида сверху
                // sr.sortingOrder = -(x + y);
            }
        }
    }

    // --- Методы генерации ---

    void GeneratePath(MapObj[][] field)
    {
        // Используем height (field.Length) и width (field[0].Length) для границ
        int currentX = 0; // Начинаем с левого края
        int currentY = UnityEngine.Random.Range(1, height - 1); // Случайная высота старта (не у краев)

        field[currentY][currentX] = MapObj.Path; // Первая точка

        while (currentX < width - 1) // Идем до правого края
        {
            // Приоритет движения: Вправо > Вверх/Вниз > Стоп
            int direction = UnityEngine.Random.Range(0, 10); // 0-5: Вправо, 6-7: Вверх, 8-9: Вниз

            int nextX = currentX;
            int nextY = currentY;

            if (direction <= 5) // Двигаемся вправо
            {
                nextX++;
            }
            else if (direction <= 7) // Двигаемся вверх
            {
                if (currentY > 0) // Не выходим за верхнюю границу
                    nextY--;
                else
                    nextX++; // Если уперлись вверх, идем вправо
            }
            else // Двигаемся вниз
            {
                if (currentY < height - 1) // Не выходим за нижнюю границу
                    nextY++;
                else
                    nextX++; // Если уперлись вниз, идем вправо
            }

            // Проверяем, чтобы новая клетка была в границах (на всякий случай)
            if (nextX < width && nextY >= 0 && nextY < height)
            {
                currentX = nextX;
                currentY = nextY;
                field[currentY][currentX] = MapObj.Path;
            }
            else
            {
                // Если вышли за пределы (маловероятно с текущей логикой), прерываем
                break;
            }
        }
        // Опционально: можно добавить еще один сегмент пути вверх/вниз в конце
    }


    void GenerateMapObjects(MapObj[][] field, MapObj objType, int densityPercent)
    {
        int attempts = 0;
        int maxAttempts = height * width; // Не больше, чем клеток всего
        int targetCount = (int)(height * width * (densityPercent / 100.0f));
        int placedCount = 0;

        while (placedCount < targetCount && attempts < maxAttempts)
        {
            int objX = UnityEngine.Random.Range(0, height); // Индекс строки (Y)
            int objY = UnityEngine.Random.Range(0, width);  // Индекс столбца (X)

            // Ставим объект только если клетка - это обычная трава (Grass)
            if (field[objX][objY] == MapObj.Grass)
            {
                field[objX][objY] = objType;
                placedCount++;
            }
            attempts++;
        }
        if (attempts >= maxAttempts && placedCount < targetCount)
        {
            Debug.LogWarning($"Не удалось разместить все объекты типа {objType}. Цель: {targetCount}, Размещено: {placedCount}");
        }
    }


    void GenerateAccess(MapObj[][] field)
    {
        // Создаем копию поля, чтобы изменения не влияли на проверку соседей
        MapObj[][] originalField = new MapObj[height][];
        for (int i = 0; i < height; ++i)
        {
            originalField[i] = (MapObj[])field[i].Clone();
        }

        // Debug.Log($"Generating Access Tiles for map {height}x{width}");

        for (int x = 0; x < height; x++) // Итерация по строкам (Y)
        {
            for (int y = 0; y < width; y++) // Итерация по столбцам (X)
            {
                // Ищем тайлы пути в *оригинальном* поле
                if (originalField[x][y] == MapObj.Path)
                {
                    // Проверяем всех 8 соседей
                    for (int i = x - 1; i <= x + 1; i++)
                    {
                        for (int k = y - 1; k <= y + 1; k++)
                        {
                            // Пропускаем сам тайл пути
                            if (i == x && k == y) continue;

                            // Проверяем границы карты
                            if (i >= 0 && i < height && k >= 0 && k < width)
                            {
                                // Если соседняя клетка в *оригинальном* поле - это ТРАВА (Grass)
                                // то меняем её на Access в *основном* поле field
                                if (originalField[i][k] == MapObj.Grass)
                                {
                                    // Debug.Log($"Setting Access at [{i},{k}] near Path at [{x},{y}]");
                                    field[i][k] = MapObj.Access;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}