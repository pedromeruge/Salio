using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Ball")
        {
            GameManager.Instance.OnGoalReached(gameObject.GetInstanceID());
            Debug.Log("Goal reached by " + gameObject.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ball")
        {
            GameManager.Instance.OnGoalExited(gameObject.GetInstanceID());
            Debug.Log("Goal exited by " + gameObject.name);
        }
    }
}