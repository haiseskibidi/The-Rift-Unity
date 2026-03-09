using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI levelTextTMP;

    private void Start()
    {
        // Инициализация отображения уровня при старте
        UpdateLevelDisplay();

        // Подписываемся на события изменения уровня из LevelManager
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelChanged += HandleLevelChanged;
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий, чтобы избежать ошибок
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelChanged -= HandleLevelChanged;
        }
    }

    /// <summary>
    /// Обработчик события изменения уровня.
    /// </summary>
    /// <param name="newLevel">Новый уровень игрока.</param>
    private void HandleLevelChanged(int newLevel)
    {
        UpdateLevelDisplay();
    }

    /// <summary>
    /// Метод для обновления отображения уровня в UI.
    /// </summary>
    private void UpdateLevelDisplay()
    {
        if (LevelManager.Instance == null)
            return;

        int currentLevel = LevelManager.Instance.GetCurrentLevel();

        // Обновляем текст в зависимости от используемого компонента
        if (levelTextTMP != null)
        {
            levelTextTMP.text = $"{currentLevel}";
        }
        else
        {
            Debug.LogWarning("Не назначен ни TextMeshProUGUI, ни Text для отображения уровня.");
        }
    }
}