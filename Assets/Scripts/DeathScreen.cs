using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public void RestartGame()
    {
        // Закрытие всех открытых UI через UIManager
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseCurrentUI();
        }

        // Найти объект Player в сцене
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            GameManager.Instance.RespawnPlayer();
        }
    }

    private void OnEnable()
    {
        // Открываем состояние меню в UIManager при активации DeathScreen
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RequestOpenUI(UIState.DeathScreen);
        }
    }

    private void OnDisable()
    {
        // Закрываем текущее состояние UI при деактивации DeathScreen
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CloseCurrentUI();
        }
    }
}