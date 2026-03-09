using UnityEngine;

public class ChangeSceneOnInteract : MonoBehaviour
{
    public string sceneToLoad = "NextScene";
    public string targetDoorID = "Door..1"; // Уникальный идентификатор целевой двери
    private bool isPlayerNearby = false;
    private PressFManager pressFManager;

    private void Start()
    {
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
            if (pressFManager != null)
            {
                pressFManager.HideCanvas();
            }

            GameManager.Instance.SetTargetDoor(targetDoorID); // Устанавливаем целевую дверь
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                Vector2 playerPosition = player.transform.position;
                GameManager.Instance.ChangeScene(sceneToLoad, playerPosition);
            }
            else
            {
                Debug.LogWarning("Player не найден для смены сцены.");
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (pressFManager != null)
            {
                pressFManager.ShowCanvas(); // Включаем PressFCanvas при подходе
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
                pressFManager.HideCanvas(); // Выключаем PressFCanvas при уходе
            }
        }
    }
}