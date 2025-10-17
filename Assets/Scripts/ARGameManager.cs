using System.Collections.Generic;
using UnityEngine;

public class ARGameManager : MonoBehaviour
{
    public static ARGameManager Instance;

    private readonly HashSet<MazeController> activeMazes = new HashSet<MazeController>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void OnMazeTrackingChanged(MazeController maze, bool isTracked)
    {
        if (isTracked)
            activeMazes.Add(maze);
        else
            activeMazes.Remove(maze);

        // globally pause game when one of actively tracked mazes is lost
        Time.timeScale = activeMazes.Count > 0 ? 1f : 0f;
    }
}