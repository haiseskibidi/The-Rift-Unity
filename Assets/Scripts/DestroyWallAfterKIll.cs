using UnityEngine;

public class DestroyWallAfterKill : MonoBehaviour
{
    [SerializeField] private string enemyID; 

    private void Start()
    {
        if (string.IsNullOrEmpty(enemyID))
        {
            Debug.LogError("Enemy ID не назначен в DestroyWallAfterKill.");
            return;
        }

        // Проверка, был ли враг убит при загрузке сцены
        if (GameManager.Instance.IsEnemyKilled(enemyID))
        {
            DestroyWall();
        }
        else
        {
            // Подписка на событие убийства врага
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnEnemyKilled += HandleEnemyKilled;
            }
        }
    }

    private void HandleEnemyKilled(string killedEnemyID)
    {
        if (killedEnemyID == enemyID)
        {
            Debug.Log($"Враг с ID {enemyID} убит. Разрушаем стену.");
            DestroyWall();
        }
    }

    private void DestroyWall()
    {
        Debug.Log($"Стена уничтожена, так как враг с ID: {enemyID} был убит.");
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // Отписка от события
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnEnemyKilled -= HandleEnemyKilled;
        }
    }
}