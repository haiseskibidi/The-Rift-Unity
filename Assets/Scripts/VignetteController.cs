 // Start of Selection
 using System.Collections;
 using UnityEngine;
 using UnityEngine.Rendering.PostProcessing;
  
 public class VignetteController : MonoBehaviour
 {
     // Ссылка на объем пост-обработки
     public PostProcessVolume postProcessVolume;
  
     // Ссылка на эффект виньетки
     private Vignette vignette;
  
     // Корутина для плавного изменения виньетки
     private Coroutine vignetteCoroutine;
  
     void Start()
     {
         if (postProcessVolume != null)
         {
             // Получаем эффект виньетки из объема пост-обработки
             postProcessVolume.profile.TryGetSettings(out vignette);
             if (vignette != null)
             {
                 // Устанавливаем начальную интенсивность
                 vignette.intensity.value = 0f;
                 vignette.active = true;
             }
         }
     }
  
     // Метод для увеличения интенсивности виньетки плавно
     public void EnableVignette()
     {
         if (vignette != null)
         {
             // Останавливаем текущую корутину, если она есть
             if (vignetteCoroutine != null)
             {
                 StopCoroutine(vignetteCoroutine);
             }
             // Запускаем корутину для плавного увеличения интенсивности
             vignetteCoroutine = StartCoroutine(TransitionVignette(0.55f, 1f));
         }
     }
  
     // Метод для уменьшения интенсивности виньетки плавно
     public void DisableVignette()
     {
         if (vignette != null)
         {
             // Останавливаем текущую корутину, если она есть
             if (vignetteCoroutine != null)
             {
                 StopCoroutine(vignetteCoroutine);
             }
             // Запускаем корутину для плавного уменьшения интенсивности
             vignetteCoroutine = StartCoroutine(TransitionVignette(0f, 1f));
         }
     }
  
     // Корутин для плавного изменения интенсивности виньетки
     private IEnumerator TransitionVignette(float targetIntensity, float duration)
     {
         float startIntensity = vignette.intensity.value;
         float elapsed = 0f;
  
         while (elapsed < duration)
         {
             vignette.intensity.value = Mathf.Lerp(startIntensity, targetIntensity, elapsed / duration);
             elapsed += Time.deltaTime;
             yield return null;
         }
         vignette.intensity.value = targetIntensity;
     }
  
     // Метод вызывается при входе коллайдера
     private void OnTriggerEnter2D(Collider2D other)
     {
         if (other.CompareTag("Player"))
         {
             // Проверяем, какой коллайдер активирован, и включаем или отключаем виньетку
             if (gameObject.CompareTag("EnableVignette"))
             {
                 EnableVignette();
             }
             else if (gameObject.CompareTag("DisableVignette"))
             {
                 DisableVignette();
             }
         }
     }
 }