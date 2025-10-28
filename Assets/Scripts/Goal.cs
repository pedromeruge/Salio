using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null) return;

        if (other.gameObject == GameManager.Instance.ball)
        {
            GameManager.Instance.OnGoalReached();
        }
    }
}