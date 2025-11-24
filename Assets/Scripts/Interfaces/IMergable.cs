using UnityEngine;
public interface IMergeable
{
    int Level { get; }
    Vector2Int GridPosition { get; set; }
    void Initialize(int level, Vector2Int position);
    void Merge(IMergeable other);
    bool CanMergeWith(IMergeable other);
}