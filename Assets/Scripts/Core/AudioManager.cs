using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    var go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioClip _mergeSound;
    [SerializeField] private AudioClip _spawnSound;
    [SerializeField] private AudioClip _levelUpSound;
    
    
    public void Initialize()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMergeSound(int level)
    {
        if (_mergeSound != null)
        {
            // Изменяем pitch в зависимости от уровня
            _sfxSource.pitch = 1f + (level * 0.1f);
            _sfxSource.PlayOneShot(_mergeSound);
        }
    }

    public void PlaySpawnSound()
    {
        if (_spawnSound != null)
            _sfxSource.PlayOneShot(_spawnSound);
    }

    public void PlayLevelUpSound()
    {
        if (_levelUpSound != null)
            _sfxSource.PlayOneShot(_levelUpSound);
    }
}
