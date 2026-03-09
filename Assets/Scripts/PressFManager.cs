using UnityEngine;
using UnityEngine.SceneManagement;

public class PressFManager : MonoBehaviour
{
    [SerializeField] private GameObject interactionCanvas;

    private void Awake()
    {
        if (interactionCanvas != null)
        {
            interactionCanvas.SetActive(false); 
        }
        else
        {
            Debug.LogError("PressFCanvas не назначен в инспекторе.");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HideCanvas(); 
    }

    public void ShowCanvas()
    {
        if (interactionCanvas != null && !interactionCanvas.activeSelf)
        {
            interactionCanvas.SetActive(true);
        }
    }

    public void HideCanvas()
    {
        if (interactionCanvas != null && interactionCanvas.activeSelf)
        {
            interactionCanvas.SetActive(false);
        }
    }
}