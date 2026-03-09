using UnityEngine;

public class ShrekSignInteract : MonoBehaviour
{
    [SerializeField] private GameObject box; // Коробка, в которой висит Шрек
    [SerializeField] private ShrekController shrekController; // Ссылка на ShrekController

    private bool isPlayerNearby = false;
    private PressFManager pressFManager;

    private void Start()
    {
        if (box != null)
            box.SetActive(true); // Убедитесь, что коробка активна в начале

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
            HandleInteraction();
        }
    }

    private void HandleInteraction()
    {
        // Скрыть коробку через ShrekController
        if (shrekController != null)
        {
            shrekController.ActivateShrek();
        }

        // Скрыть интерактивную подсказку
        if (pressFManager != null)
        {
            pressFManager.HideCanvas();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (pressFManager != null)
            {
                pressFManager.ShowCanvas(); // Показать подсказку "Нажмите F"
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
                pressFManager.HideCanvas(); // Скрыть подсказку
            }
        }
    }
}