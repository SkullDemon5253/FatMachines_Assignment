using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grid Settings")]
    public int columns = 6;
    public int rows = 6;
    public float cellSize = 1f;
    public Vector3 origin = new Vector3(-0.5f, 0, -4.5f);

    private Dictionary<Vector2Int, BoxController> occupiedCells = new Dictionary<Vector2Int, BoxController>();

    [Header("Debug")]
    public bool drawGridGizmos = true; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - origin.x) / cellSize);
        int z = Mathf.RoundToInt((worldPos.z - origin.z) / cellSize);
        return new Vector2Int(x, z);
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        float x = origin.x + cell.x * cellSize;
        float z = origin.z + cell.y * cellSize;
        return new Vector3(x, origin.y, z);
    }

    public bool IsWithinBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows;
    }

    public bool IsOccupied(Vector2Int cell)
    {
        return occupiedCells.ContainsKey(cell);
    }

    public bool TryOccupy(Vector2Int originCell, BoxController box)
    {
        List<Vector2Int> cells = GetOccupiedCells(originCell, box.Size);

        foreach (var cell in cells)
        {
            if (!IsWithinBounds(cell) || IsOccupied(cell))
                return false;
        }

        // Clear old
        foreach (var cell in box.OccupiedCells)
        {
            ClearCell(cell);
        }

        // Occupy all
        foreach (var cell in cells)
        {
            occupiedCells[cell] = box;
        }

        box.CurrentCell = originCell;
        box.OccupiedCells = cells;
        return true;
    }

    public void ClearCell(Vector2Int cell)
    {
        if (occupiedCells.ContainsKey(cell))
            occupiedCells.Remove(cell);
    }

    public void SnapBoxToCell(BoxController box, Vector2Int originCell)
    {
        if (!TryOccupy(originCell, box)) return;

        Vector3 targetPos = CellToWorld(originCell);
        box.transform.position = new Vector3(targetPos.x, box.transform.position.y, targetPos.z);
    }

    private List<Vector2Int> GetOccupiedCells(Vector2Int origin, Vector2Int size)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                cells.Add(new Vector2Int(origin.x + x, origin.y + y));
            }
        }
        return cells;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGridGizmos) return;

        // Draw grid lines
        Gizmos.color = Color.green;
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3 center = CellToWorld(new Vector2Int(x, y));
                Gizmos.DrawWireCube(center, new Vector3(cellSize, 0.1f, cellSize));
            }
        }

        // Draw occupied cells in red
        Gizmos.color = Color.red;
        foreach (var cell in occupiedCells.Keys)
        {
            Vector3 center = CellToWorld(cell);
            Gizmos.DrawCube(center, new Vector3(cellSize, 0.1f, cellSize) * 0.9f);
        }

        // Draw origin marker
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(origin, 0.2f);
    }
#endif
}