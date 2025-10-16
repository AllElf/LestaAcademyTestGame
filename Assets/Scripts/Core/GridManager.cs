using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Настройки сетки")]
    public Vector2Int gridSize = new Vector2Int(8, 8);
    public Cell cellPrefab;
    public float cellSpacing = 1f;
    public Transform gridParent;

    private List<Cell> allCells = new List<Cell>();
    public List<Cell> AllCells => allCells;

    public void GenerateGrid()
    {
        ClearGrid();
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 pos = new Vector3(x * cellSpacing, 0, y * cellSpacing);
                Cell newCell = Instantiate(cellPrefab, pos, Quaternion.identity, gridParent);
                newCell.name = $"Cell_{x}_{y}";
                newCell.gridPosition = new Vector2Int(x, y);
                newCell.SetDefault();
                allCells.Add(newCell);
            }
        }
    }

    public void ClearGrid()
    {
        foreach (var c in allCells)
        {
            if (c != null)
                Destroy(c.gameObject);
        }
        allCells.Clear();
    }

    public Cell GetRandomFreeCell()
    {
        List<Cell> free = allCells.FindAll(c => !c.IsOccupied);
        if (free.Count == 0) return null;
        return free[Random.Range(0, free.Count)];
    }
}
