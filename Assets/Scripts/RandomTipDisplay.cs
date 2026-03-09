   using UnityEngine;
   using TMPro;
   using System.Collections.Generic;

   public class RandomTipDisplay : MonoBehaviour
   {
       public TextMeshProUGUI tipText;
       public List<string> tips = new List<string>();

       private List<string> remainingTips;

       private void Start()
       {
           // Копируем все советы в список оставшихся советов
           remainingTips = new List<string>(tips);
           DisplayRandomTip();
       }

       public void DisplayRandomTip()
       {
           if (remainingTips.Count == 0)
           {
               // Если все советы были показаны, восстанавливаем список
               remainingTips = new List<string>(tips);
           }

           // Выбираем случайный индекс
           int randomIndex = Random.Range(0, remainingTips.Count);
           // Устанавливаем текст
           tipText.text = remainingTips[randomIndex];
           // Удаляем выбранный совет из списка оставшихся
           remainingTips.RemoveAt(randomIndex);
       }
   }