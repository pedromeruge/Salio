using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.Instance == null) return;

        if (other.gameObject.tag == "Ball")
        {
            GameManager.Instance.OnGoalReached();
        }
    }
}