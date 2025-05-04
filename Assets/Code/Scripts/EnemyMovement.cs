using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyMovement : MonoBehaviour
{
    public Animator animator;
    private Rigidbody2D rb;
    private MapObj[][] field;
    private Vector2 currentPosition;
    private Queue<Vector2> pathQueue = new Queue<Vector2>();
    private HashSet<Vector2> visited = new HashSet<Vector2>(); // Отслеживание посещенных клеток
    private bool isMoving = false;

void Start()
{
    MapGenerator map = GameObject.FindWithTag("Map")?.GetComponent<MapGenerator>();
    
    if (map == null)
    {
        Debug.LogError("MapGenerator не найден!");
        return;
    }

    field = map.field;
    currentPosition = new Vector2(map.startY, map.startX);
    
    // 🔥 Установка позиции персонажа в стартовую точку
    transform.position = currentPosition;

    rb = GetComponent<Rigidbody2D>();
    animator = GetComponent<Animator>();

    FindFullPath();
    MoveToNextTile();
}


    void FindFullPath()
    {
        int width = field.Length;
        int height = field[0].Length;

        Queue<Vector2> searchQueue = new Queue<Vector2>();
        searchQueue.Enqueue(currentPosition);
        visited.Add(currentPosition);

        Vector2[] directions = {
            new Vector2(1, 0),  // Вправо
            new Vector2(-1, 0), // Влево
            new Vector2(0, 1),  // Вверх
            new Vector2(0, -1)  // Вниз
        };

        while (searchQueue.Count > 0)
        {
            Vector2 position = searchQueue.Dequeue();
            int x = (int)position.x;
            int y = (int)position.y;

            foreach (Vector2 dir in directions)
            {
                int nextX = x + (int)dir.x;
                int nextY = y + (int)dir.y;
                Vector2 nextPos = new Vector2(nextX, nextY);

                if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height &&
                    field[nextX][nextY] == MapObj.Path && !visited.Contains(nextPos))
                {
                    pathQueue.Enqueue(nextPos);
                    searchQueue.Enqueue(nextPos);
                    visited.Add(nextPos);
                }
            }
        }
    }

    void MoveToNextTile()
    {
        if (isMoving || pathQueue.Count == 0) return;

        Vector2 nextPosition = pathQueue.Dequeue();

        if (nextPosition != Vector2.zero)
        {
            isMoving = true;
            animator.SetTrigger("Walk");
            StartCoroutine(MoveCoroutine(nextPosition));
        }
    }

IEnumerator MoveCoroutine(Vector2 targetPosition)
{
    while (Vector2.Distance(transform.position, targetPosition) > 0.01f)
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, 2f * Time.deltaTime);
        yield return null;
    }

    currentPosition = targetPosition;
    isMoving = false;

    Debug.Log("Дошел до: " + currentPosition); // 🔥 Проверка, что персонаж двигается

    MoveToNextTile();
}

}
