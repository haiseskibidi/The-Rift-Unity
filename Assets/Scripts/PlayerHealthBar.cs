using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Player playerHealth;
    [SerializeField] private Image totalHealthBar;
    [SerializeField] private Image currentHealthBar;

    private void Start()
    {
        UpdateHealthBars();
    }

    private void Update()
    {
        UpdateHealthBars();
    }

    /// <summary>
    /// Обновляет заполнение полос здоровья.
    /// </summary>
    private void UpdateHealthBars()
    {
        if (playerHealth != null)
        {
            float maxHealth = playerHealth.StartingHealth; // Получение максимального здоровья
            totalHealthBar.fillAmount = 1f; // Полная заполненность для totalHealthBar
            currentHealthBar.fillAmount = playerHealth.currentHealth / maxHealth;
        }
    }
}