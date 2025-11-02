using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void TransitionToLevel()
    {
        StartCoroutine(Transition(sceneToLoad));
    }

    private IEnumerator Transition(string sceneName)
    {

        // wait for the new scene to be loaded
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName.ToLower(), LoadSceneMode.Single);
        while (!loadOp.isDone) yield return null;

    }
}
