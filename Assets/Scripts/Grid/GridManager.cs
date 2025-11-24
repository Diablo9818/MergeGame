using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Управление 3D сеткой игрового поля
/// Координаты: X и Z (горизонтальная плоскость), Y всегда 0
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private GridConfig _config;
    [SerializeField] private Transform _gridParent;
    
    [Header("Visual")]
    [SerializeField] private bool _showGridInGame = false;
    [SerializeField] private Material _gridMaterial;
    [SerializeField] private GameObject _cellPrefab; // Опционально: визуальный prefab ячейки
    
    private GridCell[,] _grid;
    private Dictionary<Vector2Int, MergeElement> _elements;
    private GameObject[,] _visualCells; // Визуальное представление сетки

    public int Width => _config.Width;
    public int Height => _config.Height;
    public float CellSize => _config.CellSize + _config.CellSpacing;

    public void Initialize()
    {
        _elements = new Dictionary<Vector2Int, MergeElement>();
        _grid = InitializeGrid();
        
        if (_showGridInGame && _cellPrefab != null)
        {
            CreateVisualGrid();
        }
    }

    private GridCell[,] InitializeGrid()
    {
        var grid = new GridCell[_config.Width, _config.Height];
        
        for (int x = 0; x < _config.Width; x++)
        {
            for (int z = 0; z < _config.Height; z++)
            {
                grid[x, z] = new GridCell(x, z, false);
            }
        }
        
        return grid;
    }

    private void CreateVisualGrid()
    {
        _visualCells = new GameObject[_config.Width, _config.Height];
        
        for (int x = 0; x < _config.Width; x++)
        {
            for (int z = 0; z < _config.Height; z++)
            {
                Vector3 worldPos = GetWorldPosition(new Vector2Int(x, z));
                GameObject cell = Instantiate(_cellPrefab, worldPos, Quaternion.identity, _gridParent);
                cell.name = $"Cell_{x}_{z}";
                _visualCells[x, z] = cell;
                
                // Настраиваем размер
               // cell.transform.localScale = Vector3.one * _config.CellSize;
            }
        }
    }

    /// <summary>
    /// Конвертирует grid координаты (x, z) в world позицию
    /// </summary>
    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        float cellSize = _config.CellSize + _config.CellSpacing;
        
        // В 3D используем X и Z для горизонтальной плоскости
        // Y = 0 (все элементы на одном уровне)
        return new Vector3(
            gridPos.x * cellSize,
            0f,  // Высота
            gridPos.y * cellSize  // gridPos.y → world Z
        );
    }

    /// <summary>
    /// Конвертирует world позицию в grid координаты
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        float cellSize = CellSize;
        
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / cellSize),
            Mathf.RoundToInt(worldPos.z / cellSize)  // world Z → grid Y
        );
    }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _config.Width 
            && pos.y >= 0 && pos.y < _config.Height;
    }

    public bool IsCellOccupied(Vector2Int pos)
    {
        if (!IsValidPosition(pos) || _grid == null)
            return false;
            
        return _grid[pos.x, pos.y].IsOccupied;
    }

    public void PlaceElement(Vector2Int pos, MergeElement element)
    {
        if (!IsValidPosition(pos) || IsCellOccupied(pos))
        {
            Debug.LogWarning($"Cannot place element at {pos}");
            return;
        }

        _grid[pos.x, pos.y].IsOccupied = true;
        _elements[pos] = element;
        
        // Устанавливаем позицию в 3D мире
        element.transform.position = GetWorldPosition(pos);
        element.transform.rotation = Quaternion.identity;
        
        // Обновляем визуальную ячейку
        UpdateVisualCell(pos, true);
        
        GameEvents.CellOccupied(pos);
    }

    public void RemoveElement(Vector2Int pos)
    {
        if (!IsValidPosition(pos))
            return;

        _grid[pos.x, pos.y].IsOccupied = false;
        _elements.Remove(pos);
        
        UpdateVisualCell(pos, false);
        
        GameEvents.CellFreed(pos);
    }

    public MergeElement GetElementAt(Vector2Int pos)
    {
        return _elements.TryGetValue(pos, out var element) ? element : null;
    }

    public List<Vector2Int> GetEmptyCells()
    {
        if (_grid == null)
        {
            Debug.LogError("Grid is not initialized!");
            return new List<Vector2Int>();
        }

        var emptyCells = new List<Vector2Int>();
        
        for (int x = 0; x < _config.Width; x++)
        {
            for (int z = 0; z < _config.Height; z++)
            {
                if (!_grid[x, z].IsOccupied)
                {
                    emptyCells.Add(new Vector2Int(x, z));
                }
            }
        }
        
        return emptyCells;
    }

    public IEnumerable<MergeElement> GetMergeableNeighbors(MergeElement element)
    {
        var pos = element.GridPosition;
        var directions = new[] 
        { 
            Vector2Int.up,    // Z+
            Vector2Int.down,  // Z-
            Vector2Int.left,  // X-
            Vector2Int.right  // X+
        };

        return directions
            .Select(dir => pos + dir)
            .Where(IsValidPosition)
            .Select(GetElementAt)
            .Where(e => e != null && e.Level == element.Level);
    }

    private void UpdateVisualCell(Vector2Int pos, bool occupied)
    {
        if (!_showGridInGame || _visualCells == null)
            return;
            
        if (!IsValidPosition(pos))
            return;

        var cell = _visualCells[pos.x, pos.y];
        if (cell != null)
        {
            var renderer = cell.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                // Меняем цвет в зависимости от состояния
                renderer.material.color = occupied 
                    ? _config.OccupiedColor 
                    : _config.GridColor;
            }
        }
    }

    // Debug визуализация сетки
    private void OnDrawGizmos()
    {
        if (_config == null)
            return;

        float cellSize = _config.CellSize + _config.CellSpacing;
        
        for (int x = 0; x < _config.Width; x++)
        {
            for (int z = 0; z < _config.Height; z++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, z * cellSize);
                
                // Цвет зависит от состояния
                if (_grid != null && _grid[x, z] != null && _grid[x, z].IsOccupied)
                    Gizmos.color = _config.OccupiedColor;
                else
                    Gizmos.color = _config.GridColor;
                
                // Рисуем куб на ground level
                Gizmos.DrawWireCube(pos, new Vector3(_config.CellSize, 0.1f, _config.CellSize));
                
                // Рисуем линии сетки
                Gizmos.color = Color.white * 0.5f;
                
                // Горизонтальные линии (по X)
                if (z == 0)
                {
                    Vector3 start = new Vector3(x * cellSize, 0, 0);
                    Vector3 end = new Vector3(x * cellSize, 0, (_config.Height - 1) * cellSize);
                    Gizmos.DrawLine(start, end);
                }
                
                // Вертикальные линии (по Z)
                if (x == 0)
                {
                    Vector3 start = new Vector3(0, 0, z * cellSize);
                    Vector3 end = new Vector3((_config.Width - 1) * cellSize, 0, z * cellSize);
                    Gizmos.DrawLine(start, end);
                }
            }
        }
        
        // Рамка вокруг сетки
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (_config.Width - 1) * cellSize / 2f, 
            0, 
            (_config.Height - 1) * cellSize / 2f
        );
        Vector3 size = new Vector3(
            _config.Width * cellSize, 
            0.1f, 
            _config.Height * cellSize
        );
        Gizmos.DrawWireCube(center, size);
    }

    // Вспомогательный метод для создания визуальной сетки из кода
    public void CreateProceduralGrid()
    {
        GameObject gridObject = new GameObject("ProceduralGrid");
        gridObject.transform.SetParent(_gridParent);
        
        MeshFilter meshFilter = gridObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gridObject.AddComponent<MeshRenderer>();
        
        if (_gridMaterial != null)
            meshRenderer.material = _gridMaterial;
        
        // Создаем mesh для grid
        Mesh gridMesh = GenerateGridMesh();
        meshFilter.mesh = gridMesh;
    }

    private Mesh GenerateGridMesh()
    {
        Mesh mesh = new Mesh();
        float cellSize = _config.CellSize + _config.CellSpacing;
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();
        
        // Генерируем линии сетки
        for (int x = 0; x <= _config.Width; x++)
        {
            Vector3 start = new Vector3(x * cellSize, 0, 0);
            Vector3 end = new Vector3(x * cellSize, 0, _config.Height * cellSize);
            
            vertices.Add(start);
            vertices.Add(end);
        }
        
        for (int z = 0; z <= _config.Height; z++)
        {
            Vector3 start = new Vector3(0, 0, z * cellSize);
            Vector3 end = new Vector3(_config.Width * cellSize, 0, z * cellSize);
            
            vertices.Add(start);
            vertices.Add(end);
        }
        
        // Индексы для линий
        for (int i = 0; i < vertices.Count; i++)
        {
            indices.Add(i);
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Lines, 0);
        mesh.RecalculateBounds();
        
        return mesh;
    }
}