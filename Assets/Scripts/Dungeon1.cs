using UnityEngine;
using UnityEngine.SceneManagement;

public class Dungeon1 : MonoBehaviour
{
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
