using System;
using UnityEngine;


[CreateAssetMenu(fileName = "GridConfig", menuName = "Merge Game/Grid Config", order = 1)]
public class GridConfig : ScriptableObject
{
    [Header("Размер сетки")]
    [Tooltip("Количество ячеек по горизонтали")]
    [Range(3, 10)]
    public int Width = 5;
    
    [Tooltip("Количество ячеек по вертикали")]
    [Range(3, 10)]
    public int Height = 5;
    
    [Header("Размеры ячеек")]
    [Tooltip("Размер одной ячейки в Unity units")]
    [Range(0.5f, 2f)]
    public float CellSize = 1.0f;
    
    [Tooltip("Отступ между ячейками")]
    [Range(0f, 0.5f)]
    public float CellSpacing = 0.1f;
    
    [Header("Визуализация")]
    [Tooltip("Цвет сетки в редакторе")]
    public Color GridColor = new Color(0, 1, 0, 0.3f);
    
    [Tooltip("Цвет занятых ячеек")]
    public Color OccupiedColor = new Color(1, 0, 0, 0.3f);
    
    public float TotalCellSize => CellSize + CellSpacing;
    public int TotalCells => Width * Height;
    public Vector2 GridWorldSize => new Vector2(Width * TotalCellSize, Height * TotalCellSize);
    
    private void OnValidate()
    {
        if (Width < 3) Width = 3;
        if (Height < 3) Height = 3;
        if (CellSize < 0.5f) CellSize = 0.5f;
        if (CellSpacing < 0) CellSpacing = 0;
    }
}