using System;
using UnityEngine;
using System.Collections.Generic;

public static class GameEvents
{
    // События для демонстрации Event-Driven Architecture
    public static event Action<MergeElement> OnElementSpawned;
    public static event Action<MergeElement, MergeElement, MergeResult> OnElementsMerged;
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnLevelUp;
    public static event Action<Vector2Int> OnCellOccupied;
    public static event Action<Vector2Int> OnCellFreed;
    

    // Методы для вызова событий
    public static void ElementSpawned(MergeElement element) => OnElementSpawned?.Invoke(element);
    public static void ElementsMerged(MergeElement e1, MergeElement e2, MergeResult result) 
        => OnElementsMerged?.Invoke(e1, e2, result);
    public static void ScoreChanged(int score) => OnScoreChanged?.Invoke(score);
    public static void LevelUp(int level) => OnLevelUp?.Invoke(level);
    public static void CellOccupied(Vector2Int pos) => OnCellOccupied?.Invoke(pos);
    public static void CellFreed(Vector2Int pos) => OnCellFreed?.Invoke(pos);
}