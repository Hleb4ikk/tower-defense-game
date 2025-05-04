using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum MapObj {Grass, Cross, Access, Path}

public class MapGenerator : MonoBehaviour 
{
    [HideInInspector] 
    public MapObj[][] field;
    public Sprite pathTile;
    public Sprite grassTile;
    public Sprite crossTile;
    public int width = 12;
    public int height = 20;
    
    [HideInInspector] // Можно убрать HideInInspector, если хочешь видеть в инспекторе для отладки
    public int startX;
    
    [HideInInspector] // Можно убрать HideInInspector, если хочешь видеть в инспекторе для отладки
    public int startY;
    void Awake()
    {
        MapObj[][] field = GenerateField();
        GeneratePath(field);
        GenerateMapObjects(field, MapObj.Cross, 40);
        GenerateAccess(field);
        DrawField(field);
    }

    MapObj[][] GenerateField(){
        
        field = new MapObj[height][];
        for(int i = 0; i < height; i++){
            field[i] = new MapObj[width];
        }
        return field;
    }

    void DrawField(MapObj[][] field)
    {
        for (int x = 0; x < field.Length; x++) // Начинаем с 1
        {
            for (int y = 0; y < field[x].Length; y++) // Начинаем с 1
            {
                GameObject tile;
                switch (field[x][y]){
                    case MapObj.Path:
                        tile = new GameObject($"Path_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = pathTile;
                        break;

                    case MapObj.Cross:
                        tile = new GameObject($"Grass_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = grassTile;

                        GameObject cross = new GameObject($"Cross_Tile_{x}_{y}");
                        cross.AddComponent<SpriteRenderer>().sprite = crossTile;
                        cross.GetComponent<SpriteRenderer>().sortingOrder = 1;
                        cross.transform.position = new Vector2(x, y+0.3f);
                        break;

                    case MapObj.Access:
                        tile = new GameObject($"Access_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = grassTile;
                        tile.GetComponent<SpriteRenderer>().color = new Color(0.7f, 0.7f, 0.7f);
                        break;

                    default:
                    case MapObj.Grass:
                        tile = new GameObject($"Grass_Tile_{x}_{y}");
                        tile.AddComponent<SpriteRenderer>().sprite = grassTile; 
                        break;

                }
                tile.transform.position = new Vector2(x, y);
            }
        }  
    }

void GeneratePath(MapObj[][] field)
{
    int currentX = UnityEngine.Random.Range(0, width);; // Начальная точка по X
    int currentY = 0; // Начальная точка по Y

    startX = currentX;
    startY = currentY;

    field[currentY][currentX] = MapObj.Path; // Указываем, что эта клетка — часть пути
    currentY++;
    field[currentY][currentX] = MapObj.Path;
    while (currentY < height-1)
    {
        int direction = UnityEngine.Random.Range(0, 3);
        
        switch (direction)
        {
            case 0: 
                currentY++;
                break;

            case 1:
                if(currentX+1 < width && field[currentY-1][currentX+1] != MapObj.Path && field[currentY+1][currentX+1] != MapObj.Path &&  field[currentY][currentX+1] != MapObj.Path){
                    currentX++;
                } else {
                    currentY++;
                }
                break;

            case 2:
                if(currentX-1 >= 0 && field[currentY-1][currentX-1] != MapObj.Path && field[currentY+1][currentX-1] != MapObj.Path &&  field[currentY][currentX-1] != MapObj.Path){
                    currentX--;
                } else {
                    currentY++;
                }
                break;
        }

        field[currentY][currentX] = MapObj.Path;
    }
}

    void GenerateMapObjects(MapObj[][] field, MapObj obj, int numberOfObj){

        for (int i = 0; i < numberOfObj; i++)
        {
            int objX = UnityEngine.Random.Range(0, field.Length);
            int objY = UnityEngine.Random.Range(0, field[0].Length);

            if (field[objX][objY] == MapObj.Grass) 
            {
                field[objX][objY] = obj; 
            }
        }

    }

    void GenerateAccess(MapObj[][] field) {
        Debug.Log(field.Length + " " + field[0].Length);

        for (int x = 0; x < height; x++)
        {
            for (int y = 0; y < width; y++)
            {
                if (field[x][y] == MapObj.Path)
                {
                    for (int i = x - 1; i <= x+1; i++) {
                        for (int k = y - 1; k <= y+1; k++) {
                            if (i >= 0 && i < height && k >= 0 && k < width) {
                                Debug.Log(i + " " + k);
                                if(field[i][k] == MapObj.Grass) { // Используем правильный порядок индексов
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
