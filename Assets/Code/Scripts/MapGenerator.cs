using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
enum MapObj {Grass, Water, WideGrass, Path, Trees}
public class MapGenerator : MonoBehaviour 
{
    
    public Sprite pathTile;    // Префаб тайла пути
    public Sprite grassTile;   // Префаб тайла травы
    public Sprite waterTile;   // Префаб тайла воды
    public Sprite wideGrassTile;    // Префаб тайла дерева
    public Sprite treesTile;
    public int width = 12;           // Ширина карты
    public int height = 20;          // Высота карты

    void Awake()
    {
        int[][] field = GenerateField();
        GeneratePath(field);
        GenerateMapObjects(field, MapObj.Water, 40);
        GenerateMapObjects(field, MapObj.WideGrass, 40);
        GenerateMapObjects(field, MapObj.Trees, 40);
        GenerateAccess(field);
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
                // float isoX = (x - y) * 0.5f;
                // float isoY = (x + y) * 0.25f;

                // Создание нового объекта и добавление SpriteRenderer
                GameObject tile;
                switch (field[x][y]){
                    case 1:
                        tile = new GameObject($"Water_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = waterTile;
                        break;
                    case 2:
                        tile = new GameObject($"Wide_Grass_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = wideGrassTile;
                        break;
                    case 3:
                        tile = new GameObject($"Path_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = pathTile;
                        break;
                    case 4:
                        tile = new GameObject($"Grass_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = grassTile;

                        GameObject tree = new GameObject($"Trees_Tile_{x}_{y}");
                        tree.AddComponent<SpriteRenderer>().sprite = treesTile;
                        tree.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        tree.transform.position = new Vector2(x, y+0.3f);
                        break;
                    case 5:
                        tile = new GameObject($"Access_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = grassTile;
                        tile.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f);
                        break;
                    default:
                    case 0:
                        tile = new GameObject($"Grass_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = grassTile; 
                        break;
                }

                // Устанавливаем позицию
                tile.transform.position = new Vector2(x, y);

                // Если нужна сортировка слоёв: добавляем Sorting Layer
                // tile.GetComponent<SpriteRenderer>().sortingOrder = -(x + y); // Упрощение видимости
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
    }
}

    void GenerateMapObjects(int[][] field, MapObj obj, int numberOfObj){

        for (int i = 0; i < numberOfObj; i++)
        {
            int objX = UnityEngine.Random.Range(0, field.Length);
            int objY = UnityEngine.Random.Range(0, field[0].Length);

            if (field[objX][objY] == 0) 
            {
                field[objX][objY] = (int) obj; 
            }
        }

    }

    void GenerateAccess(int[][] field) {
        Debug.Log(field.Length + " " + field[0].Length);

        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (field[x][y] == 3)
                {
                    for (int i = x - 1; i <= x+1; i++) {
                        for (int k = y - 1; k <= y+1; k++) {
                            if (i >= 0 && i < height && k >= 0 && k < width) {
                                Debug.Log(i + " " + k);
                                if(field[i][k] == 0) { // Используем правильный порядок индексов
                                    field[i][k] = 5;
                                } 
                            }
                        } 
                    }
                }
            }
        }
    }
}
