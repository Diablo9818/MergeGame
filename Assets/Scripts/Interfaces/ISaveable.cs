public interface ISaveable
{
    string Serialize();
    void Deserialize(string data);
}