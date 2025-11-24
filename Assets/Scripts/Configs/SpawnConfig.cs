using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "SpawnConfig", menuName = "Merge Game/Spawn Config", order = 2)]
public class SpawnConfig : ScriptableObject
{
    [Header("Настройки спавна")]
    [Tooltip("Интервал между спавнами в секундах")]
    [Range(0.5f, 10f)]
    public float SpawnInterval = 3.0f;
    
    [Tooltip("Максимальный уровень элементов в игре")]
    [Range(5, 20)]
    public int MaxLevel = 10;
    
    [Tooltip("Уровень новых элементов при спавне")]
    [Range(1, 3)]
    public int InitialSpawnLevel = 1;
    
    [Header("Дополнительно")]
    [Tooltip("Спавнить элементы сразу при старте")]
    public bool SpawnOnStart = true;
    
    [Tooltip("Количество элементов для начального спавна")]
    [Range(0, 10)]
    public int InitialSpawnCount = 3;

    private void OnValidate()
    {
        if (SpawnInterval < 0.5f) SpawnInterval = 0.5f;
        if (InitialSpawnLevel < 1) InitialSpawnLevel = 1;
        if (InitialSpawnLevel > 3) InitialSpawnLevel = 3;
    }
}
