using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public Text messageText;
    public Text timerText;

    [Header("Messages")]
    [TextArea] public string startMessage = "Go!";
    [TextArea] public string endMessage = "You Win!";
    [TextArea] public string loseMessage = "Game Over!";

    [Header("Settings")]
    public float messageDisplayTime = 3f;

    public bool IsGamePaused { get; private set; } = false;
    public short currentLevel = 1;

    private bool levelRunning = false;
    private bool levelCompleted = false;
    private float levelTimer = 60f;
    [SerializeField] private string sceneToLoad;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(levelTimer).ToString();
            timerText.gameObject.SetActive(true);
        }

        startLevel();
    }

    private void Update()
    {
        if (levelRunning)
        {
            levelTimer -= Time.deltaTime;

            if (levelTimer <= 0f)
            {
                levelTimer = 0f;
                levelRunning = false;

                if (!levelCompleted)
                    StartCoroutine(LevelLoseRoutine());
            }

            if (timerText != null)
            {
                int displaySeconds = Mathf.CeilToInt(levelTimer);
                timerText.text = displaySeconds.ToString();
            }
        }
    }

    public void startLevel()
    {
        StartCoroutine(LevelStartRoutine());
    }

    private IEnumerator LevelStartRoutine()
    {
        levelTimer = 60f;
        levelCompleted = false;
        levelRunning = false;

        yield return ShowMessage(startMessage);

        levelRunning = true;
    }

    public void endLevel()
    {
        if (!levelRunning) return;

        levelRunning = false;
        levelCompleted = true;

        Debug.Log("Level " + currentLevel + " completed in " + Mathf.FloorToInt(60f - levelTimer) + " seconds!");
        currentLevel++;

        StartCoroutine(LevelEndRoutine());
    }

    private IEnumerator LevelEndRoutine()
    {
        yield return ShowMessage(endMessage);

        if (sceneToLoad == null) sceneToLoad = "MainMenu";

        StartCoroutine(Transition(sceneToLoad));
    }

    private IEnumerator LevelLoseRoutine()
    {
        Debug.Log("Level failed! Timer reached zero.");
        yield return ShowMessage(loseMessage);

        if (sceneToLoad == null) sceneToLoad = "MainMenu";
        StartCoroutine(Transition(sceneToLoad));
    }

    private IEnumerator ShowMessage(string message)
    {
        if (messageText == null)
        {
            Debug.LogWarning("Message Text UI not assigned!");
            yield break;
        }

        messageText.text = message;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDisplayTime);
        messageText.gameObject.SetActive(false);
    }

    public void OnGoalReached()
    {
        if (!levelCompleted) // avoid triggering end multiple times
            endLevel();
    }

    private IEnumerator Transition(string sceneName)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName.ToLower(), LoadSceneMode.Single);
        while (!loadOp.isDone) yield return null;
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(Transition(sceneName));
    }
}



