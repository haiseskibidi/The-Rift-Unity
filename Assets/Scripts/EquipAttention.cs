using UnityEngine;
using System.Collections;

public class EquipAttention : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    private float attentionDuration = 2f; 
    private Coroutine attentionCoroutine;
    private Player player;

    private void OnEnable()
    {
        // Подписываемся на событие изменения состояния UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIStateChanged += HandleUIStateChanged;
        }
    }

    private void OnDisable()
    {
        // Отписываемся от события при отключении объекта
        if (UIManager.Instance != null)
        {
            UIManager.Instance.OnUIStateChanged -= HandleUIStateChanged;
        }
    }

    /// <summary>
    /// Инициализация EquipAttention с привязкой к игроку.
    /// </summary>
    /// <param name="playerInstance">Экземпляр класса Player.</param>
    public void Initialize(Player playerInstance)
    {
        player = playerInstance;
    }

    /// <summary>
    /// Показать внимание к экипировке, если UIManager позволяет.
    /// </summary>
    public void ShowAttention()
    {
        if (UIManager.Instance.IsAnyUIOpen())
        {
            Debug.Log("Не удалось показать EquipAttention, так как другой UI уже открыт.");
            return;
        }

        if (attentionCoroutine != null)
        {
            StopCoroutine(attentionCoroutine); // Останавливаем текущую корутину, если она запущена
        }
        attentionCoroutine = StartCoroutine(AttentionRoutine());
    }

    private IEnumerator AttentionRoutine()
    {
        canvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(attentionDuration);
        canvas.gameObject.SetActive(false);
        attentionCoroutine = null;
    }

    /// <summary>
    /// Обработчик изменения состояния UI.
    /// </summary>
    /// <param name="newState">Новое состояние UI.</param>
    private void HandleUIStateChanged(UIState newState)
    {
        if (newState != UIState.None && canvas.gameObject.activeSelf)
        {
            // Если другой UI открывается, скрываем EquipAttention
            StopAllCoroutines();
            canvas.gameObject.SetActive(false);
            attentionCoroutine = null;
        }
    }
}