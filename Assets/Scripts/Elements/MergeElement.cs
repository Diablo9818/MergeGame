using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(Collider))]
public class MergeElement : MonoBehaviour, IMergeable, IPoolable
{
    [Header("3D Components")]
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private Animator _animator;
    [SerializeField] private ParticleSystem _mergeParticles;
    
    [Header("Visual Settings")]
    [SerializeField] private Material _baseMaterial;
    [SerializeField] private Mesh[] _levelMeshes;
    
    private int _level;
    public int Level 
    { 
        get => _level;
        private set
        {
            _level = value;
            UpdateVisuals();
        }
    }

    public Vector2Int GridPosition { get; set; }
    public bool IsActive { get; private set; }
    
    public Color ElementColor => GetColorForLevel(Level);
    
    private static readonly Color[] LevelColors = new[]
    {
        new Color(1f, 1f, 1f),      // 1: White
        new Color(1f, 1f, 0f),      // 2: Yellow
        new Color(0f, 1f, 0f),      // 3: Green
        new Color(0f, 1f, 1f),      // 4: Cyan
        new Color(0f, 0f, 1f),      // 5: Blue
        new Color(1f, 0f, 1f),      // 6: Magenta
        new Color(1f, 0f, 0f),      // 7: Red
        new Color(1f, 0.5f, 0f),    // 8: Orange
        new Color(0.5f, 0f, 1f),    // 9: Purple
        new Color(1f, 0.8f, 0f)     // 10: Gold
    };

    private Material _instanceMaterial;

    private void Awake()
    {
        if (_meshRenderer == null)
            _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();
        
        if (_baseMaterial != null)
        {
            _instanceMaterial = new Material(_baseMaterial);
            _meshRenderer.material = _instanceMaterial;
        }
    }

    public void Initialize(int level, Vector2Int position)
    {
        Level = level;
        GridPosition = position;
        IsActive = true;
        
        transform.rotation = Quaternion.identity;
        
        UpdateVisuals();
        
        Debug.Log($"[MergeElement] Initialized at {position}, Level {level}");
    }
    
    public bool CanMergeWith(IMergeable other)
    {
        if (other == null)
        {
            return false;
        }
        
        if (ReferenceEquals(other, this))
        {
            return false;
        }
        
        if (other.Level != this.Level)
        {
            return false;
        }
        
        
        return true;
    }

    public void Merge(IMergeable other)
    {
        Debug.Log($"[MergeElement] Merging {other}");
        
        Debug.Log($"[Particles] Before Play - IsPlaying: {_mergeParticles.isPlaying}");
        
        _mergeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _mergeParticles.Clear();
        _mergeParticles.Play();
        
        Debug.Log($"[Particles] After Play - IsPlaying: {_mergeParticles.isPlaying}");
        Debug.Log($"[Particles] Particle count: {_mergeParticles.particleCount}");
        _mergeParticles.Emit(30);
        
    }

    private void UpdateVisuals()
    {
        if (_meshRenderer == null) return;

        // 🔥 СТАВИМ наш материал снова (Unity иногда меняет ссылку!)
        if (_instanceMaterial != null)
            _meshRenderer.sharedMaterial = _instanceMaterial;

        // Цвет
        if (_instanceMaterial != null)
        {
            _instanceMaterial.color = ElementColor;

            if (Level >= 5)
            {
                _instanceMaterial.EnableKeyword("_EMISSION");
                _instanceMaterial.SetColor("_EmissionColor", ElementColor * 0.3f);
            }
            else
            {
                _instanceMaterial.DisableKeyword("_EMISSION");
            }
        }

        // Скейл
        float scale = 0.8f + (Level * 0.1f);
        transform.localScale = Vector3.one * scale;

        // Меши
        if (_levelMeshes != null && _levelMeshes.Length > 0 && _meshFilter != null)
        {
            int meshIndex = Mathf.Min(Level - 1, _levelMeshes.Length - 1);
            if (meshIndex >= 0 && _levelMeshes[meshIndex] != null)
            {
                _meshFilter.mesh = _levelMeshes[meshIndex];
            }
        }
    }

    private Color GetColorForLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, LevelColors.Length - 1);
        return LevelColors[index];
    }

    public void OnSpawn()
    {
        IsActive = true;
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        IsActive = false;
        gameObject.SetActive(false);
        StopAllCoroutines();
    }

    public static bool operator ==(MergeElement a, MergeElement b)
    {
        if (ReferenceEquals(a, null))
            return ReferenceEquals(b, null);
        return a.Equals(b);
    }

    public static bool operator !=(MergeElement a, MergeElement b) => !(a == b);

    public override bool Equals(object obj)
    {
        if (obj is MergeElement other)
            return Level == other.Level && GridPosition == other.GridPosition;
        return false;
    }

    public override int GetHashCode() 
    {
        return System.HashCode.Combine(Level, GridPosition);
    }

    private void OnDestroy()
    {
        if (_instanceMaterial != null)
        {
            Destroy(_instanceMaterial);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.6f);
    }
}