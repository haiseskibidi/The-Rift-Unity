using UnityEngine;

public class ToggleUIOnInteract : MonoBehaviour
{
    [SerializeField] private GameObject canvas1;
    [SerializeField] private GameObject canvas2;
    [SerializeField] private Player player;

    private bool isPlayerNearby = false;
    private bool isUIActive = false;
    private PressFManager pressFManager;

    private void Start()
    {
        if (canvas1 != null)
            canvas1.SetActive(false);
        if (canvas2 != null)
            canvas2.SetActive(false);

        pressFManager = FindObjectOfType<PressFManager>();
        if (pressFManager == null)
        {
            Debug.LogError("PressFManager не найден в сцене.");
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.F))
        {
            if (!isUIActive)
            {
                // Запрос на открытие UI через UIManager
                if (UIManager.Instance.RequestOpenUI(UIState.UIOnInteract))
                {
                    ToggleUI();
                }
            }
            else
            {
                ToggleUI();
                UIManager.Instance.CloseCurrentUI();
            }

            if (pressFManager != null)
            {
                pressFManager.HideCanvas();
            }
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isUIActive)
        {
            ToggleUI();
            UIManager.Instance.CloseCurrentUI();
            UIManager.Instance.HandleEsc();
        }
    }

    private void ToggleUI()
    {
        isUIActive = !isUIActive;

        if (canvas1 != null)
            canvas1.SetActive(isUIActive);
        if (canvas2 != null)
            canvas2.SetActive(isUIActive);

        if (player != null)
            player.enabled = !isUIActive;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (pressFManager != null)
            {
                pressFManager.ShowCanvas();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (pressFManager != null)
            {
                pressFManager.HideCanvas();
            }

            if (isUIActive)
            {
                ToggleUI();
                UIManager.Instance.CloseCurrentUI();
            }
        }
    }
}