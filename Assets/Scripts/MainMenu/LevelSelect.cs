using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelect : MonoBehaviour
{
    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void TransitionToLevelEasy()
    {
        SceneManager.LoadSceneAsync("single_maze_easy");
    }

    public void TransitionToLevelMedium()
    {
        SceneManager.LoadSceneAsync("single_maze_medium");
    }

    public void TransitionToLevelHard()
    {
        SceneManager.LoadSceneAsync("single_maze");
    }

    public void TransitionToLevelExtra()
    {
        SceneManager.LoadSceneAsync("two_mazes");
    }

}
