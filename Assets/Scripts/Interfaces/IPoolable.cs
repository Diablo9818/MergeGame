public interface IPoolable
{
    void OnSpawn();
    void OnDespawn();
    bool IsActive { get; }
}