using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private SpawnConfig _config;
    [SerializeField] private MergeElement _elementPrefab;
    [SerializeField] private GridManager _gridManager;
    
    private ObjectPool<MergeElement> _elementPool;
    private Coroutine _spawnCoroutine;
    private bool _isSpawning;

    public bool IsSpawning 
    { 
        get => _isSpawning;
        set
        {
            _isSpawning = value;
            if (value && _spawnCoroutine == null)
                _spawnCoroutine = StartCoroutine(SpawnRoutine());
            else if (!value && _spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }
    }
    
    public void Initialize()
    {
        _elementPool = new ObjectPool<MergeElement>(
            _elementPrefab, 
            transform, 
            initialSize: 20
        );
    }

    private void Start()
    {
        IsSpawning = true;
    }

    // Coroutine для периодического спавна
    private IEnumerator SpawnRoutine()
    {
        var wait = new WaitForSeconds(_config.SpawnInterval);
        
        while (_isSpawning)
        {
            yield return  wait;
            TrySpawnElement();
        }
    }

    public void TrySpawnElement()
    {
        var emptyCells = _gridManager.GetEmptyCells();
        
        if (emptyCells.Count == 0)
        {
            Debug.Log("Grid is full!");
            return;
        }

        // Используем LINQ для случайного выбора
        var randomCell = emptyCells
            .OrderBy(_ => UnityEngine.Random.value)
            .First();

        SpawnElement(randomCell, _config.InitialSpawnLevel);
    }

    // Factory Method
    private MergeElement SpawnElement(Vector2Int position, int level)
    {
        var element = _elementPool.Get();
        element.Initialize(level, position);
        _gridManager.PlaceElement(position, element);
        
        GameEvents.ElementSpawned(element);
        
        return element;
    }

    public MergeElement CreateElementAt(Vector2Int position, int level)
    {
        return SpawnElement(position, level);
    }
    
    public void ReturnElement(MergeElement element)
    {
        if (element == null)
            return;
        
        _elementPool?.Return(element);
    }

    private void OnDestroy()
    {
        IsSpawning = false;
        _elementPool?.Clear();
    }
}
