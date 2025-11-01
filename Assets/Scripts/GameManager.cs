using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]

    public Text messageText;
    public Text timerText;

    [Header("Messages")]
    [TextArea] public string startMessage = "Level Start!";
    [TextArea] public string endMessage = "Level Complete!";

    [Header("Settings")]
    public float messageDisplayTime = 3f;

    public bool IsGamePaused { get; private set; } = false;
    public short currentLevel = 1;

    private bool levelRunning = false;
    private float levelTimer = 0f;

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
            timerText.text = "0";
            timerText.gameObject.SetActive(true);
        }

        startLevel();
    }

    private void Update()
    {
        if (levelRunning)
        {
            levelTimer += Time.deltaTime;

            if (timerText != null)
            {
                int displaySeconds = Mathf.FloorToInt(levelTimer);
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
        levelTimer = 0f;
        levelRunning = false;

        yield return ShowMessage(startMessage);

        levelRunning = true;
    }

    public void endLevel()
    {
        if (!levelRunning) return;

        levelRunning = false;
        Debug.Log("Level " + currentLevel + " completed in " + Mathf.FloorToInt(levelTimer) + " seconds!");
        currentLevel++;

        StartCoroutine(LevelEndRoutine());
    }

    private IEnumerator LevelEndRoutine()
    {
        yield return ShowMessage(endMessage);
        startLevel();
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
        // add logic to only consider level ended when all balls have reached their respective goal (in levels where that happens)
        endLevel();
    }
}

