using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsGamePaused { get; private set; } = false;
    public short currentLevel = 1;


    void Awake()
    {
        //singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void startLevel()
    {
        // add stuff
    }
    
    public void endLevel()
    {
        Debug.Log("Level " + currentLevel + " completed!");
        currentLevel++;
        // add stuff
    }
}
