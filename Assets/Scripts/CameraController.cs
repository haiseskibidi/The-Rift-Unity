using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private float cameraSpeed = 2f; // Скорость камеры
    [SerializeField] private float yOffset = 1.5f; // Смещение по Y
    private Vector3 targetPosition;
    private bool isInitialized = false; // Флаг для инициализации позиции камеры

    private void Awake()
    {
        if (Player == null)
        {
            Player playerCharacter = FindFirstObjectByType<Player>();
            if (playerCharacter != null)
            {
                Player = playerCharacter.transform;
            }
            else
            {
                Debug.LogError("Игрок не найден в сцене.");
            }
        }
    }

    private void Start()
    {
        InitializeCameraPosition();
    }

    private void InitializeCameraPosition()
    {
        if (Player != null)
        {
            float PlayerY = Player.position.y;
            float targetY = PlayerY + yOffset; // Камера следует за игроком по Y
            targetPosition = new Vector3(Player.position.x, targetY, -10f); // Устанавливаем позицию камеры

            transform.position = targetPosition; // Немедленно перемещаем камеру
            isInitialized = true; // Устанавливаем флаг инициализации
        }
    }

    private void LateUpdate()
    {
        if (Player == null)
            return;

        if (!isInitialized)
        {
            InitializeCameraPosition();
            return;
        }

        float PlayerY = Player.position.y;
        float targetY = PlayerY + yOffset; // Камера следует за игроком по Y

        targetPosition = new Vector3(Player.position.x, targetY, -10f); // Меняем только по X и Y, фиксируем Z
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraSpeed * Time.deltaTime);
    }
}