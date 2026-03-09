using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadLobby : MonoBehaviour
{
    AsyncOperation asyncOperation;
    public Image LoadBar;
    public TextMeshProUGUI BarTxt;

    private void Start()
    {
        string lastSceneName = PlayerPrefs.GetString("LastSceneName", GameManager.Instance.LastSceneName);
        StartCoroutine(LoadSceneCor(lastSceneName));
    }

    IEnumerator LoadSceneCor(string sceneName)
    {
        yield return new WaitForSeconds(3f);
        asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOperation.isDone)
        {
            float progress = asyncOperation.progress / 0.9f;
            LoadBar.fillAmount = progress;
            BarTxt.text = "Loading " + string.Format("{0:0}%", progress * 100f);
            yield return null;
        }
    }
}