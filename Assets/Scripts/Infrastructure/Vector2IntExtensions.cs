using UnityEngine;

public static  class Vector2IntExtensions
{
    public static float DistanceTo(this Vector2Int a, Vector2Int b)
    {
        return Vector2Int.Distance(a, b);
    }

    public static bool IsAdjacentTo(this Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }
}
