using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class InstantDeathMessageDisplay : MonoBehaviour
{
    public TextMeshProUGUI messageText; // Ссылка на компонент TextMeshProUGUI
    public List<string> messages = new List<string>(); // Список сообщений

    private List<string> remainingMessages;

    private void Awake()
    {
        remainingMessages = new List<string>(messages);
        gameObject.SetActive(false); // Изначально объект не активен
    }

    public void ShowRandomMessage()
    {
        if (remainingMessages.Count == 0)
        {
            remainingMessages = new List<string>(messages);
        }

        int randomIndex = Random.Range(0, remainingMessages.Count);
        messageText.text = remainingMessages[randomIndex];
        remainingMessages.RemoveAt(randomIndex);

        gameObject.SetActive(true); // Активируем объект для отображения сообщения
    }
}