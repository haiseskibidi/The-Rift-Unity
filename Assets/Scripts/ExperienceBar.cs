using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ExperienceBar : MonoBehaviour
{
    [SerializeField]
    private Image fillImage; // Ссылка на Image с типом Filled

    [SerializeField]
    private float animationDuration = 0.5f; // Длительность анимации в секундах

    private Coroutine fillCoroutine;

    private void Start()
    {
        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
        }

        UpdateExperienceBarImmediate();

        // Подписываемся на события изменения опыта и уровня
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnExperienceChanged += OnExperienceChanged;
            LevelManager.Instance.OnLevelChanged += OnLevelChanged;
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnExperienceChanged -= OnExperienceChanged;
            LevelManager.Instance.OnLevelChanged -= OnLevelChanged;
        }
    }

    /// <summary>
    /// Обработчик события изменения опыта.
    /// </summary>
    /// <param name="amount">Количество добавленного опыта.</param>
    private void OnExperienceChanged(float amount)
    {
        UpdateExperienceBar();
    }

    /// <summary>
    /// Обработчик события повышения уровня.
    /// </summary>
    /// <param name="newLevel">Новый уровень игрока.</param>
    private void OnLevelChanged(int newLevel)
    {
        UpdateExperienceBar();
    }

    /// <summary>
    /// Немедленное обновление полоски опыта без анимации.
    /// Используется для инициализации.
    /// </summary>
    private void UpdateExperienceBarImmediate()
    {
        if (LevelManager.Instance == null || fillImage == null)
            return;

        float currentExperience = LevelManager.Instance.GetCurrentExperience();
        float maxExperience = LevelManager.Instance.GetMaxExperience();

        float fillAmount = Mathf.Clamp01(currentExperience / maxExperience);
        fillImage.fillAmount = fillAmount;
    }

    /// <summary>
    /// Обновляет визуальное отображение полоски опыта с анимацией.
    /// </summary>
    private void UpdateExperienceBar()
    {
        if (LevelManager.Instance == null || fillImage == null)
            return;

        float currentExperience = LevelManager.Instance.GetCurrentExperience();
        float maxExperience = LevelManager.Instance.GetMaxExperience();

        float targetFillAmount = Mathf.Clamp01(currentExperience / maxExperience);

        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
        }

        fillCoroutine = StartCoroutine(AnimateFill(fillImage.fillAmount, targetFillAmount));
    }

    /// <summary>
    /// Корутина для анимации заполнения полоски опыта.
    /// </summary>
    /// <param name="start">Начальное значение fillAmount.</param>
    /// <param name="end">Целевое значение fillAmount.</param>
    /// <returns></returns>
    private IEnumerator AnimateFill(float start, float end)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            // Можно использовать плавность при помощи Mathf.SmoothStep или анимационных кривых
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            fillImage.fillAmount = Mathf.Lerp(start, end, smoothT);
            yield return null;
        }

        fillImage.fillAmount = end;
        fillCoroutine = null;
    }
}