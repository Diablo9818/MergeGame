using UnityEngine;
using System;

[Serializable]
public class GridCell
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public bool IsOccupied { get; set; }

    public GridCell(int x, int y, bool isOccupied = false)
    {
        X = x;
        Y = y;
        IsOccupied = isOccupied;
    }

    public Vector2Int Position => new Vector2Int(X, Y);

    public override bool Equals(object obj)
    {
        return obj is GridCell cell &&
               X == cell.X &&
               Y == cell.Y;
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }

    public override string ToString()
    {
        return $"Cell({X}, {Y}) - {(IsOccupied ? "Occupied" : "Empty")}";
    }
}
