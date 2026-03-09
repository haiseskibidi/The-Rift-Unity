using UnityEngine;

public enum UIState
{
    None,
    Dialogue,
    UIOnInteract,
    Inventory,
    Menu,
    DeathScreen
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public UIState CurrentUIState { get; private set; } = UIState.None;

    public delegate void UIStateChangedHandler(UIState newState);
    public event UIStateChangedHandler OnUIStateChanged;

    // Флаг для отслеживания обработки Escape
    public bool IsEscHandled { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Сбрасываем флаг в начале каждого кадра
        IsEscHandled = false;
    }

    /// <summary>
    /// Устанавливает флаг, что Escape был обработан.
    /// </summary>
    public void HandleEsc()
    {
        IsEscHandled = true;
    }

    /// <summary>
    /// Попытка открыть UI. Возвращает true, если успешно.
    /// </summary>
    public bool RequestOpenUI(UIState uiState)
    {
        // Проверка, мёртв ли игрок
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDead)
        {
            Debug.Log("Невозможно открыть UI, так как игрок мёртв.");
            return false;
        }

        if (CurrentUIState == UIState.None)
        {
            CurrentUIState = uiState;
            OnUIStateChanged?.Invoke(CurrentUIState); // Оповещаем подписчиков об изменении состояния
            return true;
        }
        return false;
    }

    /// <summary>
    /// Закрытие текущего UI.
    /// </summary>
    public void CloseCurrentUI()
    {
        CurrentUIState = UIState.None;
        OnUIStateChanged?.Invoke(CurrentUIState); // Оповещаем подписчиков об изменении состояния
    }

    /// <summary>
    /// Проверка, открыт ли какой-либо UI.
    /// </summary>
    public bool IsAnyUIOpen()
    {
        return CurrentUIState != UIState.None;
    }
}