using UnityEngine;
using UnityEngine.EventSystems; // ���������� ��� �������� ������ �� UI

public class TowerPlacementManager : MonoBehaviour
{
    public static TowerPlacementManager Instance; // �������� ��� ������� �������

    public GameObject selectedTowerPrefab = null; // ������ ��������� ����� ��� ���������
    public LayerMask tileLayerMask;       // ����� ����, ����� Raycast ������� ������ �� �����
    public Camera mainCamera;             // ������ �� �������� ������

    void Awake()
    {
        // ��������� ���������
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // ���������� ��������, ���� �� ����
            return;
        }
    }

    void Start()
    {
        // ������� ����� ������, ���� ��� �� ��������� �������
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (mainCamera == null)
        {
            Debug.LogError("TowerPlacementManager: Main Camera �� �������!");
        }
    }

    void Update()
    {
        // 1. ���������, ������� �� ����� ��� ���������
        if (selectedTowerPrefab == null)
        {
            // ���� ����� �� �������, ������ �� ������ � Update
            return;
        }

        // 2. ������ ������/��������� �� ������ ������ ����
        if (Input.GetMouseButtonDown(1)) // 1 = ������ ������ ����
        {
            DeselectTower();
            Debug.Log("��������� ��������");
            return;
        }

        // 3. ��������� ����� ����� ������� ���� ��� ���������
        if (Input.GetMouseButtonDown(0)) // 0 = ����� ������ ����
        {
            // 4. ���������� ����, ���� �� ����� �� ������� UI (������ � �.�.)
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("���� �� UI, ��������� ������������.");
                return;
            }

            // 5. �������� ���������� �����
            PlaceSelectedTower();
        }

        // --- �������������: ������������ �������� ����� ��� �������� ---
        // ����� �������� ���, ������� ����� ���������� �������������� �����
        // selectedTowerPrefab ��� �������� ���� ��� ���������� �������.
        // ��� ������� UX, �� �������� ���.
    }

    // �����, ���������� �������� UI ��� ������ �����
    public void SelectTowerToPlace(GameObject towerPrefab)
    {
        selectedTowerPrefab = towerPrefab;
        Debug.Log($"������� �����: {towerPrefab.name}");
        // ����� ����� �������� ������ �������� �����, ���� �����
    }

    // ����� ��� ������ ������ �����
    public void DeselectTower()
    {
        selectedTowerPrefab = null;
        // ������ ������������ ��������, ���� ��� ����
    }

    // ������ ��������� �����
    // ������ ��������� ����� (������ ��� 2D)
    private void PlaceSelectedTower()
    {
        // �������� ������� ���� � ������� ����������� ��� 2D
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // ������� ��� �� ����� ���� (��� ����� �� ������, �� ��� ��������������� 2D ��� �����)
        // Physics2D.Raycast ������� ��������� ����� � �����������. ��� ����� ���� ����������� �� �����,
        // ����� ������ ��������� ����� ��� ������.
        // ��������� ��������� 0, ����� ��������� ������ ����� ��� ��������.
        // � ���������� ���� ����� ���� ������.
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, 0f, tileLayerMask);

        // ���������, ������ �� �� � ��������� �� ������ ����
        if (hit.collider != null)
        {
            // �������� ������ �����, �� ������� ����� ���
            TileScript tile = hit.collider.GetComponent<TileScript>();

            // ���������, ��� ���� ����������, �� ��� ����� ������� � �� �� �����
            if (tile != null && tile.isBuildable && !tile.hasTower)
            {
                // ������� ��������� ����� �� �������
                // �������: ����� �����. ���������� ������� ����������/�����.
                Vector3 placementPosition = hit.transform.position;
                // ����� �������� ��������� ������ �� Z, ����� ����� ���� "����" �����, ���� �����
                // placementPosition.z -= 0.1f;

                GameObject newTower = Instantiate(selectedTowerPrefab, placementPosition, Quaternion.identity);

                // �������� ���� ��� �������
                tile.hasTower = true;
                // tile.towerInstance = newTower; // ���� ����� ������� ������

                Debug.Log($"����� {selectedTowerPrefab.name} ����������� �� ���� {tile.name} (����������: {hit.transform.position})");

                DeselectTower();

                // ����� ����� �������� ������ �������� �������� �� �����
            }
            else
            {
                // ��������, ������ ������ ����������
                if (tile == null) Debug.LogWarning($"���� ����� �� ��������� ���� '{LayerMask.LayerToName(hit.collider.gameObject.layer)}', �� ��� TileScript! ������: {hit.collider.name}");
                else if (!tile.isBuildable) Debug.Log($"�� ���� ����� ({hit.collider.name}) ������� ������.");
                else if (tile.hasTower) Debug.Log($"���� ���� ({hit.collider.name}) ��� �����.");
            }
        }
        else // ���� ��������, ���� Physics2D.Raycast ������ �� ����� ��� �������� �� ��������� ����
        {
            Debug.Log("���� ���� ������ (��� �������� �� ��������� ���� � ������������).");
        }
    }
}