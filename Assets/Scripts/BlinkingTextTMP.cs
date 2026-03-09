using UnityEngine;
using TMPro;
using System.Collections;

public class BlinkingTextTMP : MonoBehaviour
{
    [SerializeField] private float blinkInterval = 0.5f; // Интервал мигания в секундах
    [SerializeField] private bool useSmoothBlink = false; // Использовать ли плавное мигание
    private TextMeshProUGUI tmpText;
    private bool isVisible = true;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        if (tmpText == null)
        {
            Debug.LogError("Компонент TextMeshProUGUI не найден на объекте.");
        }
    }

    private void OnEnable()
    {
        if (tmpText == null) return;

        // Сброс состояния при активации
        isVisible = true;
        tmpText.enabled = true;
        Color color = tmpText.color;
        color.a = 1f;
        tmpText.color = color;

        // Запуск корутины
        if (useSmoothBlink)
        {
            blinkCoroutine = StartCoroutine(SmoothBlink());
        }
        else
        {
            blinkCoroutine = StartCoroutine(Blink());
        }
    }

    private void OnDisable()
    {
        // Остановка корутины при деактивации
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        // Сброс видимости
        Color color = tmpText.color;
        color.a = 1f;
        tmpText.color = color;
    }

    /// <summary>
    /// Простое мигание путем включения и отключения видимости текста.
    /// </summary>
    private IEnumerator Blink()
    {
        while (true)
        {
            isVisible = !isVisible;
            Color color = tmpText.color;
            color.a = isVisible ? 1f : 0f;
            tmpText.color = color;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    /// <summary>
    /// Плавное мигание путем изменения альфа-канала цвета текста.
    /// </summary>
    private IEnumerator SmoothBlink()
    {
        while (true)
        {
            // Плавно уменьшаем альфа до 0
            for (float t = 0; t < blinkInterval; t += Time.deltaTime)
            {
                Color color = tmpText.color;
                color.a = Mathf.Lerp(1f, 0f, t / blinkInterval);
                tmpText.color = color;
                yield return null;
            }

            // Плавно увеличиваем альфа до 1
            for (float t = 0; t < blinkInterval; t += Time.deltaTime)
            {
                Color color = tmpText.color;
                color.a = Mathf.Lerp(0f, 1f, t / blinkInterval);
                tmpText.color = color;
                yield return null;
            }
        }
    }
}