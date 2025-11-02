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
        SceneManager.LoadSceneAsync("single_maze");
    }

}
