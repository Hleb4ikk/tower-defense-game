using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour 
{
    
    public Sprite pathTile;    // Префаб тайла пути
    public Sprite grassTile;   // Префаб тайла травы
    public Sprite waterTile;   // Префаб тайла воды
    public Sprite treeTile;    // Префаб тайла дерева
    public int width = 20;           // Ширина карты
    public int height = 20;          // Высота карты

    void Awake()
    {
        int[][] field = GenerateField();
        GeneratePath(field);
        GenerateLakes(field, 10);    // Генерация озёр
        GenerateTrees(field, 0.1f); // Генерация деревьев
        DrawField(field);
    }

    int[][] GenerateField(){
        
        int[][] field = new int[height][];
        for(int i = 0; i < height; i++){
            field[i] = new int[width];
        }
        return field;
    }

    void DrawField(int[][] field)
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
                        tile.AddComponent<SpriteRenderer>().sprite = waterTile;
                        break;
                    case 2:
                        tile = new GameObject($"Tree_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = treeTile;
                        break;
                    case 3:
                        tile = new GameObject($"Path_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = pathTile;
                        break;
                    default:
                    case 0:
                        tile = new GameObject($"Grass_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = grassTile; 
                        break;
                }

                // Устанавливаем позицию
                tile.transform.position = new Vector2(isoX, isoY);

                // Если нужна сортировка слоёв: добавляем Sorting Layer
                tile.GetComponent<SpriteRenderer>().sortingOrder = -(x + y); // Упрощение видимости
            }
        }  
    }

void GeneratePath(int[][] field)
{
    int currentX = UnityEngine.Random.Range(0, width);; // Начальная точка по X
    int currentY = 0; // Начальная точка по Y

    field[currentY][currentX] = 3; // Указываем, что эта клетка — часть пути
    currentY++;
    field[currentY][currentX] = 3;
    while (currentY < height-1)
    {
        int direction = UnityEngine.Random.Range(0, 3);
        
        switch (direction)
        {
            case 0: 
                currentY++;
                break;

            case 1:
                if(currentX+1 < width && field[currentY-1][currentX+1] != 3 && field[currentY+1][currentX+1] != 3 &&  field[currentY][currentX+1] != 3){
                    currentX++;
                } else {
                    currentY++;
                }
                break;

            case 2:
                if(currentX-1 >= 0 && field[currentY-1][currentX-1] != 3 && field[currentY+1][currentX-1] != 3 &&  field[currentY][currentX-1] != 3){
                    currentX--;
                } else {
                    currentY++;
                }
                break;
        }

        field[currentY][currentX] = 3;
        
        // Лог для проверки текущей позиции
        Debug.Log($"Current Position: X = {currentX}, Y = {currentY}");
    }
}

    
    void GenerateLakes(int[][] field, int numberOfLakes)
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

    void GenerateTrees(int[][] field, float treeProbability)
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
}
