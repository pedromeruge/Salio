using Unity.VisualScripting;
using UnityEngine;

public class DebugGravity : MonoBehaviour
{
    [SerializeField] float vectorHeight = 0.5f;
    private Transform mazeTransform;
    void Awake()
    {
        mazeTransform = this.transform;
    }
    void OnDrawGizmos()
    {
        if (mazeTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(mazeTransform.position, mazeTransform.position - mazeTransform.up * vectorHeight);
        }
    }
}
