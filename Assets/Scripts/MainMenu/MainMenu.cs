using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("single_maze");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
