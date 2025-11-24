using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private MergeSystem _mergeSystem;
    [SerializeField] private AudioManager _audioManager;
    
    void Start()
    {
        _gameManager.Initialize();
        _gridManager.Initialize();
        _spawnManager.Initialize();
        _mergeSystem.Initialize();
        _audioManager.Initialize();
    }
}
