using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _returnToMainMenuButton;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _settingsButton; // Пока что без реализации

    private bool isMenuOpen = false;

    private void Start()
    {
        // Изначально меню закрыто
        _panel.SetActive(false);

        // Привязка методов к кнопкам
        _returnToMainMenuButton.onClick.AddListener(ReturnToMainMenu);
        _continueButton.onClick.AddListener(CloseMenu);
        // settingsButton.onClick.AddListener(OpenSettings); // Будет реализовано позже
    }

    private void Update()
    {
        if(!UIManager.Instance.IsAnyUIOpen() && !UIManager.Instance.IsEscHandled)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OpenMenu();
            }
        }
    }

    private void OpenMenu()
    {
        if (UIManager.Instance.RequestOpenUI(UIState.Menu))
        {
            isMenuOpen = true;
            _panel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void CloseMenu()
    {
        isMenuOpen = false;
        _panel.SetActive(false);
        Time.timeScale = 1f;
        UIManager.Instance.CloseCurrentUI();
    }

    private void ReturnToMainMenu()
    {
        GameManager.Instance.SaveGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
        CloseMenu();
        UIManager.Instance.CloseCurrentUI();
    }

    // private void OpenSettings()
    // {
    //     
    // }
}