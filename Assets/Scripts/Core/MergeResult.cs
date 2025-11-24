using UnityEngine;
using System;

[Serializable]
public struct MergeResult
{
    public Vector2Int Position { get; }
    public int NewLevel { get; }
    public int PointsGained { get; }

    public MergeResult(Vector2Int position, int newLevel, int pointsGained)
    {
        Position = position;
        NewLevel = newLevel;
        PointsGained = pointsGained;
    }

    // Equality members для корректного сравнения
    public override bool Equals(object obj)
    {
        return obj is MergeResult result &&
               Position.Equals(result.Position) &&
               NewLevel == result.NewLevel &&
               PointsGained == result.PointsGained;
    }

    public override int GetHashCode()
    {
        int hashCode = 17;
        hashCode = hashCode * 31 + Position.GetHashCode();
        hashCode = hashCode * 31 + NewLevel.GetHashCode();
        hashCode = hashCode * 31 + PointsGained.GetHashCode();
        return hashCode;
    }

    public static bool operator ==(MergeResult left, MergeResult right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MergeResult left, MergeResult right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"MergeResult(Position: {Position}, Level: {NewLevel}, Points: {PointsGained})";
    }
}