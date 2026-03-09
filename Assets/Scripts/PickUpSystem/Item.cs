using Inventory.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Item : MonoBehaviour
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private float duration = 0.3f;

    private bool canBePickedUp = true;  

    [SerializeField]
    private string itemID;

    public string ItemID => itemID;

    private void Awake()
    {
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = Guid.NewGuid().ToString();
            Debug.LogWarning("ItemID не установлен. Генерируется новый ID.");
        }
    }

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = InventoryItem.ItemImage;
        if (SaveManager.Instance.IsItemPicked(itemID))
        {
            Destroy(gameObject);
        }
    }

    public void DestroyItem()
    {
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(AnimateItemPickup());
    }

    private IEnumerator AnimateItemPickup()
    {
        audioSource.Play();
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            transform.localScale = 
                Vector3.Lerp(startScale, endScale, currentTime / duration);
            yield return null;
        }
        // Сохраняем, что предмет был подобран
        SaveManager.Instance.PickUpItem(itemID);
        Destroy(gameObject);
    }

    public void SetItem(ItemSO itemSO, int quantity, string existingItemID = null)
    {
        InventoryItem = itemSO;
        Quantity = quantity;
        GetComponent<SpriteRenderer>().sprite = InventoryItem.ItemImage;

        if (!string.IsNullOrEmpty(existingItemID))
        {
            itemID = existingItemID;
        }
        else
        {
            itemID = Guid.NewGuid().ToString();
            Debug.Log("Установлен новый ItemID.");
        }

        canBePickedUp = false; // Запрещаем подбор сразу после сброса
    }

    // Метод для установки itemID вручную
    public void SetItemID(string id)
    {
        if (string.IsNullOrEmpty(itemID))
        {
            itemID = id;
            Debug.Log($"ItemID установлен вручную: {itemID}");
        }
        else
        {
            Debug.LogWarning("Попытка установить ItemID, уже установленный ID.");
        }
    }

    // Метод вызывается, когда игрок выходит из триггера
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canBePickedUp = true; // Разрешаем подбор после выхода игрока
        }
    }

    // Публичное свойство для проверки, может ли предмет быть подобран
    public bool CanBePickedUp => canBePickedUp;
}