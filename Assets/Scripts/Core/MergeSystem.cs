using UnityEngine;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Система merge для 3D игры с ИСПРАВЛЕННОЙ логикой проверки
/// </summary>
public class MergeSystem : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GridManager _gridManager;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private AudioManager _audioManager;
    
    [Header("Raycast Settings")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _elementLayerMask = -1;
    [SerializeField] private float _raycastDistance = 100f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color _selectedColor = Color.yellow;
    [SerializeField] private GameObject _selectionIndicatorPrefab;
    
    private MergeElement _selectedElement;
    private GameObject _selectionIndicator;
    

    public void Initialize()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;
            
        if (_selectionIndicatorPrefab != null)
        {
            _selectionIndicator = Instantiate(_selectionIndicatorPrefab);
            _selectionIndicator.SetActive(false);
        } 
    }

    private void OnEnable()
    {
        GameEvents.OnElementSpawned += OnElementSpawned;
    }

    private void OnDisable()
    {
        GameEvents.OnElementSpawned -= OnElementSpawned;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        bool mouseClicked = false;
        Vector2 mousePosition = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            mouseClicked = true;
            mousePosition = Mouse.current.position.ReadValue();
        }
#else
        if (Input.GetMouseButtonDown(0))
        {
            mouseClicked = true;
            mousePosition = Input.mousePosition;
        }
#endif

        if (mouseClicked)
        {
            HandleClick(mousePosition);
        }
    }

    private void HandleClick(Vector2 screenPosition)
    {
        Ray ray = _mainCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _elementLayerMask))
        {
            MergeElement clickedElement = hit.collider.GetComponent<MergeElement>();
            
            if (clickedElement != null)
            {
                HandleElementClick(clickedElement);
            }
            else
            {
                DeselectElement();
            }
        }
        else
        {
            DeselectElement();
        }
        
        Debug.DrawRay(ray.origin, ray.direction * _raycastDistance, Color.red, 1f);
    }

    private void HandleElementClick(MergeElement clickedElement)
    {
        // СЛУЧАЙ 1: Ничего не выбрано - просто выбираем
        if (_selectedElement == null)
        {
            SelectElement(clickedElement);
            Debug.Log($"Selected element at {clickedElement.GridPosition}, Level {clickedElement.Level}");
            return;
        }
        
        // СЛУЧАЙ 2: Кликнули на тот же элемент - отменяем выбор
        if (_selectedElement == clickedElement)
        {
            Debug.Log("Deselected element");
            DeselectElement();
            return;
        }
        
        // СЛУЧАЙ 3: Выбран другой элемент - проверяем merge
        // ВАЖНО: CanMergeWith проверяет ВСЁ (уровень И соседство)
        if (_selectedElement.CanMergeWith(clickedElement))
        {
            Debug.Log($"MERGE! {_selectedElement.GridPosition} + {clickedElement.GridPosition}");
            MergeElements(_selectedElement, clickedElement);
        }
        else
        {
            // НЕЛЬЗЯ merge - объясняем почему и переключаемся
            string reason = GetMergeFailReason(_selectedElement, clickedElement);
            Debug.LogWarning($"Cannot merge: {reason}");
            
            // Переключаемся на новый элемент
            DeselectElement();
            SelectElement(clickedElement);
        }
    }

    /// <summary>
    /// Диагностика: почему merge не работает
    /// </summary>
    private string GetMergeFailReason(MergeElement elem1, MergeElement elem2)
    {
        // Единственная причина: разные уровни
        if (elem1.Level != elem2.Level)
        {
            return $"Different levels (Level {elem1.Level} vs Level {elem2.Level})";
        }
        
        return "Unknown reason";
    }

    private void SelectElement(MergeElement element)
    {
        _selectedElement = element;
        
        // Визуальное увеличение
        element.transform.localScale *= 1.2f;
        
        // Эффект свечения
        AddSelectionOutline(element);
        
        // Индикатор под элементом
        if (_selectionIndicator != null)
        {
            _selectionIndicator.SetActive(true);
            _selectionIndicator.transform.position = element.transform.position + Vector3.down * 0.5f;
            _selectionIndicator.transform.SetParent(element.transform);
        }
        
        // Показываем информацию о выбранном элементе
        ShowElementInfo(element.GridPosition);
    }

    private void DeselectElement()
    {
        if (_selectedElement != null)
        {
            // Возвращаем размер
            _selectedElement.transform.localScale /= 1.2f;
            
            // Убираем свечение
            RemoveSelectionOutline(_selectedElement);
            
            // Прячем индикатор
            if (_selectionIndicator != null)
            {
                _selectionIndicator.SetActive(false);
                _selectionIndicator.transform.SetParent(transform);
            }
            
            _selectedElement = null;
        }
    }

    private void MergeElements(MergeElement element1, MergeElement element2)
    {
        var pos1 = element1.GridPosition;
        var pos2 = element2.GridPosition;
        var newLevel = element1.Level + 1;

        Debug.Log($"Starting merge animation: {pos1} + {pos2} → Level {newLevel}");

        // Запускаем анимацию merge
        StartCoroutine(MergeAnimation(element1, element2, pos1, newLevel));
    }

    private IEnumerator MergeAnimation(
        MergeElement element1, 
        MergeElement element2, 
        Vector2Int targetPos, 
        int newLevel)
    {
        // Анимация: element2 летит к element1
        Vector3 startPos = element2.transform.position;
        Vector3 endPos = element1.transform.position;
        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 midPoint = (startPos + endPos) / 2 + Vector3.up * 0.5f;
            Vector3 pos = Vector3.Lerp(
                Vector3.Lerp(startPos, midPoint, t),
                Vector3.Lerp(midPoint, endPos, t),
                t
            );

            element2.transform.position = pos;
            element2.transform.Rotate(Vector3.up, 720f * Time.deltaTime);

            yield return null;
        }
        
        element1.Merge(element2);
        element2.Merge(element1);
        

        // Снимаем оба элемента с сетки (они больше не должны считаться занятыми)
        var pos1 = element1.GridPosition;
        var pos2 = element2.GridPosition;

        _gridManager.RemoveElement(pos1);
        _gridManager.RemoveElement(pos2);

        // Создаем новый элемент повышенного уровня
        var newElement = _spawnManager.CreateElementAt(targetPos, newLevel);

        // Вычисляем очки
        int points = CalculatePoints(newLevel);
        var result = new MergeResult(targetPos, newLevel, points);

        // Уведомляем систему
        GameEvents.ElementsMerged(element1, element2, result);
        GameEvents.ScoreChanged(points);

        // Звук
        if (_audioManager != null)
            _audioManager.PlayMergeSound(newLevel);

        Debug.Log($"Merge complete! New element at {targetPos}, Level {newLevel}, +{points} points");

        // Снимаем выделение до возврата/деактивации объектов
        DeselectElement();

        // Возвращаем старые элементы в пул (или уничтожаем, если нет пула)
        _spawnManager.ReturnElement(element1);
        _spawnManager.ReturnElement(element2);

        // Готово
    }

    private int CalculatePoints(int level)
    {
        return (int)Mathf.Pow(2, level) * 10;
    }

    private void OnElementSpawned(MergeElement element)
    {
        if (_audioManager != null)
            _audioManager.PlaySpawnSound();
    }

    private void AddSelectionOutline(MergeElement element)
    {
        var renderer = element.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.EnableKeyword("_EMISSION");
            renderer.material.SetColor("_EmissionColor", _selectedColor * 0.5f);
        }
    }

    private void RemoveSelectionOutline(MergeElement element)
    {
        var renderer = element.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.DisableKeyword("_EMISSION");
        }
    }

    /// <summary>
    /// DEBUG: Показывает информацию о выбранном элементе
    /// </summary>
    private void ShowElementInfo(Vector2Int pos)
    {
        Debug.Log($"=== Selected element at {pos} ===");
        Debug.Log($"Level: {_selectedElement.Level}");
        Debug.Log($"Can merge with ANY other Level {_selectedElement.Level} element");
    }

    // Debug визуализация
    private void OnDrawGizmos()
    {
        if (_selectedElement != null)
        {
            // Выделенный элемент - желтая сфера
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_selectedElement.transform.position, 0.7f);
            
            // Показываем ВСЕ элементы того же уровня на сетке
            if (_gridManager != null)
            {
                for (int x = 0; x < _gridManager.Width; x++)
                {
                    for (int z = 0; z < _gridManager.Height; z++)
                    {
                        Vector2Int pos = new Vector2Int(x, z);
                        var element = _gridManager.GetElementAt(pos);
                        
                        if (element != null && element != _selectedElement && element.Level == _selectedElement.Level)
                        {
                            // Элемент того же уровня - можно merge
                            Gizmos.color = Color.green;
                            Gizmos.DrawLine(_selectedElement.transform.position, element.transform.position);
                            Gizmos.DrawWireSphere(element.transform.position, 0.5f);
                        }
                    }
                }
            }
        }
    }
}