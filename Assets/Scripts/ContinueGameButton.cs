using UnityEngine;
using UnityEngine.UI;

public class ContinueGameButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }

        // Проверка состояния игры при инициализации
        UpdateButtonInteractable();
    }

    private void OnButtonClick()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.HasStartedNewGame)
            {
                GameManager.Instance.ContinueGame();
            }
            else
            {
                Debug.LogWarning("Сначала начните новую игру.");
            }
        }
    }

    private void UpdateButtonInteractable()
    {
        if (GameManager.Instance != null)
        {
            button.interactable = GameManager.Instance.HasStartedNewGame;
        }
    }
}