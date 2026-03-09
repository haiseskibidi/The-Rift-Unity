using UnityEngine;
using UnityEngine.UI;

public class StartNewGameButton : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    private void OnButtonClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
            
            // Обновляем доступность кнопки "Continue"
            UpdateContinueButton();
        }
    }

    private void UpdateContinueButton()
    {
        // Найти кнопку "Continue" в сцене и обновить её состояние
        ContinueGameButton continueButton = FindObjectOfType<ContinueGameButton>();
    }
}