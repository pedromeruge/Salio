using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("single_maze_easy");
    }

    public void LevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void HowToPlay()
    {
        SceneManager.LoadScene("HowToPlay");
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
