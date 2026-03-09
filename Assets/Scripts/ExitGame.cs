using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();
        Debug.Log("Игра завершена.");
    }
}
