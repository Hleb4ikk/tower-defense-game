using System;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Sprite pathTilePrefab;    // Префаб тайла пути
    public Sprite grassTilePrefab;   // Префаб тайла травы
    public Sprite waterTilePrefab;   // Префаб тайла воды
    public Sprite treeTilePrefab;    // Префаб тайла дерева
    public int width = 10;           // Ширина карты
    public int height = 5;          // Высота карты

    void Start()
    {
        float[][] field = GenerateField();
        GeneratePath(field);
        GenerateLakes(field, 3);    // Генерация озёр
        GenerateTrees(field, 0.1f); // Генерация деревьев
        DrawField(field);
        PositionCamera();
    }

    float[][] GenerateField(){
        
        float[][] field = new float[height][];
        for(int i = 0; i < height; i++){
            field[i] = new float[width];
        }
        return field;
    }

    void DrawField(float[][] field)
    {
        for (int x = 0; x < field.Length; x++) // Начинаем с 1
        {
            for (int y = 0; y < field[x].Length; y++) // Начинаем с 1
            {
                // Изометрические преобразования
                float isoX = (x - y) * 0.5f;
                float isoY = (x + y) * 0.25f;

                // Создание нового объекта и добавление SpriteRenderer
                GameObject tile;
                switch (field[x][y]){
                    case 1:
                        tile = new GameObject($"Water_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = waterTilePrefab;
                        break;
                    case 2:
                        tile = new GameObject($"Tree_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = treeTilePrefab;
                        break;
                    case 3:
                        tile = new GameObject($"Path_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = pathTilePrefab;
                        break;
                    default:
                    case 0:
                        tile = new GameObject($"Grass_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = grassTilePrefab; 
                        break;
                }

                // Устанавливаем позицию
                tile.transform.position = new Vector2(isoX, isoY);

                // Если нужна сортировка слоёв: добавляем Sorting Layer
                tile.GetComponent<SpriteRenderer>().sortingOrder = -(x + y); // Упрощение видимости
            }
        }

        
    }

    void GeneratePath(float[][] field)
    {
        int currentX = UnityEngine.Random.Range(0, field.Length); // Стартовая строка
        int currentY = 0; // Путь начинается с левого края карты

        while (currentY < field[0].Length) // Пока путь не достиг конца по оси Y
        {
            field[currentX][currentY] = 3; // Значение 3 обозначает путь

            // Случайное направление движения: либо вперёд, либо вниз/вверх строго по вертикали
            int direction = UnityEngine.Random.Range(0, 3); // 0 - вправо, 1 - вверх, 2 - вниз

            if (direction == 0 && currentY < field[0].Length - 1)
            {
                currentY++; // Движение вправо
            }
            else if (direction == 1 && currentX > 0)
            {
                currentX--; // Движение вверх
            }
            else if (direction == 2 && currentX < field.Length - 1)
            {
                currentX++; // Движение вниз
            }
            // Если направление невозможно (например, выход за границы), остаёмся на месте и идём вправо
            else if (currentY < field[0].Length - 1)
            {
                currentY++;
            }
        }
    }
    
    void GenerateLakes(float[][] field, int numberOfLakes)
    {
        for (int i = 0; i < numberOfLakes; i++)
        {
            int lakeX = UnityEngine.Random.Range(0, field.Length);
            int lakeY = UnityEngine.Random.Range(0, field[0].Length);

            // Проверяем, чтобы озеро не накладывалось на путь или другие объекты
            if (field[lakeX][lakeY] == 0) 
            {
                field[lakeX][lakeY] = 1; // Значение 1 указывает воду
            }
        }
    }

    void GenerateTrees(float[][] field, float treeProbability)
    {
        for (int x = 0; x < field.Length; x++)
        {
            for (int y = 0; y < field[x].Length; y++)
            {
                // Проверяем, чтобы деревья генерировались только на траве
                if (field[x][y] == 0 && UnityEngine.Random.Range(0f, 1f) < treeProbability)
                {
                    field[x][y] = 2; // Значение 2 указывает дерево
                }
            }
        }
    }

    void PositionCamera()
    {
        float centerX = 0;
        float centerY = width * 0.25f;
        GameObject camera = new GameObject("Camera");
        camera.AddComponent<Camera>();
        camera.transform.position = new Vector2(centerX, centerY);
        camera.GetComponent<Camera>().nearClipPlane = 0f;
        camera.GetComponent<Camera>().orthographic = true;
    }
}
