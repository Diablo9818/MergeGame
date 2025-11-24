using UnityEngine;
using System;

public class GameManager : MonoBehaviour, ISaveable
{
     private static GameManager _instance;
    public static GameManager Instance => _instance;

    [SerializeField] private GridManager _gridManager;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private MergeSystem _mergeSystem;
    
    private int _score;
    private int _highScore;

    public int Score 
    { 
        get => _score;
        private set
        {
            _score = value;
            if (_score > _highScore)
            {
                _highScore = _score;
                SaveHighScore();
            }
        }
    }
    
    public void Initialize()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        LoadHighScore();
    }

    private void OnEnable()
    {
        GameEvents.OnScoreChanged += OnScoreChanged;
        GameEvents.OnElementsMerged += OnElementsMerged;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= OnScoreChanged;
        GameEvents.OnElementsMerged -= OnElementsMerged;
    }

    private void OnScoreChanged(int points)
    {
        Score += points;
    }

    private void OnElementsMerged(MergeElement e1, MergeElement e2, MergeResult result)
    {
        Debug.Log($"Merged level {e1.Level} elements! New level: {result.NewLevel}, Points: {result.PointsGained}");
    }

    // ISaveable implementation
    public string Serialize()
    {
        return JsonUtility.ToJson(new SaveData { Score = _score, HighScore = _highScore });
    }

    public void Deserialize(string data)
    {
        var saveData = JsonUtility.FromJson<SaveData>(data);
        _score = saveData.Score;
        _highScore = saveData.HighScore;
    }

    private void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", _highScore);
        PlayerPrefs.Save();
    }

    private void LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    [Serializable]
    private class SaveData
    {
        public int Score;
        public int HighScore;
    }
}
